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

- LEFT HALF, horizontal drag: walk toward / away from the opponent (PlayerController.Move(float axis)). Drag right of the touch origin walks forward, left walks back. Releasing stops.
- RIGHT HALF, tap: light attack; consecutive taps chain up to 3 hits (PlayerController.TapAttack()).
- RIGHT HALF, swipe toward opponent: heavy attack (PlayerController.HeavyAttack()).
- RIGHT HALF, swipe up: launcher (PlayerController.Launcher()).
- RIGHT HALF, swipe down: sweep (PlayerController.Sweep()).
- RIGHT HALF, press and hold (no movement): block while held (PlayerController.SetBlock(bool held)).
- BOTTOM-RIGHT: three ability card buttons labeled with card art (ui_ability_icons_kest tiles); tap spends meter (PlayerController.CastSpecial(int slot)).
- TOP-RIGHT: PAUSE button (GameManager.TogglePause()).
- Input-to-impact: gesture recognition resolves on touch-up or 120 ms of hold, whichever is first; attack anims cancel their first 2 frames into the next buffered input.

## MOVES (name / trigger / behavior / numbers)

Both champions: 1000 HP, meter of 3 segments (a segment charges per 150 damage dealt or 80 taken).

| # | Move | Trigger | Behavior |
|---|---|---|---|
| 1 | Light chain 1 (Jab) | tap | 40 dmg, 1.1 m reach, 0.25 s recovery, chains within 0.6 s |
| 2 | Light chain 2 (Cross) | tap tap | 50 dmg, same reach, chains |
| 3 | Light chain 3 (Finisher) | tap tap tap | 70 dmg, small knockback, 0.5 s recovery |
| 4 | Heavy | swipe toward | 120 dmg, 1.6 m reach, 0.8 s recovery, knockback 1.5 m |
| 5 | Launcher | swipe up | 90 dmg, pops the opponent airborne 0.7 s (juggle: one free light) |
| 6 | Sweep | swipe down | 80 dmg, low hit, beats standing block |
| 7 | Block | hold | reduces damage 75 percent, 10 percent chip on specials, cannot attack while held |
| 8 | Kest S1: Fox-fire Dash | card 1 (1 seg) | gap-closing dash strike, 100 dmg, plays vfx_kest_foxfire |
| 9 | Kest S2: Phantom Rake | card 2 (2 seg) | three-hit phantom claw combo, 160 dmg total |
| 10 | Kest S3: Hunt of Shadows | card 3 (3 seg) | cinematic: ring of fox shadows, 280 dmg, brief slowdown |
| 11 | Tengi S1: Crow Wall | card 1 (1 seg) | 1.2 s counter stance; countered hit answers for 130 dmg (vfx feathers) |
| 12 | Tengi S2: Culling Arc | card 2 (2 seg) | horizontal blade wave, 180 dmg, plays vfx_tengi_bladewave |
| 13 | Tengi S3: Black Sun | card 3 (3 seg) | slow overhead execution arc, 300 dmg, huge recovery on whiff |

## ENEMY AI (Tengi)

State machine in EnemyAI.cs: APPROACH (walk to 1.4 m), POKE (lights/heavy mix, 60/40), PUNISH (whiffed player heavy triggers counter window), DEFEND (blocks 45 percent of incoming strings, drops block vs sweeps 30 percent), SPEND (uses S1 at 1 seg when player approaches, S2 at 2 seg at range, S3 at 3 seg after a knockdown). Difficulty ramps per round: reaction delay 320 ms round 1, 260 ms round 2, 200 ms round 3; block rate +10 percent per round.

## ROUND / MATCH STRUCTURE

- Session flow: Boot -> Main menu -> Story intro (3 motion-comic panels, tap to advance) -> Match -> Ending panel -> back to menu.
- Match: best-of-3 rounds, 60 s timer per round. Round ends on KO or timeout (higher remaining HP wins the round).
- Round banners: "ROUND ONE" / "ROUND TWO" / "FINAL ROUND" then "FIGHT". KO shows "K.O."; timeout shows "TIME".
- LIVING STAGE EVENT (interview decision): after round 1 ends, a scripted cutaway plays - khulandra_breach sprite rises behind the midground with roar SFX and vfx_kaiju_shockwave, banner "KHULANDRA RISES", and the ground layer swaps from harbor_ground to harbor_ground_flooded for the rest of the match. Fighters wade: walk speed -10 percent, splash particles on movement. Kaiju stays frame-breaking scale (Pillar 2) - only the breach sprite, never a full body.
- Match point: winning blow freezes 0.4 s, smash-cut (sfx_ending_sting) to the winner's Horrific Ending splash panel with caption, then results.
- Player wins: panel_ending_kest_01, caption "The fox does not bury its dead. It multiplies them."
- Player loses: panel_ending_tengi_01, caption "The culling spares no one. Not even the brave."

## SCENE: "Harbor Ruins"

Layered 2D stage from existing assets: harbor_sky (far, slight parallax 0.1x), harbor_mid (alpha midground, parallax 0.4x), fight plane strip harbor_ground (tiles horizontally), khulandra_breach (event sprite, behind mid). Flat collision ground at y=0; arena 12 m wide with soft walls. Camera: orthographic-feel perspective locked on X, follows the midpoint of both fighters, zooms 10 percent tighter when they close within 2.5 m.

## ART DIRECTION (locked - do not restyle)

"Ink-heavy seinen comic noir" per docs/ART_DIRECTION.md sections 1-2 (D-004, owner-accepted). All 41 visual assets already exist under `assets/` and are synced into `game/Assets/Art` by scripts/sync_assets.sh. No new visual generation in this build except animation clips. Characters are the existing rigged GLBs (kest_model.glb, tengi_model.glb).

## ANIMATION PIPELINE (owner decision 2026-07-18, D-011 - verbatim intent)

3D characters generated and rigged via Higgsfield (image-to-3D + rig action library: idle, walk, punch, block, special attack, hit reaction, death), imported into Unity via glTFast, and driven by Unity's Animator Controller with blend trees for movement and animation triggers wired to PlayerController/EnemyAI events. Non-standard rigs (caudatas, kaiju) get custom action selections rather than humanoid retargeting.

Slice implementation of that pipeline:

- Generate 7 animated GLB variants of Kest via the rig action library (animation_actions search: idle, walk, punch, block, special attack, hit reaction, death), seed-pinned.
- Both champions use the same Meshy humanoid skeleton, so the 7 clips retarget onto Tengi in Unity via Humanoid avatars (the owner's custom-selection rule applies to future non-humanoid rigs, not these two). If credits allow, Tengi gets his own punch/special clips for silhouette flavor.
- GltfCharacterLoader imports base GLBs + clip GLBs at runtime through glTFast, builds an AnimatorOverrideController: locomotion blend tree (idle <-> walk on speed), triggers TapAttack/Heavy/Launcher/Sweep/Block/Special/Hit/Death.
- Budget guard: preflight get_cost per animation job; generate in priority order (idle, walk, punch, hit, death, block, special) and stop before credits drop under a 50-credit buffer; any clip not generated is listed as planned in the manifest and the Animator falls back to the punch clip with VFX overlay.

## AUDIO (interview decision: generate everything now - D-010 deferral satisfied inside the game pipeline)

All generated with Higgsfield in this build and saved per D-007 containers (music .ogg seamless loops, SFX .wav, convert with ffmpeg on save):

- Music (sonilo_music, duration 75 s loops): mus_title_theme, mus_fight_harbor, mus_story_fourpillars - manifest rows already spec these.
- SFX (mirelo_text_to_audio): sfx_hit_light, sfx_hit_heavy, sfx_block, sfx_kest_special, sfx_tengi_special, sfx_ending_sting, sfx_khulandra_roar, sfx_ui_tap.
- Announcer (seed_audio TTS, one deep grave voice): "ROUND ONE", "ROUND TWO", "FINAL ROUND", "FIGHT", "K.O.", "KHULANDRA RISES" - saved as assets/audio/vo/announcer_*.wav (new manifest rows).
- Priority if credits run short: fight music, hit_light, hit_heavy, block, roar, announcer set, ending_sting, then the rest; ungenerated rows stay planned with a README note.

## UI (all strings literal, English; fonts: hud.ttf = Barlow Condensed SemiBold, display.ttf = Shojumaru)

- Main menu: background key_art.png; emblem.png centered top; title "REALM OF GORYO" (display font); subtitle "SHADOW OF GIANTS"; button "TAP TO FIGHT"; hint line "Tap: light chain - Swipe: heavy - Hold: block - Cards: specials"; version line "Shadow of Giants slice v0.1 - internal placeholder build".
- Story intro: the three story_fourpillars panels full-screen, caption strip at the bottom, "TAP TO CONTINUE" bottom-right, "SKIP" top-right.
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
│   │                   GltfCharacterLoader.cs, StoryIntro.cs, EndingPanel.cs, AudioManager.cs, MainMenu.cs
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
- Story intro uses the three existing Four Pillars panels with short original captions; full Act 1 chapters remain future work.
- Walk-speed penalty in flood state is 10 percent (readable but not punishing).
