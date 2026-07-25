# DECISIONS — append-only log

*Format: `D-### — title (date, status)`. Statuses: `accepted`, `proposed`, `rejected`, `superseded by D-###`. Newest entries at the bottom. A decision is logged the same session it is made. One allowed mutation: the status line of a `PROPOSED` entry may be flipped to accepted/rejected with the sign-off date. Everything else is append-only — supersede, never rewrite.*

## D-001 — Minimal documentation structure (2026-07-17, accepted)

**Decision:** Doc set is CLAUDE.md + README.md at root; VISION, STATUS, ART_DIRECTION, ASSET_MANIFEST, ARCHITECTURE, DECISIONS in `docs/`. Session continuity = STATUS.md (rewritten each session) + DECISIONS.md (append-only) + the `/wrap-session` skill, mechanically backed by a SessionStart freshness hook and check scripts (see D-008).

**Dropped or merged from the owner's candidate list:** GAMEPLAY.md → will exist as `docs/DESIGN_BRIEF.md`, born from the studio intake interview (the Vision names the brief as the next document). UI_GUIDELINES.md → ART_DIRECTION §3. HIGGSFIELD_PROMPTS.md → ART_DIRECTION §5 plus per-asset subject lines in the manifest. CODING_STANDARDS.md → a CLAUDE.md section, added when code starts. SKILLS.md → CLAUDE.md §Tooling. ROADMAP.md → STATUS §Milestones (Vision §7 already holds the horizon-level roadmap). CHANGELOG.md → STATUS session log + git history. TESTING.md → ARCHITECTURE §Testing until there is code to test.

**Why:** fewer files get updated more reliably, and every fact has exactly one home — no drift between near-duplicates.

## D-002 — Placeholder assets via Higgsfield, replaceable by design (2026-07-17, accepted)

Claude acts as the temporary art team using the Higgsfield MCP (generation tools only — never its deploy/publish tools). Every asset gets: a manifest row, a canonical stable path, and a generation record written immediately at generation time — so the official art team can replace any asset by overwriting one file (full per-type contract: ART_DIRECTION §4). All placeholders are internal-only pending rights-holder approval (Vision §12).

## D-003 — Asset naming and statuses (2026-07-17, accepted)

IDs are `<category>_<subject>_<variant>`; files are `snake_case` with no version suffixes (git is the version history once commits are enabled). Statuses: `planned → placeholder → final`, plus `needs-rework`. Derived assets flip to `needs-rework` when their source changes.

## D-004 — Art identity proposal: "ink-heavy seinen comic noir" (2026-07-17, accepted 2026-07-18)

Style bible and six-swatch palette in ART_DIRECTION.md v0.1 (Sumi Ink / Bone Paper / Ash Steel / Blood Seal / Goryō Flame / Signal Amber). Accepted by direct owner sign-off on 2026-07-18, ahead of the style test (see D-009).

## D-005 — Style test gates all batch generation (2026-07-17, accepted)

The style test is generated **first and alone**: one asset (`concept_style-test_fight-scene`), for which up to 4 candidate images may be generated per the normal pipeline — a candidate batch counts as one asset. It may be generated under the still-PROPOSED D-004 style, because the owner reviews D-004 *on this image*. No other asset is generated until the owner approves the look (approval recorded by flipping D-004 to accepted and noting it in the asset's generation record). Keeps the placeholder set cohesive and avoids spending generation credits on an unapproved look.

## D-006 — No game code before the design brief (2026-07-17, accepted)

Per owner instruction (2026-07-17) and the Vision's own sequence (studio intake interview → design brief → build), `game/` stays empty until `docs/DESIGN_BRIEF.md` exists.

## D-007 — Audio containers: OGG music, WAV SFX (2026-07-17, accepted)

Music ships as `.ogg` (MP3 cannot loop seamlessly — encoder padding gaps the loop); SFX ship as `.wav` (lowest decode latency for the sub-80ms input-to-impact bar, Vision Pillar 3). Decided before any audio is generated because canonical paths are stable forever — changing the extension later would break the file-for-file replacement contract. Convert on save if the generator returns another format.

## D-008 — Doc updates are mechanically enforced, not aspirational (2026-07-17, accepted)

A SessionStart hook (`.claude/settings.json`) runs `scripts/check_docs_fresh.sh` every session start and warns when project files are newer than the docs — the signature of a session that died before `/wrap-session`. The next session self-heals by reconciling first. `scripts/check_manifest.sh` mechanically verifies the no-orphan-assets invariant at wrap time. Generation records are written at generation time (never batched), so a mid-session crash cannot lose prompt provenance.

## D-009 — Owner sign-offs of 2026-07-18 (accepted)

The owner answered all open questions on 2026-07-18: **Rights** — the project team owns the *Realm of Goryo* IP and is authorized to develop it (Vision §12 satisfied). **Tone** — the M-rating escalation and Horrific Endings framing are approved (Vision §3.4 satisfied). **Style** — D-004 accepted by direct sign-off; the style test is still generated first, but as a cohesion anchor self-reviewed against the style bible rather than an owner gate (softens D-005's stop-and-wait). **Git** — session-end commits authorized; remote is `https://github.com/RajdeepSah/KAIJU_RUIN` (owner username RajdeepSah). **VISION.md §-reference typos** — owner approved the two corrections; applied. **Business model** — deferred to the design brief (Vision §10).

## D-010 — Audio generation deferred to the scaffold phase (2026-07-18, accepted; satisfied 2026-07-18 by D-013)

The Higgsfield MCP restricts its music (sonilo_music) and SFX (mirelo_text_to_audio) models to its own game-creation pipeline; standalone music/SFX generation is not permitted through the MCP. The 10 audio manifest rows therefore stay `planned` until the design-brief/Unity-scaffold phase, where the `game-studio` skill's asset flow can use those models legitimately. Manifest specs and D-007 container rules are unchanged.

## D-011 — Animation pipeline (2026-07-18, accepted — owner decision)

3D characters generated and rigged via Higgsfield (image-to-3D + rig action library: idle, walk, punch, block, special attack, hit reaction, death), imported into Unity via glTFast, and driven by Unity's Animator Controller with blend trees for movement and animation triggers wired to PlayerController/EnemyAI events. Non-standard rigs (caudatas, kaiju) get custom action selections rather than humanoid retargeting. Slice implementation detail: clips are generated once on Kest's rig and retargeted to Tengi via Unity Humanoid avatars (both share the Meshy humanoid skeleton); the custom-selection rule applies to future non-humanoid rigs.

## D-012 — Business model: FREE (2026-07-18, accepted — owner decision)

The game is free. No purchases, no ads, no monetization code in the slice. Resolves the Vision §10 lane question (owner answer 2026-07-18); Pillar 6's fairness commitments stand for any future monetization work, which would need a new decision.

## D-013 — Vertical-slice format (2026-07-18, accepted — intake interview)

Best-of-3 rounds (60 s), 1v1 Kest (player) vs Tengi (AI). Khulandra living-stage event fires between rounds 1 and 2 as a scripted breach that swaps the ground to the flooded strip. All audio (music, SFX, announcer VO) is generated NOW inside the game-build pipeline, satisfying D-010's deferral. Full spec: docs/DESIGN_BRIEF.md v1.0 (the build prompt for `game/`).

## D-014 — Session 4 gameplay UI + control-scheme rework (2026-07-18, accepted — owner directive)

The owner froze the gameplay loop, architecture, and environment/nature art as approved and directed a focused rework of the **gameplay UI and the touch control scheme** (HUD readability, mobile ergonomics, input responsiveness/feel), cross-checked against the 80 ms input-to-impact and 60 fps bars (Vision Goal 2). Decisions that now constrain future work:

- **Swipes commit at threshold-crossing, not on touch-up.** Heavy/launcher/sweep fire the instant finger displacement exceeds the swipe threshold (4% screen width), removing 80–300 ms of finger-contact latency. Taps still resolve on touch-up; hold-block still engages at 120 ms but is now driven from `Update()` off a cached finger position, so it fires on time even when the OS coalesces `Stationary` events.
- **One-slot input buffer** (`PlayerController`): an attack arriving during recovery is stored (≤180 ms) and fired the frame the fighter can act again — the brief's "next buffered input" rule. Chain mashing is now recovery-limited (deterministic ~250 ms cadence) instead of retry-limited (felt like dropped inputs). The literal "cancel the first 2 animation frames" micro-clause is **not** implemented (documented deviation); the buffer captures the feel gain.
- **Pause state is owned by `GameManager`** (`GameManager.Paused` + public `TogglePause()`), replacing the private `TouchUI` method (the brief specified `GameManager.TogglePause()`). `TouchInput` and `EnemyAI` gate on it, closing the defect where attacks landed — and could score a KO — behind the PAUSED overlay because `Time.timeScale = 0` never stopped the raw-input gesture layer.
- **Swipe away from the opponent is a no-op** (was a phantom jab + 0.25 s recovery lock that fed Tengi's PUNISH state). Retreat is the left-half drag. Reserving away-swipe for a future back-dash is left open.
- **The gesture shield covers all interactive UI.** `PointerOverUi` tests a registry of every button rect (3 cards + pause + the active pause-overlay buttons), so a UI press never double-fires as an attack. Non-interactive HUD images are set `raycastTarget = false`.
- **REMATCH tears down the prior fight** (`GameManager.CleanupFight()` runs before every fight) — it previously stacked a second camera/HUD/fighter set and a ghost fight on top of the old one.
- **HUD readability upgrades**: dual 3-cell segmented meters (player Goryō Flame, **enemy meter added** in Blood Seal so the AI's untelegraphed specials are anticipable); ghost-drain health bars; chain-step pips that light on **landed** hits only; a block-state indicator glyph; distinct block (Ash Steel deflect, no blood) and whiff VFX; per-step chain animation escalation; ability-card cost pips with distinct cast/deny feedback; timer urgency under 10 s; and the previously-missing "KHULANDRA RISES" banner. UI sprite sheets (`icon_sheet`, `button_set`, ability icons) are now correctly **sliced** instead of drawn whole — no new art was generated.
- **Sub-80 ms touch acknowledgement**: every touch-down spawns an ink-ring splash (ART_DIRECTION §3).
- **Frame pacing**: `vSyncCount = 0`, `targetFrameRate` held at **60** (not the panel refresh) to protect the Snapdragon-695 thermal budget (Pillar 7). A dev-only `PerfMonitor` overlay (toggle F1) reads fps mean / 1%-low and gesture→impact latency, so the Goal 2 bars can be verified **on device** (in-process latency is ~0 frames — the path is synchronous — so most of the real 80 ms is touch-panel + display, measurable only with slow-mo/Perfetto).

All combat numbers stay frozen (DESIGN_BRIEF MOVES); no fight-sim, round-flow, or stage-event logic changed. Open items requiring owner input are recorded in STATUS.md (meter-charge-rate deviation; key_art vs harbor_sky menu background; away-swipe reservation).

## D-015 — Session 5 combat expansion: "lean into the fighter" (2026-07-19, accepted — owner directive)

The owner directed a deliberate expansion of combat depth and feel — "this is a fighting game at its core; lean into that" — with four aims: more action / faster, more dynamic combat; more player moves (combos, specials, varied attacks); more fighting animations per character; more creative freedom in move design. Explicit scope line: **keep the current gameplay loop, architecture, and environment/nature art as-is.** This decision therefore **supersedes the D-014 "all combat numbers frozen" note** for combat depth and cadence (the freeze was a session-4 self-imposed scope, not a design lock); the best-of-3 / 60 s / Khulandra / KO / ending loop, the one-thumb control-zone scheme, the deterministic X-axis sim architecture, HP=1000 / meter=3-segment, and the stage/nature art are all preserved unchanged.

What now constrains future work:

- **Feel systems (new `CombatFx.cs`).** Hit-stop is a per-hit freeze *window* (`CombatFx.Frozen`, scaled by move weight: light 45 ms → special 140 ms), deliberately **not** `Time.timeScale` (pause owns that; the two must never fight). Camera shake + a dolly-in "punch" on heavy/launcher/special/KO are composited in `RoundManager.Update()` on top of a pre-shake `camBase`. This is the primary lever that makes the fight read as fast and impactful.
- **Procedural animation (new `ProcAnim.cs`).** The slice ships 6 shared clips retargeted to both champions, so every attack otherwise reads as the same punch. A short per-move envelope now drives a local offset on the rig's **`Visual` child transform only** — lunge, uppercut rise, low-sweep crouch, back-hop, air reach, character-flavoured special flourish. **Hard invariant preserved:** the root's X position remains the sole sim truth for range/hit; procedural motion never touches the root. This gives "more animations per character" at zero generation cost, and is the drop-in seam for real generated clips later (same `ProcAnim.Move` gestures per move).
- **New universal moves, same one-thumb scheme (no new gestures except D-016):** evasive **back-dash** with i-frames (swipe away, see D-016); **air-juggle** follow-ups (while the opponent is popped: tap → Air Rake keeps them airborne, swipe-up → Air Slam spikes down) with a `MaxJuggle`=4 decay so juggles can't loop forever; **parry** (a block landing within 160 ms of its start = perfect guard: zero damage, attacker stunned ~0.45 s, defender +meter); **special-cancel** (a card cast during your own recovery cancels it and links — "normal xx special"); **chain-cancel enders** (a light chain can cancel into heavy/launcher/sweep on connect — target combos); **step-in momentum** (attacks drive the attacker forward, never crossing a min gap — aggressive flow).
- **Per-character identity via `CharStyle` fields (no rebalance of HP/damage):** Kest (player) is agile — `AttackSpeed` 1.12, quicker feet, teal foxfire accent, snappier procedural amplitude/duration; Tengi (AI) is heavy — `AttackSpeed` 0.92, broader/slower committed swings, crimson bladewave accent. HP stays 1000/1000 (brief); style is cadence + procedural amplitude + VFX tint only.
- **Faster cadence retune (the number changes this decision authorises):** universal-normal recoveries trimmed (jab/cross 0.25→0.20 s, finisher 0.5→0.38, heavy 0.8→0.62, launcher 0.6→0.50, sweep 0.6→0.50), walk speed 2.6→3.0 (Kest 3.1 / Tengi 2.8 via style). Damage numbers, reaches, meter economy, and the meter-charge rates are **unchanged** (the D-014 meter-taken-rate open question is untouched and still stands). New moves' numbers: AirRake 55 / AirSlam 95 dmg; parry 0 dmg + 60 meter to defender; back-dash 1.2 m + 0.24 s i-frames + 0.30 s recovery.
- **AI parity:** Tengi received the same toolkit (air follow-ups, ramped read-parry from round 2, evasive back-dash, varied poke/launcher/sweep mix) so the fight stays dynamic and fair rather than one-sided.
- **Generation posture (owner call this session):** code-first — the entire expansion is code + procedural animation at zero credit cost, with new move VFX **code-composited** from existing sprites (tinted/scaled) plus **3 cheap generated sprites** (`vfx_dash_streak`, `vfx_parry_spark`, `vfx_impact_ring`, recraft_v4_1, 1.25 cr each = 3.75 cr total), respecting the D-013 50-credit buffer (84.5 → 80.75). All new VFX degrade gracefully (fallback to an existing sprite) if not yet synced. A full per-move *generated* animation library remains future work gated on a credit top-up (the balance can't fund it with candidate iteration).

Validated: all 21 scripts compile clean under Unity 6000.0.78f1's Roslyn against the real UnityEngine/UGUI/glTFast reference assemblies (0 errors). On-device playtest is the owner's step (editor holds the project lock; no Android device attached).

## D-016 — Swipe-away bound to the evasive back-dash (2026-07-19, accepted — resolves D-014 open item)

The right-half swipe *away* from the opponent — a deliberate no-op since D-014, reserved pending a decision — is now the **evasive back-dash**: a ~1.2 m backward hop with i-frames (0.24 s) and 0.30 s recovery, executable only from neutral (never a cancel, so it can't be abused to escape recovery). This closes the third session-4 open question. Retreat by walking is still the left-half drag; the back-dash is the committal, invulnerable spacing/defense tool. The remaining two session-4 open questions (meter-taken-charge-rate deviation; key_art vs harbor_sky menu background) are unaffected and still open.

## D-017 — Multiplayer mode: pluggable-net architecture + data-driven roster (2026-07-19, accepted — owner directive)

The owner directed adding a **"Play Multiplayer"** mode where a player can choose their champion, choose the opponent's champion, invite a friend by room code, and find a random online match worldwide — with the UI and networking built to **scale** to more characters and matchmaking options later. This is the first feature to touch the "real-time PvP" space that ARCHITECTURE.md flags as a *gated, service-dependent* project, so it is built as a **seam, not a hosted service** — the game still builds to a local APK and plays with zero external dependencies (Vision §11 / ground rule 3).

What now constrains future work:

- **Networking is an abstraction with a shipping offline default.** Two interfaces are the only seam the game talks to: `IMatchmaker` (create/join room, quick match — coroutine-shaped so the lobby animates status) and `INetTransport` (carries a per-tick `FighterInputCmd` between peers). The **shipping** implementations are `LocalMatchmaker` + `LoopbackTransport`: real room codes, a real "Searching…/Connecting…" flow, real session hand-off — but the *peer* is simulated, so the opponent resolves to the **AI** (`MatchConfig.RemoteOpponent = false`). A live backend is the drop-in: implement `RelayMatchmaker` / `RelayTransport` (stubbed with the exact integration steps — Unity Authentication + Lobby + Matchmaker + Relay + UTP; input-lockstep suits the deterministic X-axis sim) and flip `NetService.Backend` to `Relay`. **No UI or fight code changes** to go live. `NetService` (Bootstrap singleton) is the one place the backend is selected and the transport lifetime is owned.
- **Roster is data-driven (`CharacterRoster` / `CharacterDef`).** Every per-character value the fight code used to hardcode (model GLB, anim-clip prefix, portrait, theme tint, special set, HUD icon key, attack speed, walk speed, procedural amp/dur, tagline) now lives in one `CharacterDef`. **Adding a champion = adding one entry** — it appears in character-select, HUD, VS, and the fight with no other code change. `RoundManager` spawns both fighters from `MatchConfig` via the roster; `CombatSystem.Special(set, slot)` picks a fighter's cards by its `SpecialSet`, so `PlayerController` and `EnemyAI` are champion-agnostic. Kest and Tengi are now just the first two rows.
- **Front-end flow.** Menu now offers **Solo Fight** and **Play Multiplayer**. Solo: Menu → CharacterSelect → Fight. Online: Menu → MultiplayerMenu (Quick Match / Create Room / Join Room-by-code) → CharacterSelect → Lobby (matchmaking + room code to share) → Fight. Character-select lets you pick **both** your fighter and your opponent (a preference the online matchmaker may override for Quick Match, where the opponent's pick comes from the peer). `MatchConfig` is the shared blackboard the front-end fills and the fight reads; its defaults reproduce the original Kest-vs-Tengi single-player fight, so nothing regresses.
- **Remote-drive seam.** In a live match the opponent fighter is driven by `RemoteController` (applies received `FighterInputCmd` through the *same* `PlayerController` verbs the local human uses, with `Local = false` so remote inputs never touch the local HUD); the local player mirrors intent out via `NetInputRelay`. Under loopback these are inert (no peer) and the opponent is the AI — so they are the built, documented PvP seam, not shipping-exercised live code.
- **Art posture: prepared, not generated.** Per the owner, **no artwork was generated this session.** The multiplayer screens ship on existing placeholder art (harbor sky, plates, portraits, VS screen). Two dedicated assets (`ui_menu_bg`, `ui_roster_card`) are `planned` rows with pre-composed prompts in ASSET_MANIFEST's *Planned generation queue*, referenced in code via `AssetLib.SpriteOr(preferred, fallback)` so they drop in on the next sync with zero code change when the owner says **"generate necessary artwork."** A per-new-champion art template is documented there too.

Validated: all 32 scripts (21 prior + 11 new) compile clean under Unity 6000.0.78f1's Roslyn against the real UnityEngine/UGUI/glTFast reference assemblies (0 errors). The loopback online flow and character-select are exercisable in-editor now; live worldwide PvP awaits the backend integration above (still gated — it needs a hosted relay/matchmaking service, which the owner has not authorised and which ground rule 3 keeps out of the local build). On-device playtest remains the owner's step.
