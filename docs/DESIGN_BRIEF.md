# DESIGN BRIEF - Realm of Goryo: Shadow of Giants (Vertical Slice)

*Status: v1.0, locked 2026-07-18 after the studio intake interview (owner answers: best-of-3 rounds; Khulandra event between rounds 1 and 2; generate all audio now). This document is the build prompt for the Unity project in `game/`. It implements Vision Goal 1 (docs/VISION.md section 7). Style rule for this doc: regular hyphens only.*

Build a local Unity project for Android in `game/`. Reuse the 41 placeholder assets already generated (docs/ASSET_MANIFEST.md); generate only the remaining animation clips and audio with Higgsfield. Do not deploy or publish anything - local APK builds only (CLAUDE.md ground rule 3).

## TITLE

**Realm of Goryo: Shadow of Giants** - vertical slice "Harbor Ruins". Internal codename Kaiju Ruin.

## ONE-LINE PITCH

Two champions of rival futures duel on a drowned pier while a god-sized kaiju reshapes the fight - a mature, ink-heavy comic-book fighting game built for one thumb.

## GENRE AND CORE LOOP

Single-player 2D fighting game (rigged 3D characters locked to a 2D gameplay plane, side-locked camera). The player is Kest; the AI is Tengi. Best-of-3 rounds, 60 seconds each. Second to second: read the opponent, tap out light chains, swipe for heavies and launchers, hold to block, spend meter on ability cards. Between rounds 1 and 2, Khulandra breaches the harbor and floods the stage. Match point triggers a Horrific Ending splash panel. A three-panel motion-comic opens the session.

## TARGET / ENGINE

- Unity 6 LTS (6000.0.x), URP, Android, landscape (sensor landscape), minimum API level 26, ARM64.
- Scripting backend: Mono for fast local test builds; IL2CPP + ARM64 for release-grade APK (both documented in README).
- Performance bars (Vision Pillar 7 / Goal 2): 60 fps on Snapdragon 695-class hardware, input-to-impact under 80 ms, install under 300 MB.
- Package: com.nyalia.kaijuruin (product name "Shadow of Giants").
- BUSINESS MODEL: FREE (owner decision 2026-07-18, D-012). No purchases, no ads, no monetization code in the slice. Pillar 6 stands for any future model work.

## TOUCH CONTROLS (Vision Pillar 3: one thumb, real decisions)

Screen is split into interaction zones. Every gesture routes to a named C# method on PlayerController.

**Amended session 10, D-024 - the left thumb also carries STANCE, and the right thumb's swipe ring is eight-way.** The left half's horizontal drag still walks; holding *away* past half deflection also raises a standing guard (back is both retreat and block), holding *down-and-away* is the crouch guard, and holding *down* is a crouch (a stance, since it modifies attacks) - all pushed to `PlayerController.SetStance(bool crouch, bool away)` every frame the finger is down. The right half's four swipe directions became eight; the four diagonals carry the overhead claw slam, the haunch bash, the command grab and the leg sweep (`TouchInput.DiagonalRatio` = 0.55 decides diagonal vs cardinal). The right-thumb hold remains the committed, rooted, parry-armed guard - the only guard that can parry.

- LEFT HALF, horizontal drag: walk toward / away from the opponent (PlayerController.Move(float axis)). Drag right of the touch origin walks forward, left walks back. Releasing stops.
- RIGHT HALF, tap: light attack; consecutive taps chain up to 3 hits (PlayerController.TapAttack()).
- RIGHT HALF, swipe toward opponent: heavy attack (PlayerController.HeavyAttack()).
- RIGHT HALF, swipe up: launcher (PlayerController.Launcher()).
- RIGHT HALF, swipe down: sweep (PlayerController.Sweep()).
- RIGHT HALF, swipe away from opponent: evasive back-dash with i-frames (PlayerController.BackDash()) - session 5, D-016 (was a no-op).
- RIGHT HALF, tap / swipe up while the opponent is airborne: air-juggle follow-ups (Air Rake / Air Slam) - session 5, D-015; contextual, no new gesture.
- RIGHT HALF, press and hold (no movement): block while held; a block that connects within 160 ms of its start is a parry (PlayerController.SetBlock(bool held)) - session 5, D-015.
- BOTTOM-RIGHT: three ability card buttons labeled with card art (ui_ability_icons_kest tiles); tap spends meter (PlayerController.CastSpecial(int slot)).
- TOP-RIGHT: PAUSE button (GameManager.TogglePause()).
- Input-to-impact: gesture recognition resolves on touch-up or 120 ms of hold, whichever is first; attack anims cancel their first 2 frames into the next buffered input.

## MOVES (name / trigger / behavior / numbers)

**Reach numbers below are baseline (Kest) values — amended session 9, D-023.** Each move's listed reach is a centre-to-centre distance quoted for Kest's measured body (0.78 m knuckle reach + 0.32 m hurt half-depth = the 1.10 m jab), and the fight resolves it per pairing as `reach + (attacker.ArmReach − 0.78) + (target.HurtDepth − 0.32)`. Tengi measures 1.14 m / 0.46 m off his GLB, so he strikes and is struck from further out (his jab connects at 1.46 m against Kest). Bodies are solid: two fighters never close past the sum of their push depths (0.56–0.72 m). Per-champion metrics live in `CharacterRoster.CharacterDef`; adding a champion means measuring its model.

Both champions: 1000 HP, meter of 3 segments. **Amended session 11, D-025:** a segment charges per **~56 damage dealt or ~60 taken** (was 150 / ~160) - the rates carry the reciprocal of the damage cut, so a segment still costs the same NUMBER OF HITS as it always did (~3.8 landed jabs, ~4.0 taken). The on-taken rate stays **deliberately halved** relative to dealt so defense does not over-generate meter; ratified as **D-018**, unchanged in hit terms by D-025.

| # | Move | Trigger | Behavior |
|---|---|---|---|
| 1 | Light chain 1 (Jab) | tap | 15 dmg, 1.1 m reach, 0.25 s recovery, chains within 0.6 s |
| 2 | Light chain 2 (Cross) | tap tap | 21 dmg, same reach, chains |
| 3 | Light chain 3 (Finisher) | tap tap tap | 28 dmg, small knockback, 0.5 s recovery |
| 4 | Heavy | swipe toward | 45 dmg, 1.6 m reach, 0.8 s recovery, knockback 1.5 m |
| 5 | Launcher | swipe up | 34 dmg, pops the opponent airborne 0.7 s (juggle: one free light) |
| 6 | Sweep | swipe down | 30 dmg, low hit, beats standing block |
| 7 | Block | hold | reduces damage 75 percent, 10 percent chip on specials, cannot attack while held |
| 8 | Kest S1: Fox-fire Dash | card 1 (1 seg) | gap-closing dash strike, 38 dmg, plays vfx_kest_foxfire |
| 9 | Kest S2: Phantom Rake | card 2 (2 seg) | three-hit phantom claw combo, 60 dmg total |
| 10 | Kest S3: Hunt of Shadows | card 3 (3 seg) | cinematic: ring of fox shadows, 105 dmg, brief slowdown |
| 11 | Tengi S1: Crow Wall | card 1 (1 seg) | 1.2 s counter stance; countered hit answers for 49 dmg (vfx feathers) |
| 12 | Tengi S2: Culling Arc | card 2 (2 seg) | horizontal blade wave, 68 dmg, plays vfx_tengi_bladewave |
| 13 | Tengi S3: Black Sun | card 3 (3 seg) | slow overhead execution arc, 112 dmg, huge recovery on whiff |

## MOVES v2 - combat expansion (session 5, D-015 - owner directive "lean into the fighter")

Layered onto the SAME one-thumb control scheme; the only new gesture binding is the previously-dead swipe-away (D-016). Core loop, architecture, HP/meter, and environment art are unchanged. Universal-normal recoveries were trimmed for a faster fight (jab/cross 0.20s, finisher 0.38s, heavy 0.62s, launcher/sweep 0.50s) and walk speed raised to 3.0 (Kest 3.1 / Tengi 2.8); damage numbers and meter economy are unchanged.

| # | Move | Trigger | Behavior |
|---|---|---|---|
| 14 | Back-dash (Evade) | swipe away from opponent | ~1.2 m backward hop, i-frames 0.24 s, recovery 0.30 s, dust VFX; neutral-only (not a recovery-cancel). Resolves the away-swipe reservation. |
| 15 | Air Rake | tap while opponent is airborne | 21 dmg juggle hit that keeps them airborne; part of the launcher juggle route |
| 16 | Air Slam | swipe up while opponent is airborne | 36 dmg spike, ends the juggle with knockback; heavy hit-stop |
| 17 | Parry (perfect guard) | block that connects within 160 ms of its start | 0 dmg / 0 chip, attacker stunned ~0.45 s (punish window), defender +60 meter, teal spark + screen flash |
| 18 | Special-cancel | cast a card during your own attack recovery | spends meter to cancel the recovery and link the special ("normal xx special" combos) |
| 19 | Chain-cancel enders | during a connected light chain: swipe up / in / down | cancels the light's recovery into launcher / heavy / sweep (target combos); launcher ender opens the air juggle |

Feel systems (all render/timing only, never touch the deterministic sim): per-hit hit-stop scaled by move weight (light 45 ms to special 140 ms), camera shake + dolly-in punch on heavies/launchers/specials/KO, and a per-move procedural body-motion layer (ProcAnim) that gives each move a distinct silhouette on the shared rig (lunge, uppercut rise, low crouch, back-hop, air reach, character-flavoured special) at zero generation cost. Per-character feel: Kest agile (faster cadence, teal foxfire), Tengi heavy (slower, broader swings, crimson). New move VFX: vfx_dash_streak, vfx_parry_spark, vfx_impact_ring (generated) plus code-composited tints of existing sprites; all degrade gracefully if unsynced. HUD gained a combo counter and a parry cue. Full rationale and invariants: DECISIONS D-015 / D-016.

## MOVES v3 - kaiju-scaled moveset + two-stance guard (session 10, D-024 - owner directive)

The owner directed more variety across **kaiju-scaled** strikes (claws, tail, haunches - not human boxing form), built **together with** the guard system the mix-ups depend on, as **shared normals for the whole roster** (no per-character variants; only the CharacterDef feel knobs and measured body separate one champion's version from another's). Each move has its own generated clip (13 new clip GLBs, ASSET_MANIFEST) and its own reach/damage/recovery. Reaches are baseline (Kest) centre-to-centre values resolved per pairing exactly as in D-023.

### GUARD (the co-requirement)

`Fighter.GuardKind { None, Standing, Crouch }` replaces the old single block flag. One helper - `CombatSystem.GuardStops` - carries the whole rule so hit resolution, the AI and the HUD cannot disagree:

| Attack class | Standing guard | Crouch guard | No guard |
|---|---|---|---|
| high / mid (most normals) | **blocked** (75% off, 10% chip on specials) | **blocked** | hits |
| **LOW** (tail sweep, leg sweep) | **hits** | blocked | hits |
| **OVERHEAD** (claw slam) | blocked | **hits** | hits |
| **GRAB** (command grab) | **hits** | **hits** | hits |

- Airborne fighters guard nothing; a clean hit **breaks** the stance (a stunned fighter no longer counts as guarding).
- **Attacking opens you up:** the guard drops for the whole of the attack's recovery and cannot re-enter until it ends (`Fighter.OpenUpUntil` / `CanGuard`), so a stance can never be held through the strikes thrown out of it.
- Parry (D-015, 160 ms) belongs to the **right-thumb hold only**. The held-back walking guard is cheap and cannot parry; re-arming requires a fresh press.

### THE NORMALS

| # | Move | Trigger | Dmg | Reach | Recovery | Guard class |
|---|---|---|---|---|---|---|
| 20 | Claw Jab | tap | 15 | 1.10 | 0.20 | mid |
| 21 | Claw Cross | tap tap | 21 | 1.20 | 0.24 | mid |
| 22 | Claw Hook (arcing) | tap tap tap | 28 | 1.15 | 0.34 | mid, knockback 0.5 |
| 23 | Rising Claw Uppercut | swipe up | 34 | 1.35 | 0.50 | mid, launcher |
| 24 | Overhead Claw Slam | swipe up-and-toward | 41 | 1.30 | 0.56 | **OVERHEAD** |
| 25 | Haymaker | swipe toward | 45 | 1.60 | 0.62 | mid, knockback 1.5 |
| 26 | Tail Roundhouse | crouch + swipe toward | 38 | 1.75 | 0.54 | mid, knockback 1.2, **longest normal** |
| 27 | Low Tail Sweep | swipe down | 30 | 1.45 | 0.50 | **LOW** + knockdown |
| 28 | Low Leg Sweep | swipe down-and-away, or tap while crouching | 21 | 1.15 | 0.32 | **LOW** |
| 29 | Haunch Bash | swipe down-and-toward | 26 | 0.95 | 0.36 | mid, knockback 0.9 |
| 30 | Command Grab + body slam | swipe up-and-away | 52 | 0.85 | 0.70 | **IGNORES GUARD** + knockdown |

*Superseded by the rows above: MOVES rows 1-3 (Jab/Cross/Finisher), 4 (Heavy), 5 (Launcher), 6 (Sweep) and 7 (Block) as written - the moves survive under these names and numbers, and `Finisher` is now `Hook`.*

**The grab's cost is range and commitment.** It is the shortest offensive option in every pairing (0.85 m baseline, still clear of every push-out floor), has the longest recovery of any normal, cannot catch an airborne body, cannot be parried, and knocks down with wake-up i-frames so it never loops. It is the answer to a player who simply holds guard - and the reason holding guard is a decision instead of a default.

**Readability.** Every move has its own clip, fitted to its own window by `FighterAnimator.PlayFor` (the clips run 0.6-4.8 s while normals recover in 0.20-0.70 s; at speed 1 the fighter was still winding up after the hit resolved). `ProcAnim` gained a per-move gesture for each addition plus a **sustain** so a held crouch is a held pose. The HUD guard glyph shows *which* stance is up (teal standing / amber and dropped for crouch), and F2 gained a white grab-range tick.

## DAMAGE REBALANCE (session 11, D-025 - owner directive "longer, more realistic matches")

Every damage number in the tables above is **37.5% of its pre-session-11 value** - a 61.8-62.9% cut on each of the 19 moves, normals and specials alike. Two things were deliberately NOT scaled with it:

- **Health stays 1000** (and the round timer stays 60 s). Cutting the bar in step would have cancelled the directive: because the bar did not move, a KO now takes **~2.5-2.7x as many landed hits** (an average normal: 13 hits -> 33), which is the entire point.
- **Meter stays unchanged per HIT.** `MeterDealt` / `MeterTaken` in `CombatSystem` carry the reciprocal of the cut, so a segment still costs ~3.8 landed jabs. Scaling meter with damage would have made cards 2.7x rarer for 0.375x the payoff; instead they fire ~2.7x more often for 37.5% each, and specials' share of a health bar is exactly what it was.

Needing no change, and getting none: **juggle decay** (a ratio - a 4-hit route is still the same share of a bar relative to every other route), the **block reduction / chip** multipliers, every **FxWeight**, and all **stun / i-frame / parry / buffer** timings (those measure reaction, and the interaction *rate* did not change - only the health each interaction removes).

Changed beyond arithmetic: **combo windows widened ~17%** (chain 0.60 -> 0.70 s, light-into-heavy cancel 0.35 -> 0.42 s) because a completed string now buys 37.5% of what it did while a round holds ~2.7x more attempts - flagged in D-025 as the one judgment call, and a two-number revert. The HUD's **ghost-drain rate** was scaled by the same 0.375 (0.6 -> 0.225 fill/s), since it is a fraction-of-bar speed and small hits had made the amber "chunk lost" read nearly invisible.

Modelled result: median round **20-26 s -> 42-60 s**, clean hits per round **14-17 -> 32-45**. Open risk in D-025: at the passive end of the model the 60 s timer starts producing timeouts instead of KOs, and the fix (75-99 s) touches D-013's locked format, so it awaits the owner's on-device read.

## IMPACT CINEMA - selective slow motion (session 12, D-026 - owner directive)

Slow motion plays **only on certain attacks**, never on an ordinary hit. Two scripts: `TimeDirector.cs` (how a shot looks - it is also the **sole owner of `Time.timeScale`**, which pause now routes through) and `Cinematics.cs` (which hits earn one).

**Triggers - all earned, none random.** No RNG anywhere: a random crit would make the same read look different on two identical inputs, and would be the first non-deterministic thing in a sim the live-PvP plan wants lockstep-able.

| trigger | condition | shot |
|---|---|---|
| COUNTER | a **committed** strike (Heavy/Launch/Special weight, or the grab) lands on someone mid-swing - inside their own `AttackLockUntil` and **not** already in hitstun (a whiff punish, not the 2nd hit of a combo) | critical |
| BREAKER | the hit that takes an opponent from above **25%** health to at or below it (self-limiting - health only falls within a round) | critical |
| COMEBACK | a committed strike landed from under **20%** health; once per fighter per round | critical |
| SUPER | a **tier-3 card only** connecting cleanly (tiers 1-2 are the ones D-025 made frequent, and are excluded) | super |
| K.O. | the finishing blow, always; deeper and longer when it takes the match | ko / match-ko |

**Budget: at most 2 non-K.O. shots per round, at least 7 s apart** (K.O. exempt - there is one). At full budget that is **2.4-2.8 s of cinema in a 43-61 s round: 4-6% of its length, 7-9% of its clean hits.** Set at the rare end on purpose - too rare is a one-constant fix; too frequent is the failure the directive names.

**Shot table** (unscaled seconds; a stronger shot interrupts a weaker one, blending from the *current* scale rather than snapping back to 1 first):

| shot | scale | ease in | hold | ease out | camera push-in | wall-clock |
|---|---|---|---|---|---|---|
| critical | 0.32 | 0.14 | 0.16 | 0.34 | 0.35 m | 0.64 s |
| super | 0.26 | 0.16 | 0.26 | 0.42 | 0.55 m | 0.84 s |
| K.O. | 0.20 | 0.18 | 0.34 | 0.52 | 0.70 m | 1.04 s |
| match K.O. | 0.15 | 0.20 | 0.55 | 0.70 | 0.90 m | 1.45 s |

**Why it reads as a camera move and not as lag:** ramps run on **unscaled** time (easing out on scaled time means the fight wades back to speed); interpolation is **logarithmic**, because the eye tracks the frame-to-frame *ratio* of the scale; the envelope is asymmetric in **duration only** (~0.15 s in, 2.5-3.5x longer out) with **smootherstep** on both ramps, so a ramp meeting a flat stretch has no kink; and a shot **starts after the hit-stop bite**, waiting for `CombatFx` to unfreeze, so impact -> freeze -> slow motion -> return is one event. Audio follows: SFX pitch drags almost fully with the scale (including the impact already in flight), music dips and ducks slightly, **VO not at all**.

**Damage is untouched** - "critical" classifies the moment, not the numbers. Adding crit damage would quietly undo the D-025 table.

Locked out during **live remote PvP** (dilating one peer's clock is a desync, not an effect); loopback/AI is unaffected. **F3** toggles the whole system, persisted to `PlayerPrefs` `kr.slowmo`.

## ENEMY AI (Tengi)

State machine in EnemyAI.cs: APPROACH, POKE (lights/heavy mix), PUNISH (whiffed player heavy triggers counter window), DEFEND, SPEND (S1 at 1 seg as a close interrupt, S2 at 2 seg as the ranged answer, S3 at 3 seg on a knockdown). Difficulty ramps per round: reaction delay 320 ms round 1, 260 ms round 2, 200 ms round 3; block rate +10 percent per round (45 percent base).

**Amended session 9, D-023 — spacing is computed, not hardcoded.** Decisions stay on the reaction-delay tick but **locomotion runs every frame** (it used to live inside the tick, walking one frame in ~19). Every distance the AI acts on is derived from `CombatSystem.EffectiveReach` for the live pairing instead of a literal: APPROACH walks to just inside its own jab reach; POKE draws from the moves that actually reach, weighted 50/22/14/14 (jab / launcher / sweep / heavy) among those in range; DEFEND blocks only inside the opponent's threat reach; SPEND requires the card to reach (Kest's slot 1 exempt — it closes the gap itself); and the AI never loiters in the gap where the opponent's longest normal covers it and none of its own cover them, committing forward or stepping out. A per-round `IdleChance` (0.22 / 0.16 / 0.10) leaves punishable gaps between pokes.

**Amended session 10, D-024 - the AI plays the same game the player does.** POKE now draws from all ten shared normals (still reach-gated), DEFEND takes its guard in **either stance** and chooses which by READING the opponent - whatever hit it last is what it braces against (`Fighter.LastAttackLow` / `LastAttackOverhead`), so leaning on one half of the mix-up gets answered. New **MIX-UP** state: against a held guard it reaches for the tool that beats that stance - overhead vs crouch, low vs standing, grab vs either - at a per-round `MixupRate` (0.45 / 0.65 / 0.85), falling through to normal spacing when nothing that beats the stance is in range (so a turtle it cannot reach never freezes it). Crouch-stance share of its own guards: `CrouchGuardRate` 0.25 / 0.35 / 0.45 by round. *Superseded: "APPROACH (walk to 1.4 m) … DEFEND (blocks 45 percent of incoming strings, drops block vs sweeps 30 percent)" as literal distances.*

## ROUND / MATCH STRUCTURE

- Session flow (**amended session 8b, D-022**): Boot -> **Title screen (buttonless, tap anywhere)** -> **Story intro (7 world/lore beats, tap to advance, SKIP any time - shown ONCE per install, before character select on both the solo and the multiplayer path)** -> **How to play (controls primer, once; re-openable from fight select)** -> **Fight select (SOLO FIGHT / PLAY MULTIPLAYER + HOW TO PLAY + REPLAY STORY)** -> [multiplayer only: Multiplayer hub] -> Character select -> [multiplayer only: Lobby] -> Match -> Ending panel -> back to the title screen. Returning runs go Title -> Fight select directly; the intro no longer replays per solo fight (it used to run inside the fight flow). *Superseded: the original "Boot -> Main menu -> Story intro (3 motion-comic panels) -> Match" line.*
- Match: best-of-3 rounds, 60 s timer per round. Round ends on KO or timeout (higher remaining HP wins the round).
- Round banners: "ROUND ONE" / "ROUND TWO" / "FINAL ROUND" then "FIGHT". KO shows "K.O."; timeout shows "TIME".
- LIVING STAGE EVENT (interview decision): after round 1 ends, a scripted cutaway plays - khulandra_breach sprite rises behind the midground with roar SFX and vfx_kaiju_shockwave, banner "KHULANDRA RISES", and the ground layer swaps from harbor_ground to harbor_ground_flooded for the rest of the match. Fighters wade: walk speed -10 percent, splash particles on movement. Kaiju stays frame-breaking scale (Pillar 2) - only the breach sprite, never a full body.
- Match point: winning blow freezes, smash-cut (sfx_ending_sting) to the winner's Horrific Ending splash panel with caption, then results. **Amended session 12, D-026:** the freeze is now a composed beat rather than a flat 0.4 s wait — a 0.16 s hit-stop (`CombatFx.StopKo`; the killing blow previously had *no* hit-stop at all, since `Resolve` returns before the impact FX on the death path) easing straight into the K.O. slow-motion shot, and the "K.O." banner waits in **real** time for that shot to release. A scaled `WaitForSeconds` there is stretched by the very cinematic it waits on: 0.4 s would run ~2.7 s at the match shot's 0.15×. *Superseded: "winning blow freezes 0.4 s".*
- Player wins: panel_ending_kest_01, caption "The fox does not bury its dead. It multiplies them."
- Player loses: panel_ending_tengi_01, caption "The culling spares no one. Not even the brave."

## SCENE: "Harbor Ruins"

Layered 2D stage from existing assets: harbor_sky (far, slight parallax 0.1x), harbor_mid (alpha midground, parallax 0.4x), fight plane strip harbor_ground (tiles horizontally), khulandra_breach (event sprite, behind mid). Flat collision ground at y=0; arena 12 m wide with soft walls. Camera: orthographic-feel perspective locked on X, follows the midpoint of both fighters, zooms 10 percent tighter when they close within 2.5 m.

**Ground cues (added session 9, D-023, `GroundCues.cs` — code-generated sprites, no assets).** A soft contact shadow sits under each fighter on the fight plane (the stage is layered sprites and cannot receive a real shadow, so a rigged silhouette otherwise reads as pasted in front of the harbor); it tightens and fades as a fighter is lifted off the ground by a juggle. For the local player only, a two-tone ground band runs from the front of their body to their jab reach and on to their heavy reach, each segment lighting when the opponent is inside it — the gap is read off the floor rather than estimated between two silhouettes. `GroundCues.ShowReachGuide` turns the band off; `F2` adds a debug overlay marking every boundary that decides a hit, for both fighters.

## ART DIRECTION (locked - do not restyle)

"Ink-heavy seinen comic noir" per docs/ART_DIRECTION.md sections 1-2 (D-004, owner-accepted). All 41 visual assets already exist under `assets/` and are synced into `game/Assets/Art` by scripts/sync_assets.sh. No new visual generation in this build except animation clips. Characters are the existing rigged GLBs (kest_model.glb, tengi_model.glb).

## ANIMATION PIPELINE (owner decision 2026-07-18, D-011 - verbatim intent)

3D characters generated and rigged via Higgsfield (image-to-3D + rig action library: idle, walk, punch, block, special attack, hit reaction, death), imported into Unity via glTFast, and driven by Unity's Animator Controller with blend trees for movement and animation triggers wired to PlayerController/EnemyAI events. Non-standard rigs (caudatas, kaiju) get custom action selections rather than humanoid retargeting.

Slice implementation of that pipeline:

- Generate 7 animated GLB variants of Kest via the rig action library (animation_actions search: idle, walk, punch, block, special attack, hit reaction, death), seed-pinned.
- Both champions use the same Meshy humanoid skeleton, so the 7 clips retarget onto Tengi in Unity via Humanoid avatars (the owner's custom-selection rule applies to future non-humanoid rigs, not these two). If credits allow, Tengi gets his own punch/special clips for silhouette flavor.
- GltfCharacterLoader imports base GLBs + clip GLBs at runtime through glTFast, builds an AnimatorOverrideController: locomotion blend tree (idle <-> walk on speed), triggers TapAttack/Heavy/Launcher/Sweep/Block/Special/Hit/Death.
- Budget guard: preflight get_cost per animation job; generate in priority order (idle, walk, punch, hit, death, block, special) and stop before credits drop under a 50-credit buffer; any clip not generated is listed as planned in the manifest and the Animator falls back to the punch clip with VFX overlay. **Update (session 8, D-021):** the deferred **special** clip is now generated and wired (the fallback-to-punch no longer applies to specials), plus four extra combat clips (airrake, airslam, parry, back-dash) generated and synced — see ASSET_MANIFEST + D-021.

## AUDIO (interview decision: generate everything now - D-010 deferral satisfied inside the game pipeline)

All generated with Higgsfield in this build and saved per D-007 containers (music .ogg seamless loops, SFX .wav, convert with ffmpeg on save):

- Music (sonilo_music, duration 75 s loops): mus_title_theme, mus_fight_harbor, mus_story_fourpillars - manifest rows already spec these.
- SFX (mirelo_text_to_audio): sfx_hit_light, sfx_hit_heavy, sfx_block, sfx_kest_special, sfx_tengi_special, sfx_ending_sting, sfx_khulandra_roar, sfx_ui_tap.
- Announcer (seed_audio TTS, one deep grave voice): "ROUND ONE", "ROUND TWO", "FINAL ROUND", "FIGHT", "K.O.", "KHULANDRA RISES" - saved as assets/audio/vo/announcer_*.wav (new manifest rows).
- Priority if credits run short: fight music, hit_light, hit_heavy, block, roar, announcer set, ending_sting, then the rest; ungenerated rows stay planned with a README note.

## UI (all strings literal, English; fonts: hud.ttf = Barlow Condensed SemiBold, display.ttf = Shojumaru)

- Title screen (**amended session 8b, D-022 — no buttons**): background key_art.png; emblem.png centered top; title "REALM OF GORYO" (display font); subtitle "SHADOW OF GIANTS"; pulsing prompt "TAP ANYWHERE TO BEGIN"; version line "Shadow of Giants slice v0.1 - internal placeholder build". The whole key art is the tap target. Mode buttons and the control hint line moved to fight select / how to play.
- Fight select (**new, D-022**): menu_bg backdrop, title "CHOOSE YOUR FIGHT", plates "SOLO FIGHT" and "PLAY MULTIPLAYER", secondary plates "HOW TO PLAY" and "REPLAY STORY", the control hint line, version line, "BACK" (to the title screen).
- Story intro (**amended session 8b, D-022**): 7 full-screen beats — 2061 / THE FOUR PILLARS / TOKYO HARBOR / THE KAIJU / THE POWERS / THE CHAMPIONS / SHADOW OF GIANTS — each with an eyebrow heading + a caption strip at the bottom, progress pips bottom-left, "TAP TO CONTINUE" bottom-right, "SKIP" top-right. Art is existing placeholders (key_art, the three story_fourpillars panels, menu_bg, vs_screen, harbor_sky + the khulandra_breach cut-out overflowing the frame per Pillar 2). Beat text is a plain-language digest of LORE_BIBLE v1 §2–§6 — every claim traces to a `[CONFIRMED]` line, no invented canon; it is the single place to correct when the Lore Bible reaches v2.
- How to play (**new, D-022**): ink panel over a blocking dim, heading "HOW TO PLAY", lead "One thumb. The screen is split: move on the left, fight on the right.", two columns of control lines mirroring TOUCH CONTROLS + MOVES v2, a Khulandra/living-stage note, button "GOT IT". Auto-opens once after the intro; re-openable from fight select.
- HUD: two health bars top (hud_healthbar.png frames, Bone fill draining to reveal Blood Seal), fighter names "KEST" (left) and "TENGI" (right), round pips (2 per side), center timer counting 60 to 0, meter bar bottom-left (hud_meter.png, Goryo Flame fill), three ability cards bottom-right (ability_card.png frame + ability_icons_kest tiles), PAUSE button top-right (icon_sheet glyph).
- Banners (display font, full width): "ROUND ONE", "ROUND TWO", "FINAL ROUND", "FIGHT", "K.O.", "TIME", "KHULANDRA RISES".
- Pause overlay: panel_frame.png, "PAUSED", buttons "RESUME" and "QUIT TO TITLE".
- Results: "VICTORY" or "DEFEAT", line "Rounds: {playerRounds} - {enemyRounds}", buttons "REMATCH" and "TITLE".
- VS splash before round 1: vs_screen.png with both portraits and "VS" (display font).

## PROJECT / TECH

```
game/
├── Assets/
│   ├── Scripts/        Bootstrap.cs, GameManager.cs, RoundManager.cs, PlayerController.cs,
│   │                   EnemyAI.cs, CombatSystem.cs, StageManager.cs, TouchInput.cs, TouchUI.cs,
│   │                   GltfCharacterLoader.cs, StoryIntro.cs, EndingPanel.cs, AudioManager.cs, MainMenu.cs,
│   │                   FightSelectMenu.cs, HowToPlay.cs (D-022) + the D-017 multiplayer front-end/net seam
│   ├── Art/            synced copies of assets/ (concept excluded), via scripts/sync_assets.sh
│   ├── Models/         kest_model.glb, tengi_model.glb + animation clip GLBs
│   ├── Audio/          music/, sfx/, vo/
│   ├── Fonts/          hud.ttf, display.ttf
│   └── Scenes/         Boot.unity (near-empty: one GameObject with Bootstrap)
├── Packages/manifest.json   (com.unity.cloud.gltfast, URP, Input System)
├── ProjectSettings/         (Android, landscape, API 26, ARM64, product/package names, icon)
└── README.md                (exact Unity Hub open + Android build steps)
```

- Code-first Bootstrap builds everything at runtime (camera, light, stage layers, fighters, UI canvas) - no hand-authored prefabs beyond the near-empty Boot scene.
- Deterministic fight sim core: CombatSystem resolves hits from capsule ranges on the X axis (ARCHITECTURE.md testing intent), render layer stays thin.
- App icon: ui emblem (emblem.png) as the Android launcher icon. Main-menu art: key_art.png. No new store art - nothing is published.
- Sync contract: never edit game/Assets/Art copies; masters live in assets/ (ARCHITECTURE.md).

## DELIVERABLE

A complete `game/` Unity project folder plus README with exact local build steps (Unity Hub install 6000.0.x LTS, open folder, let glTFast resolve, switch platform to Android, Build). The user builds the APK on their machine. Nothing is deployed or published; all placeholder art stays internal (rights confirmed D-009, but placeholders remain pre-lore-bible inferences).

## DEFAULTS CHOSEN (correct me if wrong)

- Kest is the player character; Tengi is AI (max contrast per Vision Goal 1; reversing is a one-line change).
- 1000 HP / 60 s rounds / damage numbers above are first-pass tuning targets, expected to move in playtest.
- Announcer voice is a single deep grave male-read voice; swap by regenerating with another seed_audio preset.
- Story intro uses the three existing Four Pillars panels with short original captions; full Act 1 chapters remain future work. **Amended D-022:** it now runs 7 beats over existing placeholder art (the three panels plus key_art, menu_bg, vs_screen, harbor_sky + khulandra_breach) and is front-end, once-per-install, and skippable; full Act 1 chapters are still future work.
- Walk-speed penalty in flood state is 10 percent (readable but not punishing).
