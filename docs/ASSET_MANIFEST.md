# ASSET MANIFEST — Kaiju Ruin

*The ledger of every art, audio, and font asset. One row per asset. No file exists in `assets/` without a row here; no row changes status unless the file actually changed (verify with `scripts/check_manifest.sh`). Prompts are composed per the template in ART_DIRECTION.md §5; the pipeline and full replacement contract live in ART_DIRECTION.md §4 — this doc only states: overwrite the file at the canonical path, flip the status, record the date.*

**Statuses:** `planned` → `placeholder` (Higgsfield stand-in) → `final` (official art team). `needs-rework` flags an asset that must be regenerated.

**Canon note (Vision §3.3 / §3.5):** every character and kaiju design below is an *inference*, not confirmed canon — including Kest's kitsune features, Tengi's tengu-mask, and Goryo's guardian role. All of it is placeholder-by-contract, pending the lore bible and rights-holder review.

**Derived assets:** a row marked "derived from X" flips to `needs-rework` in the same edit whenever X's file or status changes (ART_DIRECTION §4).

Scope: this list covers the **vertical slice** (Vision Goal 1: full touch combat between Kest and Tengi, one living harbor stage with a Khulandra event, one Horrific Ending each, one motion-comic sequence) plus core UI and audio. It grows with the design brief. **Animation pipeline: decided (D-011) and listed.** Character animation ships as per-move rigged clip GLBs generated on Kest's rig and retargeted to both champions via the shared Meshy humanoid skeleton (ARCHITECTURE.md §Animation pipeline). The clip library is now **11 Kest clips**: idle, walk, punch, block, hit, death (session 3) + special, airrake, airslam, parry, backdash (session 8 — the deferred combat-art library from D-015, now generated). Only `special` is wired into the runtime `FighterAnimator` so far; airrake/airslam/parry/backdash are generated + synced and one-line-wireable pending an on-device look (see RoundManager.ClipFiles + STATUS).

## Concept art (reference only — never shipped in builds)

| ID | Canonical path | Spec | Status | Subject line |
|---|---|---|---|---|
| concept_style-test_fight-scene | assets/concept/style_test_fight_scene.png | 1920×1080 PNG | placeholder | Kest the werefox and Tengi the culler mid-duel on a ruined harbor pier at dusk, the colossal silhouette of Khulandra rising from the sea behind them, frame-breaking scale |
| concept_kest_sheet | assets/concept/kest_concept_sheet.png | 2048×1152 PNG | placeholder | character exploration sheet of Kest the werefox: lean rushdown fighter, part-transformed kitsune with fox ears, feral eyes, clawed hands, tattered traveling cloak, wisps of spectral teal fox-fire |
| concept_tengi_sheet | assets/concept/tengi_concept_sheet.png | 2048×1152 PNG | placeholder | character exploration sheet of Tengi the culler: towering grim figure in dark lacquered armor with a tengu-mask helm, crow-feather mantle, massive single-edged culling blade |
| concept_khulandra_event | assets/concept/khulandra_stage_event.png | 1920×1080 PNG | placeholder | Khulandra, abyssal leviathan kaiju, breaching a harbor with a wall of black water, scale so vast only part of the body fits the frame |
| concept_stage_harbor | assets/concept/stage_harbor_concept.png | 1920×1080 PNG | placeholder | ruined 2061 Tokyo harbor district at dusk: cracked concrete pier, half-sunken torii gate, kanji-lit ruins, distant coalition floodlights |

## Characters (vertical-slice roster: Kest, Tengi)

| ID | Canonical path | Spec | Status | Subject line |
|---|---|---|---|---|
| char_kest_apose | assets/characters/kest_apose.png | 1024×1536 PNG | placeholder | Kest the werefox, full body neutral A-pose: lean androgynous fighter, fox ears, clawed hands, fitted dark traveling garb with torn cloak, spectral teal fox-fire accents |
| char_kest_model | assets/characters/kest_model.glb | GLB mesh — derived from char_kest_apose via `generate_3d` | placeholder | — |
| char_kest_portrait | assets/characters/kest_portrait.png | 1024×1024 PNG, alpha | placeholder | Kest the werefox, head and shoulders, sly feral grin, one eye catching spectral teal light |
| char_tengi_apose | assets/characters/tengi_apose.png | 1024×1536 PNG | placeholder | Tengi the culler, full body neutral A-pose: towering heavy fighter in dark lacquered plate, tengu-mask helm, crow-feather mantle, massive culling blade sheathed on the back |
| char_tengi_model | assets/characters/tengi_model.glb | GLB mesh — derived from char_tengi_apose via `generate_3d` | placeholder | — |
| char_tengi_portrait | assets/characters/tengi_portrait.png | 1024×1024 PNG, alpha | placeholder | Tengi the culler, head and shoulders, tengu-mask helm half in shadow, Blood Seal red glint in the eye slits |
| char_kest_anim_idle | assets/characters/kest_anim_idle.glb | animated GLB — derived from char_kest_apose (clip: Combat_Stance #89) | placeholder | — |
| char_kest_anim_walk | assets/characters/kest_anim_walk.glb | animated GLB — derived from char_kest_apose (clip: Casual_Walk #30) | placeholder | — |
| char_kest_anim_punch | assets/characters/kest_anim_punch.glb | animated GLB — derived from char_kest_apose (clip: Triple_Combo_Attack #105) | placeholder | — |
| char_kest_anim_block | assets/characters/kest_anim_block.glb | animated GLB — derived from char_kest_apose (clip: Block1 #138) | placeholder | — |
| char_kest_anim_hit | assets/characters/kest_anim_hit.glb | animated GLB — derived from char_kest_apose (clip: Hit_Reaction #178) | placeholder | — |
| char_kest_anim_death | assets/characters/kest_anim_death.glb | animated GLB — derived from char_kest_apose (clip: Dead #8) | placeholder | — |
| char_kest_anim_special | assets/characters/kest_anim_special.glb | animated GLB — derived from char_kest_apose (clip: Charged_Spell_Cast #125) | placeholder | — |
| char_kest_anim_airrake | assets/characters/kest_anim_airrake.glb | animated GLB — derived from char_kest_apose (clip: Jumping_Punch #457) | placeholder | — |
| char_kest_anim_airslam | assets/characters/kest_anim_airslam.glb | animated GLB — derived from char_kest_apose (clip: Leap_and_Punch #464) | placeholder | — |
| char_kest_anim_parry | assets/characters/kest_anim_parry.glb | animated GLB — derived from char_kest_apose (clip: Sword_Parry #147) | placeholder | — |
| char_kest_anim_backdash | assets/characters/kest_anim_backdash.glb | animated GLB — derived from char_kest_apose (clip: Back_Jump #468) | placeholder | — |

## Stage: Harbor Ruins (living stage with Khulandra event)

| ID | Canonical path | Spec | Status | Subject line |
|---|---|---|---|---|
| stage_harbor_sky | assets/stages/harbor_sky.png | 2560×1440 PNG | placeholder | far background: bruised dusk sky over a dark sea, storm cell on the horizon, faint silhouette of something vast beneath the waves |
| stage_harbor_mid | assets/stages/harbor_mid.png | 2560×1440 PNG, alpha | placeholder | midground layer: ruined harbor buildings, cranes, half-sunken torii gate, kanji-lit signage, no ground plane, no characters |
| stage_harbor_ground | assets/stages/harbor_ground.png | 2560×512 PNG, tiles horizontally | placeholder | foreground fight plane: cracked concrete pier surface strip, seamless horizontal tiling, debris and standing water detail |
| stage_harbor_ground_flooded | assets/stages/harbor_ground_flooded.png | 2560×512 PNG, tiles horizontally | placeholder | the same cracked pier strip after Khulandra's tide surge: ankle-deep black water, floating debris, rippled reflections — post-event state so the stage visibly changes |
| stage_khulandra_breach | assets/stages/khulandra_breach.png | 2048×2048 PNG, alpha | placeholder | Khulandra's head and foreclaw breaching upward with cascading black water, cut by the frame, for the mid-fight stage event (reused as the story intro's "THE KAIJU" beat overlay over `stages/harbor_sky`, again cropped by the frame per Pillar 2 — D-022) |

## Comic panels (story mode + Horrific Endings)

| ID | Canonical path | Spec | Status | Subject line |
|---|---|---|---|---|
| panel_ending_kest_01 | assets/panels/ending_kest_01.png | 1920×1080 PNG | placeholder | Horrific Ending splash: the fallen opponent dragged into a ring of spectral teal fox-fire, Kest's silhouette multiplying into vulpine shadows |
| panel_ending_tengi_01 | assets/panels/ending_tengi_01.png | 1920×1080 PNG | placeholder | Horrific Ending splash: Tengi's culling blade planted upright as a grave marker, crows descending, the defeated reduced to a shadow burned into the pier |
| panel_story_fourpillars_01 | assets/panels/story_fourpillars_01.png | 1920×1080 PNG | placeholder | motion-comic panel: 2061 Japan overrun — a coalition watchtower dwarfed under a kaiju-darkened sky, soldiers and scientists at human scale |
| panel_story_fourpillars_02 | assets/panels/story_fourpillars_02.png | 1920×1080 PNG | placeholder | motion-comic panel: a war room of world flags and flickering screens, the map of Japan burning district by district |
| panel_story_fourpillars_03 | assets/panels/story_fourpillars_03.png | 1920×1080 PNG | placeholder | motion-comic panel: two champions facing away from each other under rain, the same colossal shadow falling over both |

## UI

| ID | Canonical path | Spec | Status | Subject line |
|---|---|---|---|---|
| ui_emblem | assets/ui/emblem.png | 1024×1024 PNG, alpha | placeholder | circular ink emblem: a goryō spirit-flame burning inside a broken torii gate (no lettering — title is typeset in-engine) |
| ui_panel_frame | assets/ui/panel_frame.png | 512×512 PNG, alpha, 9-slice | placeholder | sharp-cornered comic panel frame, 3px ink rule with rough brush edge, empty center |
| ui_button_set | assets/ui/button_set.png | 1024×512 PNG, alpha | placeholder | sheet of two rectangular ink-framed buttons, normal and pressed states, Bone Paper fill, empty label areas |
| ui_hud_healthbar | assets/ui/hud_healthbar.png | 1024×128 PNG, alpha | placeholder | horizontal health bar frame in ink, Bone Paper fill zone, rough brush end caps |
| ui_hud_meter | assets/ui/hud_meter.png | 1024×128 PNG, alpha | placeholder | segmented special-ability meter frame in ink with spectral teal glow zones |
| ui_ability_card | assets/ui/ability_card.png | 512×768 PNG, alpha | placeholder | tarot-like ability card frame, ink border with folklore corner motifs, empty art window and caption strip |
| ui_ability_icons_kest | assets/ui/ability_icons_kest.png | 1536×512 PNG, alpha, 3 tiles | placeholder | row of three ability-card illustrations for Kest: a spectral fox-fire dash streak, a threefold phantom-claw rake, a ring of hunting fox shadows |
| ui_ability_icons_tengi | assets/ui/ability_icons_tengi.png | 1536×512 PNG, alpha, 3 tiles | placeholder | row of three ability-card illustrations for Tengi: the culling blade raised against a black sun, a descending culling arc, a wall of crow feathers |
| ui_icon_sheet | assets/ui/icon_sheet.png | 1024×256 PNG, alpha | placeholder | row of four single-weight ink glyph icons: pause, settings, tag-switch, block |
| ui_vs_screen | assets/ui/vs_screen.png | 1920×1080 PNG | placeholder | VS screen background: split ink wash, Blood Seal left, Goryō Flame right, empty portrait zones both sides (also the "THE CHAMPIONS" beat of the story intro — D-022) |
| ui_menu_bg | assets/ui/menu_bg.png | 2560×1440 PNG | placeholder | shared front-end backdrop for the multiplayer, character-select, and lobby screens: brooding ink-wash harbor at dusk with heavy negative space through the center for menu elements, no characters, no lettering (added session 6, D-017 — MultiplayerMenu/CharacterSelect/Lobby reference `ui/menu_bg` via AssetLib.SpriteOr, falling back to `stages/harbor_sky` until generated). Session 8b: also the fight-select screen backdrop and the "Tokyo harbor" beat of the story intro (D-022). |
| ui_key_art | assets/ui/key_art.png | 2048×1152 PNG (native 2688×1536, 16:9) | placeholder | main-menu backdrop, **promoted from `concept_key-art` (D-019)** so it can ship: two rival champions back to back, small in frame, the kaiju Goryo towering into storm clouds above a half-drowned 2061 Tokyo skyline (MainMenu.cs draws it as the title background; falls back to `stages/harbor_sky`). Also the opening "2061" beat of the story intro (D-022). |
| ui_roster_card | assets/ui/roster_card.png | 1024×276 PNG, alpha | placeholder | character-select roster slot: sharp-cornered comic-panel card frame in ink with a rough brush edge, Bone Paper fill, empty portrait window at the left and a blank name/tagline strip at the right, no lettering (added session 6, D-017 — CharacterSelectMenu tints this per selection; falls back to a plain fill until generated) |

## VFX

| ID | Canonical path | Spec | Status | Subject line |
|---|---|---|---|---|
| vfx_hit_spark | assets/vfx/hit_spark.png | 512×512 PNG, alpha | placeholder | radial ink-splash impact burst, Bone Paper core with Signal Amber fringe |
| vfx_ink_blood | assets/vfx/ink_blood.png | 512×512 PNG, alpha | placeholder | stylized Blood Seal ink-splatter burst, heavy droplets, screentone falloff |
| vfx_meter_flare | assets/vfx/meter_flare.png | 512×512 PNG, alpha | placeholder | spectral teal spirit-flame flare, wisping upward like a goryō |
| vfx_kest_foxfire | assets/vfx/kest_foxfire.png | 512×512 PNG, alpha | placeholder | Kest special-move effect: streaking spectral teal fox-fire trail with ember wisps |
| vfx_tengi_bladewave | assets/vfx/tengi_bladewave.png | 1024×512 PNG, alpha | placeholder | Tengi special-move effect: dark crescent blade wave edged in Blood Seal, trailing crow feathers |
| vfx_kaiju_shockwave | assets/vfx/kaiju_shockwave.png | 1024×256 PNG, alpha | placeholder | horizontal ground shockwave of dust, seawater, and debris for the Khulandra stage event |
| vfx_dash_streak | assets/vfx/dash_streak.png | 512×512 PNG, alpha | placeholder | dash / afterimage motion streak: horizontal spectral speed-trail with ghosting ink echoes (code tints it per fighter — teal for Kest, crimson for Tengi) |
| vfx_parry_spark | assets/vfx/parry_spark.png | 512×512 PNG, alpha | placeholder | perfect-guard burst: radial spectral spirit-flame flash with sharp concentric ink shards deflecting outward |
| vfx_impact_ring | assets/vfx/impact_ring.png | 512×512 PNG, alpha | placeholder | concussive impact ring: single expanding bone-white shockwave ring edged in black ink with screentone falloff, empty dark center |

## Audio

*Containers per D-007: music `.ogg` (seamless loop), SFX `.wav` (low latency). Convert on save if the generator returns another format.*

| ID | Canonical path | Spec | Status | Subject line |
|---|---|---|---|---|
| mus_title_theme | assets/audio/music/title_theme.ogg | 60–90s seamless loop | placeholder | title theme: slow taiko pulse under low brass and distorted shamisen, dread building to resolve |
| mus_fight_harbor | assets/audio/music/fight_harbor.ogg | 90–120s seamless loop | placeholder | fight track: driving taiko and dark electronics, rising intensity stinger layers |
| mus_story_fourpillars | assets/audio/music/story_fourpillars.ogg | 60–90s loop, low intensity | placeholder | motion-comic bed: sparse mournful shamisen and distant sirens over a low drone, room for narration |
| sfx_hit_light | assets/audio/sfx/hit_light.wav | <1s | placeholder | sharp light strike impact |
| sfx_hit_heavy | assets/audio/sfx/hit_heavy.wav | <1s | placeholder | crunching heavy blow with low thud |
| sfx_block | assets/audio/sfx/block.wav | <1s | placeholder | dull guarded impact, wood-and-metal |
| sfx_kest_special | assets/audio/sfx/kest_special.wav | 1–2s | placeholder | whooshing spectral flame dash with a vulpine snarl beneath |
| sfx_tengi_special | assets/audio/sfx/tengi_special.wav | 1–2s | placeholder | massive blade cleave with a murder of crows erupting |
| sfx_ending_sting | assets/audio/sfx/ending_sting.wav | 1–2s | placeholder | Horrific Ending smash-cut sting: abrupt taiko hit collapsing into ringing silence |
| sfx_khulandra_roar | assets/audio/sfx/khulandra_roar.wav | 2–4s | placeholder | abyssal leviathan roar, sub-bass heavy, water-choked |
| sfx_ui_tap | assets/audio/sfx/ui_tap.wav | <0.3s | placeholder | soft ink-brush tick for UI feedback |
| sfx_whiff | assets/audio/sfx/whiff.wav | <0.4s | planned | short air-cutting whoosh for a missed strike; punchy, mixed for mobile speakers (added session 4 — CombatSystem plays it on a whiffed attack; falls silent until generated) |
| vo_announcer_round_one | assets/audio/vo/announcer_round_one.wav | <2s | placeholder | announcer: "ROUND ONE" |
| vo_announcer_round_two | assets/audio/vo/announcer_round_two.wav | <2s | placeholder | announcer: "ROUND TWO" |
| vo_announcer_final_round | assets/audio/vo/announcer_final_round.wav | <2s | placeholder | announcer: "FINAL ROUND" |
| vo_announcer_fight | assets/audio/vo/announcer_fight.wav | <1s | placeholder | announcer: "FIGHT!" |
| vo_announcer_ko | assets/audio/vo/announcer_ko.wav | <2s | placeholder | announcer: "K.O.!" |
| vo_announcer_khulandra_rises | assets/audio/vo/announcer_khulandra_rises.wav | <3s | placeholder | announcer: "KHULANDRA RISES" |

## Fonts

*Not generated — sourced. License must be recorded before a font ships (ART_DIRECTION §3).*

| ID | Canonical path | Spec | Status | License | Notes |
|---|---|---|---|---|---|
| font_hud | assets/fonts/hud.ttf | condensed grotesque sans | placeholder | OFL (proposed: Barlow Condensed) | HUD, numbers, body UI text |
| font_display | assets/fonts/display.ttf | rough brush display face | placeholder | TBD — verify before ship | titles, Horrific Ending captions |

## Planned generation queue — Multiplayer (M6, D-017)

*Everything the multiplayer feature needs, ready to generate on the owner command **"generate necessary artwork"**. Nothing here is generated yet — the game ships today on the fallbacks noted in each row. When the command comes, generate in this order per the ART_DIRECTION §4 pipeline, then flip each row to `placeholder` and append its generation record below. The prompts are pre-composed to the §5 template (STYLE CORE + recipe + subject + avoid-list) — paste verbatim.*

**Nothing else is code-blocked on these:** `AssetLib.SpriteOr` already prefers the dedicated path and falls back, and `scripts/sync_assets.sh` already copies `assets/ui/*.png`, so each asset lights up on the next sync with zero code change.

1. **ui_menu_bg** (`assets/ui/menu_bg.png`, 2560×1440) — recipe: *Stage layer*. Prompt:
   > mature dark comic book illustration, heavy black ink lines, dramatic chiaroscuro lighting, gritty screentone shading, desaturated palette of ash gray and bone white with blood red and spectral teal accents, Japanese folklore meets near-future 2061 technothriller, cinematic composition, high contrast, wide 2D fighting game stage background, horizontal composition, clear flat ground plane, layered depth with atmospheric haze, no characters, brooding ink-wash harbor at dusk with heavy negative space through the center for menu elements, no characters, no lettering, no text, no watermark, no signature, no photorealism, no bright anime cel shading, no chibi proportions

2. **ui_roster_card** (`assets/ui/roster_card.png`, 512×256, alpha, 9-slice) — recipe: *UI element* → then `remove_background`. Prompt:
   > mature dark comic book illustration, heavy black ink lines, dramatic chiaroscuro lighting, gritty screentone shading, desaturated palette of ash gray and bone white with blood red and spectral teal accents, Japanese folklore meets near-future 2061 technothriller, cinematic composition, high contrast, flat game UI element, single-weight ink line style, on a plain solid background, sharp-cornered comic-panel card frame with a rough brush ink edge, bone paper fill, empty portrait window at the left and a blank name strip at the right, no lettering, no text, no watermark, no signature, no photorealism, no bright anime cel shading, no chibi proportions

### New-character art template (per champion added to CharacterRoster)

Adding a champion is code-ready today (one `CharacterDef`, D-017). Its **art** is these five rows — clone Kest's, swap `<id>`/subject, and the shared code picks them up by canonical path (portraits/models/anim GLBs are already synced by pattern; a champion may reuse Kest's `*_anim_*` clips per the D-011 slice rule, in which case only apose/model/portrait are new):

| Row | Path | Recipe | Feeds |
|---|---|---|---|
| `char_<id>_apose` | `assets/characters/<id>_apose.png` (1024×1536) | Character sheet | `generate_3d` → model |
| `char_<id>_model` | `assets/characters/<id>_model.glb` | (derived, `generate_3d`) | in-fight mesh |
| `char_<id>_portrait` | `assets/characters/<id>_portrait.png` (1024×1024, alpha) | Portrait | select + VS + HUD |
| `char_<id>_anim_*` | `assets/characters/<id>_anim_{idle,walk,punch,block,hit,death}.glb` | (rig clips, D-011) | animation (or reuse Kest's) |
| `ui_ability_icons_<id>` | `assets/ui/ability_icons_<id>.png` (1536×512, alpha, 3 tiles) | UI element | card icons (`IconKey`) |

## Generation records (append-only)

*One entry per generated asset, written immediately after generation (ART_DIRECTION §4 step 6):*

```
### <asset ID> — <date>
- Model: <higgsfield model used>
- Job ID: <higgsfield job/generation id>
- Seed: <seed, if the model reports one>
- Prompt: <full final prompt, verbatim>
- Notes: <candidates generated, which was picked, post-processing applied, owner approval if any>
```

### concept_style-test_fight-scene — 2026-07-18
- Model: recraft_v4_1 (standard, 2k, 16:9, palette param = the six ART_DIRECTION §2 hexes)
- Job ID: 9d7f9388-c431-4ed2-9d2e-c9a628050d66
- Seed: n/a (model does not report one)
- Prompt: mature dark comic book illustration, heavy black ink lines, dramatic chiaroscuro lighting, gritty screentone shading, desaturated palette of ash gray and bone white with blood red and spectral teal accents, Japanese folklore meets near-future 2061 technothriller, cinematic composition, high contrast, full illustrated scene, epic scale, human figures dwarfed by a colossal kaiju that breaks the frame, Kest the werefox and Tengi the culler mid-duel on a ruined harbor pier at dusk, the colossal silhouette of Khulandra rising from the sea behind them, frame-breaking scale, no text, no watermark, no signature, no photorealism, no bright anime cel shading, no chibi proportions
- Notes: 2 candidates (2nd job 4594c6b3-1985-4218-9215-5b5e8d5cdd6b, rejected — too flat/poster-like, weak chiaroscuro, off-brief weapon). Winner delivered 2688×1536 (≥ spec, same aspect). No post-processing. Style pre-approved by owner (D-009); self-reviewed against the style bible — this image is the cohesion anchor for the batch.

### concept_key-art — 2026-07-18
- Model: recraft_v4_1 (standard, 2k, 16:9, §2 palette param)
- Job ID: 85fbe371-7308-4314-bca4-145b3a286ba7
- Seed: n/a
- Prompt: STYLE CORE + concept recipe + row subject line + avoid-list, comma-joined (§5 template; verbatim in job params)
- Notes: 2 candidates (2nd job 447e77ce-b263-4152-91d9-19251c7b9b84, rejected — red-dominated beyond §2 ratio, off-palette yellow eye). Winner: spectral bone-white goryō-like kaiju, fisheye scale. 2688×1536, no post-processing.

### concept_kest_sheet — 2026-07-18
- Model: recraft_v4_1 (standard, 2k, 16:9, §2 palette param)
- Job ID: e3bdb2f4-c9f5-4159-b1f0-8d3363315a0a
- Seed: n/a
- Prompt: STYLE CORE + adapted character-exploration recipe ("character design exploration sheet, full body dynamic pose studies with three-quarter and profile views, plain light parchment background") + row subject line + avoid-list
- Notes: 1 candidate, accepted. Three dynamic poses, teal fox-fire, parchment ground. 2688×1536.

### concept_tengi_sheet — 2026-07-18
- Model: recraft_v4_1 (standard, 2k, 16:9, §2 palette param)
- Job ID: fc9d520b-d5f4-4376-802b-7083dab8464c
- Seed: n/a
- Prompt: STYLE CORE + adapted character-exploration recipe (as above) + row subject line + avoid-list
- Notes: 1 candidate, accepted. Four views, tengu-mask helm, crow-feather mantle, culling blade. 2688×1536.

### concept_khulandra_event — 2026-07-18
- Model: recraft_v4_1 (standard, 2k, 16:9, §2 palette param)
- Job ID: 11bfd194-0533-48fc-aed9-0c262309b586
- Seed: n/a
- Prompt: STYLE CORE + concept recipe + row subject line + avoid-list (§5 template)
- Notes: 1 candidate, accepted. Tsunami-scale breach, human silhouette foreground. 2688×1536.

### concept_stage_harbor — 2026-07-18
- Model: recraft_v4_1 (standard, 2k, 16:9, §2 palette param)
- Job ID: 4eb001fd-ce2e-4d13-81fa-4b39079d553a
- Seed: n/a
- Prompt: STYLE CORE + adapted wide-establishing concept recipe ("full illustrated scene, wide establishing shot, layered depth with atmospheric haze") + row subject line + avoid-list
- Notes: 1 candidate, accepted. Torii foreground, kanji-lit skyline, dusk. Stylized glyph signage only (no real text). 2688×1536.

### char_kest_apose — 2026-07-18
- Model: recraft_v4_1 (standard, 2k, 2:3, §2 palette, background_color #CFCFCF)
- Job ID: 37123379-042e-4bc0-950b-305caef34a85
- Seed: n/a
- Prompt: STYLE CORE + character-sheet recipe + row subject line + avoid-list (§5 template; verbatim in job params)
- Notes: 1 candidate, accepted. 1664×2560 (2:3, ≥ spec). Feeds generate_3d for char_kest_model.

### char_tengi_apose — 2026-07-18
- Model: recraft_v4_1 (standard, 2k, 2:3, §2 palette, background_color #CFCFCF)
- Job ID: 6a66d95a-3aa1-48df-a70d-02ead90bbdf9
- Seed: n/a
- Prompt: STYLE CORE + character-sheet recipe + row subject line + avoid-list
- Notes: 1 candidate, accepted. 1664×2560. Feeds generate_3d for char_tengi_model.

### char_kest_portrait — 2026-07-18
- Model: recraft_v4_1 (standard, 2k, 1:1, §2 palette, background_color #15171C) + image_background_remover
- Job ID: b5e77f7a-32ad-4c59-bf96-bfac3e52bf15 (gen), af7eaa42-6659-4c87-992a-8f5d6187a5a4 (cutout)
- Seed: n/a
- Prompt: STYLE CORE + portrait recipe + row subject line + avoid-list
- Notes: 1 candidate, accepted. Saved cutout: 2048×2048 RGBA.

### char_tengi_portrait — 2026-07-18
- Model: recraft_v4_1 (standard, 2k, 1:1, §2 palette, background_color #15171C) + image_background_remover
- Job ID: ce2128dd-6caa-47f3-b3c5-aa62ea66dc59 (gen), d10ef383-309f-44c7-b435-1e9ace1de838 (cutout)
- Seed: n/a
- Prompt: STYLE CORE + portrait recipe + row subject line + avoid-list
- Notes: 1 candidate, accepted. Saved cutout: 2048×2048 RGBA.

### stage_harbor_sky — 2026-07-18
- Model: recraft_v4_1 (standard, 2k, 16:9, §2 palette)
- Job ID: 56213723-447f-4f8c-b3e9-b8c3b3ee2e6a
- Seed: n/a
- Prompt: STYLE CORE + stage-layer recipe + row subject line + avoid-list (§5 template; verbatim in job params)
- Notes: 1 candidate, accepted. Saved 2688×1536 (≥ spec, same aspect).

### stage_harbor_mid — 2026-07-18
- Model: recraft_v4_1 (standard, 2k, 16:9, §2 palette, background_color #FFFFFF) + image_background_remover
- Job ID: 6cb9ad83-b542-4de4-a4ae-0e9b7f4c2324 (gen), f1c2b3af-f65f-4086-b8ae-6e613ff070b1 (cutout)
- Seed: n/a
- Prompt: STYLE CORE + stage-layer recipe (adapted: skyline silhouetted on plain white, no ground plane) + row subject line + avoid-list
- Notes: 1 candidate, accepted. Saved cutout 2688×1536 RGBA.

### stage_harbor_ground — 2026-07-18
- Model: recraft_v4_1 (standard, 2k, 16:9, reduced palette #15171C/#E9E2D0/#5A636E)
- Job ID: c01efcb0-629e-4fa1-ab4a-0d63ffcd0d2d
- Seed: n/a
- Prompt: STYLE CORE (chiaroscuro clause dropped for a flat texture band) + strip recipe: "horizontal strip composition, continuous flat surface texture band... weathered concrete pier deck... muted monochrome grays only, no red, no water" + avoid-list
- Notes: 2nd attempt — first job 24aacd96-7d86-4e6e-b76c-44eeb8e9d657 rejected (blood-red pooling far beyond §2 accent ratio for the default stage state). Reduced palette recorded as deliberate art-direction choice. Post-processing: center-band crop to 5:1, offset+cosine-blend seamless tiling pass (PIL), resized to 2560×512.

### stage_harbor_ground_flooded — 2026-07-18
- Model: recraft_v4_1 (standard, 2k, 16:9, reduced palette + #3FB08F)
- Job ID: d91fa1f8-0ded-4af5-a929-c993bc7c90a0
- Seed: n/a
- Prompt: as stage_harbor_ground but "flooded concrete pier deck after a tidal surge: thin sheet of dark seawater, spectral teal ripple reflections, floating debris" + avoid-list
- Notes: 2nd attempt — first job 580e239b-ddc8-4ce4-bc90-2470cc606966 rejected (came out dry, stray red wedge). Same crop/tiling pass, 2560×512.

### stage_khulandra_breach — 2026-07-18
- Model: recraft_v4_1 (standard, 2k, 1:1, §2 palette, background_color #FFFFFF) + image_background_remover
- Job ID: e276d3b8-b6dc-428b-8c6e-e203073d6ddf (gen), faa92e19-051c-4d15-ad2f-af0e2c8f90a4 (cutout)
- Seed: n/a
- Prompt: STYLE CORE + subject (Khulandra head and foreclaw breaching, cut off by top of frame, isolated on plain white) + avoid-list
- Notes: 1 candidate, accepted. Saved cutout 2048×2048 RGBA.

### char_kest_model — 2026-07-18
- Model: image_to_3d (Meshy) — should_texture, enable_rigging, pose_mode a-pose, rigging_height_meters 1.8
- Job ID: c1732488-93d9-459a-a607-6a92c6363aec
- Seed: 20260718
- Prompt: none (derived from char_kest_apose, job 37123379-042e-4bc0-950b-305caef34a85)
- Notes: rigged humanoid skeleton + textures, GLB 9.6 MB. Derived asset: flips to needs-rework if char_kest_apose changes (ART_DIRECTION §4).

### char_tengi_model — 2026-07-18
- Model: image_to_3d (Meshy) — should_texture, enable_rigging, pose_mode a-pose, rigging_height_meters 2.4
- Job ID: e2928fa9-6251-43e4-ba2e-4bb92e5799ee
- Seed: 20260718
- Prompt: none (derived from char_tengi_apose, job 6a66d95a-3aa1-48df-a70d-02ead90bbdf9)
- Notes: rigged humanoid skeleton + textures, GLB 10.2 MB. Derived asset rule applies.

### panel_ending_kest_01 — 2026-07-18
- Model: recraft_v4_1 (standard, 2k, 16:9, §2 palette)
- Job ID: e5e0f1d8-a372-4aac-8938-0b2542a66f72
- Seed: n/a
- Prompt: STYLE CORE + comic-panel recipe + row subject line + avoid-list (§5 template; verbatim in job params)
- Notes: 1 candidate, accepted. 2688×1536 (≥ spec). Pillar 4 check passed: horror carried by consequence and framing, no explicit gore.

### panel_ending_tengi_01 — 2026-07-18
- Model: recraft_v4_1 (standard, 2k, 16:9, §2 palette)
- Job ID: c2f01394-fb0c-445f-8530-07408f4d3cf1
- Seed: n/a
- Prompt: STYLE CORE + comic-panel recipe + row subject line + avoid-list (§5 template; verbatim in job params)
- Notes: 1 candidate, accepted. 2688×1536 (≥ spec). Pillar 4 check passed: horror carried by consequence and framing, no explicit gore.

### panel_story_fourpillars_01 — 2026-07-18
- Model: recraft_v4_1 (standard, 2k, 16:9, §2 palette)
- Job ID: 93f65181-85bc-4a7a-8539-942a8da669d6
- Seed: n/a
- Prompt: STYLE CORE + comic-panel recipe + row subject line + avoid-list (§5 template; verbatim in job params)
- Notes: 1 candidate, accepted. 2688×1536 (≥ spec). Pillar 4 check passed: horror carried by consequence and framing, no explicit gore.

### panel_story_fourpillars_02 — 2026-07-18
- Model: recraft_v4_1 (standard, 2k, 16:9, §2 palette)
- Job ID: ff412987-1aa2-46e1-9bb6-0c6df1c08baf
- Seed: n/a
- Prompt: STYLE CORE + comic-panel recipe + row subject line + avoid-list (§5 template; verbatim in job params)
- Notes: 1 candidate, accepted. 2688×1536 (≥ spec). Pillar 4 check passed: horror carried by consequence and framing, no explicit gore.

### panel_story_fourpillars_03 — 2026-07-18
- Model: recraft_v4_1 (standard, 2k, 16:9, §2 palette)
- Job ID: 85ba3d97-d4a1-4482-a647-65f870396e12
- Seed: n/a
- Prompt: STYLE CORE + comic-panel recipe + row subject line + avoid-list (§5 template; verbatim in job params)
- Notes: 1 candidate, accepted. 2688×1536 (≥ spec). Pillar 4 check passed: horror carried by consequence and framing, no explicit gore.

### ui_emblem — 2026-07-18
- Model: recraft_v4_1 (2k 1:1, teal-reduced palette, white bg) — background remover where a cutout job is listed
- Job ID: 11763504-1418-47ea-b5f2-acc59284d547 (gen), 5d92e880-048a-467c-a830-8eb5b72c9f8f (cutout)
- Seed: n/a
- Prompt: STYLE CORE-derived UI/VFX prompt + row subject line + avoid-list (verbatim in job params)
- Notes: trim-fit to 1024×1024 RGBA

### ui_panel_frame — 2026-07-18
- Model: recraft_v4_1 (1k 1:1, ink palette, white bg) — background remover where a cutout job is listed
- Job ID: 5cc52378-d699-49fc-a4cc-10884b7290a8 (gen), bfddc02f-d5ea-4b0e-afc0-339c5690a722 (cutout)
- Seed: n/a
- Prompt: STYLE CORE-derived UI/VFX prompt + row subject line + avoid-list (verbatim in job params)
- Notes: trim-fit to 512×512 RGBA; interior ghost pixels cleared locally (PIL) so the frame center is truly empty

### ui_button_set — 2026-07-18
- Model: recraft_v4_1 (2k 1:1, white bg) — background remover where a cutout job is listed
- Job ID: c5db21e3-d7f9-4b15-b8fa-dbc199d61e59 (gen), f1d8a834-77b7-4e44-943c-25f9db8e28b1 (cutout)
- Seed: n/a
- Prompt: STYLE CORE-derived UI/VFX prompt + row subject line + avoid-list (verbatim in job params)
- Notes: trim-fit to 1024×512 RGBA; two stacked states (normal top, pressed bottom)

### ui_hud_healthbar — 2026-07-18
- Model: recraft_v4_1 (2k 16:9, white bg) — background remover where a cutout job is listed
- Job ID: a73b6b28-d6dd-4dd8-a502-3055899c788b (gen), 346f1d80-45c1-4696-8106-58f219a1e47c (cutout)
- Seed: n/a
- Prompt: STYLE CORE-derived UI/VFX prompt + row subject line + avoid-list (verbatim in job params)
- Notes: trim-fit to 1024×128 RGBA

### ui_hud_meter — 2026-07-18
- Model: recraft_v4_1 (2k 16:9, white bg) — background remover where a cutout job is listed
- Job ID: 32c5c3d5-4c91-4561-b82d-b600b9cc5d3d (gen), 795a278b-1bcb-41b5-98b2-c98ff982339c (cutout)
- Seed: n/a
- Prompt: STYLE CORE-derived UI/VFX prompt + row subject line + avoid-list (verbatim in job params)
- Notes: trim-fit to 1024×128 RGBA

### ui_ability_card — 2026-07-18
- Model: recraft_v4_1 (2k 2:3, amber-accent palette, white bg) — background remover where a cutout job is listed
- Job ID: 91b1af2c-6b58-4e16-b799-97d11e0041dc (gen), 4ad5b421-ae7b-45e3-ae7e-ab6d6b9fd799 (cutout)
- Seed: n/a
- Prompt: STYLE CORE-derived UI/VFX prompt + row subject line + avoid-list (verbatim in job params)
- Notes: trim-fit to 512×768 RGBA

### ui_ability_icons_kest — 2026-07-18
- Model: recraft_v4_1 (2k 16:9 three-panel row, white bg) — background remover where a cutout job is listed
- Job ID: cfdf8533-138c-419f-a483-b5ba32b4ace2 (gen), 5d2fb7b3-45f2-4cdc-a46e-57609e6288dc (cutout)
- Seed: n/a
- Prompt: STYLE CORE-derived UI/VFX prompt + row subject line + avoid-list (verbatim in job params)
- Notes: trim-fit to 1536×512 RGBA

### ui_ability_icons_tengi — 2026-07-18
- Model: recraft_v4_1 (2k 16:9 three-panel row, white bg) — background remover where a cutout job is listed
- Job ID: b57b6ca5-8ec0-4bb2-91cf-3c8edc2737fb (gen), 034338c3-4a1a-4b3c-b77d-3e16d5af5c84 (cutout)
- Seed: n/a
- Prompt: STYLE CORE-derived UI/VFX prompt + row subject line + avoid-list (verbatim in job params)
- Notes: trim-fit to 1536×512 RGBA

### ui_icon_sheet — 2026-07-18
- Model: recraft_v4_1 (1k 16:9, single-color, white bg) — background remover where a cutout job is listed
- Job ID: 7b4f8d02-079c-4b59-8939-1373f416ac1c (gen, 2nd attempt), 06c4c923-0ddc-4480-9e0a-7044a08f4c54 (cutout)
- Seed: n/a
- Prompt: STYLE CORE-derived UI/VFX prompt + row subject line + avoid-list (verbatim in job params)
- Notes: 1st attempt a4691e2f-870f-4cc6-b19e-59eba5f5aff4 (model_type vector) returned SVG — rejected, regenerated as raster. Trim-fit to 1024×256 RGBA

### ui_vs_screen — 2026-07-18
- Model: recraft_v4_1 (2k 16:9 full-bleed) — background remover where a cutout job is listed
- Job ID: b8ea2258-5bf6-4f9e-912d-d25988b795a4
- Seed: n/a
- Prompt: STYLE CORE-derived UI/VFX prompt + row subject line + avoid-list (verbatim in job params)
- Notes: saved as-is, 2688×1536

### vfx_hit_spark — 2026-07-18
- Model: recraft_v4_1 (1k 1:1, black bg) — background remover where a cutout job is listed
- Job ID: 49de5f56-4ac9-4267-bba9-48ffe018ec03
- Seed: n/a
- Prompt: STYLE CORE-derived UI/VFX prompt + row subject line + avoid-list (verbatim in job params)
- Notes: luminance-keyed to alpha locally (additive glow), resized 512×512

### vfx_ink_blood — 2026-07-18
- Model: recraft_v4_1 (1k 1:1, white bg) — background remover where a cutout job is listed
- Job ID: 0c15c61f-5d13-49d2-ab91-93983a7b1a10 (gen), cd7b47bc-46a2-4b56-88e4-862fa3882850 (cutout)
- Seed: n/a
- Prompt: STYLE CORE-derived UI/VFX prompt + row subject line + avoid-list (verbatim in job params)
- Notes: trim-fit to 512×512 RGBA

### vfx_meter_flare — 2026-07-18
- Model: recraft_v4_1 (1k 1:1, black bg) — background remover where a cutout job is listed
- Job ID: bdc11aba-02b6-4b91-80bf-175c5913a421
- Seed: n/a
- Prompt: STYLE CORE-derived UI/VFX prompt + row subject line + avoid-list (verbatim in job params)
- Notes: luminance-keyed to alpha, 512×512

### vfx_kest_foxfire — 2026-07-18
- Model: recraft_v4_1 (1k 1:1, black bg) — background remover where a cutout job is listed
- Job ID: 23224b75-80c5-4bdf-99b3-bacbaea86353
- Seed: n/a
- Prompt: STYLE CORE-derived UI/VFX prompt + row subject line + avoid-list (verbatim in job params)
- Notes: luminance-keyed to alpha, 512×512

### vfx_tengi_bladewave — 2026-07-18
- Model: recraft_v4_1 (1k 16:9, white bg) — background remover where a cutout job is listed
- Job ID: dc5affd3-2d87-4d51-bbb4-ef1a428a9e4c (gen), d334fab1-6e46-4fc4-9ecc-8e568c92101c (cutout)
- Seed: n/a
- Prompt: STYLE CORE-derived UI/VFX prompt + row subject line + avoid-list (verbatim in job params)
- Notes: trim-fit to 1024×512 RGBA

### vfx_kaiju_shockwave — 2026-07-18
- Model: recraft_v4_1 (1k 16:9, white bg) — background remover where a cutout job is listed
- Job ID: 196d0899-6995-4819-9899-cee392d80e47 (gen), 787b1c9c-0281-4d36-9ccf-c2b9733b6bb7 (cutout)
- Seed: n/a
- Prompt: STYLE CORE-derived UI/VFX prompt + row subject line + avoid-list (verbatim in job params)
- Notes: trim-fit to 1024×256 RGBA

### vfx_dash_streak — 2026-07-19 (session 5, D-015)
- Model: recraft_v4_1 (1k 1:1, black bg), 1.25 credits
- Job ID: 29d3776e-049f-4266-b1af-ecdfa131fed6
- Seed: n/a
- Prompt: `mature dark comic book illustration, heavy black ink lines, dramatic chiaroscuro lighting, gritty screentone shading, desaturated palette of ash gray and bone white with blood red and spectral teal accents, Japanese folklore meets near-future 2061 technothriller, cinematic composition, high contrast, stylized 2D fight effect, ink splash energy, centered on a plain solid black background, horizontal motion-streak afterimage, spectral teal speed trail with ghosting echoes and ink smear, the trailing wake of a fighter dashing, glowing against pure black, no text, no watermark, no signature, no photorealism, no bright anime cel shading, no chibi proportions`
- Notes: value-keyed to alpha locally (Pillow, per-pixel max(R,G,B), black floor 20), resized 512×512 RGBA. Code tints it per fighter (CombatSystem.SpawnDash uses Fighter.Theme).

### vfx_parry_spark — 2026-07-19 (session 5, D-015)
- Model: recraft_v4_1 (1k 1:1, black bg), 1.25 credits
- Job ID: 22a14553-1f7d-4edd-8d10-36f30665e450
- Seed: n/a
- Prompt: `...STYLE CORE + VFX recipe..., radial perfect-guard burst, spectral teal spirit-flame flash with sharp concentric ink shards deflecting outward from a bright core, glowing against pure black, ...avoid-list` (full text = STYLE CORE, VFX recipe, this subject, universal avoid-list)
- Notes: value-keyed to alpha locally (Pillow, black floor 20), 512×512 RGBA. Used by CombatSystem parry branch (fallback meter_flare).

### vfx_impact_ring — 2026-07-19 (session 5, D-015)
- Model: recraft_v4_1 (1k 1:1, black bg), 1.25 credits
- Job ID: 11fad325-2f27-40e0-9c40-76dd493d7727
- Seed: n/a
- Prompt: `...STYLE CORE + VFX recipe..., a single expanding concussive impact ring, bone-white shockwave ring edged in black ink with screentone falloff, empty dark center, glowing against pure black, ...avoid-list`
- Notes: value-keyed to alpha locally (Pillow, black floor 20), 512×512 RGBA. Spawned on Heavy/Special clean hits (fallback hit_spark).

### font_hud — 2026-07-18
- Model: n/a (sourced, not generated)
- Job ID: n/a
- Seed: n/a
- Prompt: n/a
- Notes: Barlow Condensed SemiBold, SIL OFL 1.1, fetched from google/fonts (ofl/barlowcondensed). Saved as assets/fonts/hud.ttf.

### font_display — 2026-07-18
- Model: n/a (sourced, not generated)
- Job ID: n/a
- Seed: n/a
- Prompt: n/a
- Notes: Shojumaru Regular, SIL OFL 1.1, fetched from google/fonts (ofl/shojumaru) — rough brush display face with Japanese-folklore flavor, fits ART_DIRECTION §3. Saved as assets/fonts/display.ttf.

### char_kest_anim_* (idle/walk/punch/block/hit/death) — 2026-07-18
- Model: image_to_3d (Meshy) — texture+rig+animation, pose_mode a-pose, height 1.8, rig action library per D-011
- Job IDs: idle 00d8048b-ea9c-4a83-b417-5c29089eb9b6 (action 89 Combat_Stance), walk cbfa5f60-e45d-4c88-8148-dcc3b54b4d18 (30 Casual_Walk), punch 3104d352-2d7d-415f-b073-ec3a80b65d6a (105 Triple_Combo_Attack), block 938ab611-ef35-4dbe-9140-20076189e209 (138 Block1), hit 02626411-fcaf-4ed5-b021-4322fba85fd2 (178 Hit_Reaction), death b180c73e-999b-4829-b73e-93bd37469a81 (8 Dead)
- Seed: 20260718 (all)
- Prompt: none (derived from char_kest_apose, job 37123379-042e-4bc0-950b-305caef34a85)
- Notes: 6 of the 7 D-011 clips; "special attack" intentionally not generated (50-credit buffer, D-013 budget guard) — Animator falls back to punch + VFX overlay. Clips retarget onto Tengi via the shared Meshy humanoid skeleton. Derived-asset rule applies to all six.

### mus_title_theme / mus_fight_harbor / mus_story_fourpillars — 2026-07-18
- Model: sonilo_music (75 s each), generated inside the game-build pipeline (D-013 satisfying D-010)
- Job IDs: e3547d3b-f9fc-45f3-ae45-e9154fcdf275, 1b762773-3f07-410c-af92-c9b951729e0a, b85b6915-3cda-46b9-a526-694d12bea7b3
- Seed: n/a
- Prompt: per manifest subject lines + "instrumental, loopable"
- Notes: delivered m4a, converted to .ogg (libvorbis q5, D-007) via ffmpeg. Loop points not hand-trimmed — flag for polish pass.

### sfx_* (8 rows) — 2026-07-18
- Model: mirelo_text_to_audio (1-4 s), game-build pipeline
- Job IDs: hit_light 03ee1ecd, hit_heavy cb8a6ff2, block b51a02ba, kest_special 73162ca0, tengi_special dbdfad7e, ending_sting 591d8d43, khulandra_roar 980695e0, ui_tap b09f2166
- Seed: n/a
- Prompt: per manifest subject lines
- Notes: delivered mp3, converted to 44.1 kHz .wav (D-007) via ffmpeg.

### vo_announcer_* (6 rows) — 2026-07-18
- Model: seed_audio TTS (default preset voice), game-build pipeline
- Job IDs: round_one 507f7add, round_two 29c41305, final_round 7d470b77, fight b805ebe1, ko a9a0cc73, khulandra_rises afef7b1a
- Seed: n/a
- Prompt: the literal banner strings from DESIGN_BRIEF.md
- Notes: delivered 24 kHz wav, resampled to 44.1 kHz. Voice is the default preset — regenerate with a chosen preset for a graver announcer read if wanted.

### ui_menu_bg — 2026-07-25 (session 7, D-017 planned queue)
- Model: recraft_v4_1 (standard, 2k, 16:9, §2 palette `colors`, no background_color)
- Job ID: ef2b5a28-6158-4977-873d-1068a17c580c (winner); 042e72c2-7467-45a2-9e2b-8847aeb2b712 (2nd candidate, rejected)
- Seed: n/a
- Prompt: mature dark comic book illustration, heavy black ink lines, dramatic chiaroscuro lighting, gritty screentone shading, desaturated palette of ash gray and bone white with blood red and spectral teal accents, Japanese folklore meets near-future 2061 technothriller, cinematic composition, high contrast, wide 2D fighting game stage background, horizontal composition, clear flat ground plane, layered depth with atmospheric haze, no characters, brooding ink-wash harbor at dusk with heavy negative space through the center for menu elements, no characters, no lettering, no text, no watermark, no signature, no photorealism, no bright anime cel shading, no chibi proportions
- Notes: 2 candidates. Winner ef2b5a28 chosen for a clean dark central negative-space void (over a bone-lit pier plane) that keeps overlaid menu/roster/lobby UI readable, with red/teal accents pushed to the edges as stage framing (Sumi/Ash/Bone still ~70%). Rejected 042e72c2 — busy mid-frame skyline + a large saturated bottom-right Blood-Seal mass that would fight overlaid text. Saved native 2688×1536 (≥ 2560×1440 spec, same 16:9 aspect), RGB, no post-processing — same convention as stage_harbor_sky. Prompt pasted verbatim from the ASSET_MANIFEST *Planned generation queue*. Credits: 2000 (team plan) — the refill STATUS/D-015 gated on; ≈16 cr (2 × 2k candidates).

### ui_roster_card — 2026-07-25 (session 7, D-017 planned queue)
- Model: recraft_v4_1 (standard, 2k, 16:9, §2 palette `colors`, background_color #FFFFFF) + image_background_remover
- Job ID: 15f42268-8230-466f-a99b-b8d36cbd14d4 (winner gen), 7e5b6326-1c4a-4e84-ab0e-722ab81b5324 (cutout); 9ce68690-872a-4b68-be4d-3b8f80f51b0d (2nd re-gen candidate, rejected)
- Seed: n/a
- Prompt: mature dark comic book illustration, heavy black ink lines, gritty screentone shading, high contrast, desaturated palette of ash gray and bone white with blood red and spectral teal accents, flat game UI element, single-weight ink line style, on a plain solid white background, an empty blank horizontal roster card frame, sharp-cornered comic-panel border with a rough brush ink edge, bone paper fill inside, a blank empty rectangular portrait slot on the left side and a blank empty horizontal name bar on the right side, completely empty flat template, nothing inside the frame, no characters, no figure, no person, no face, no creature, no portrait, no illustration inside, no lettering, no text, no watermark, no signature, no photorealism, no bright anime cel shading, no chibi proportions
- Notes (ui_roster_card): The verbatim queue prompt's "portrait window" phrasing made the FIRST pass paint a full champion INTO the card (rejected jobs 22665830-ca21-4ab9-b92c-25a5bcb64c5a, 6ddab9c7-1716-4757-b0f6-72ffea6622c6) — wrong both functionally (this is a reusable frame the game overlays real portraits onto) and canon-wise (designs are inferences). Re-gen with a hardened emptiness-forcing subject + avoid-list (above) produced clean empty templates. Winner 15f42268: square portrait window (left) + horizontal name bar (right), rough-brush comic-panel border. remove_background keyed the outer white to alpha while the enclosed windows stayed opaque (verified: outer α=0, windows/body α=255, no holes). Trimmed to the opaque content bbox (2257×609) and downscaled to 1024×276 RGBA (native ≈3.71:1; 2k was over-spec for a card downscaled to 1024 — kept for cleaner cutout edges). **Spec revised** from the aspirational "512×256 / 9-slice": CharacterSelectMenu draws it as a *Simple* Image with `preserveAspect=false`, stretched to the ~3.9:1 select slot and tinted per selection (CardOff dark / CardOnYou teal / CardOnFoe crimson) with the portrait + name/tagline drawn on top — so a wide ~3.7:1 card is correct and 9-slice does not apply. ≈33 cr this asset (4 × 2k gens + cutout). Session-7 total this batch: 2000 → 1951 = 49 cr.

### ui_key_art — 2026-07-25 (session 8, promoted from concept_key-art, D-019)
- Model: recraft_v4_1 (unchanged — no regeneration; this is a reclassification, not new art)
- Job ID: 85fbe371-7308-4314-bca4-145b3a286ba7 (original concept_key-art generation, 2026-07-18)
- Seed: n/a
- Prompt: STYLE CORE + concept recipe + concept_key-art subject line + avoid-list (§5 template; verbatim in original job params — see the `concept_key-art — 2026-07-18` record above)
- Notes: **No new credits spent.** The DESIGN_BRIEF always named `key_art.png` as the main-menu background, but the asset was classified `concept_key-art` and `scripts/sync_assets.sh` deliberately excludes `assets/concept/` from shipping, so `MainMenu.cs` had to substitute `stages/harbor_sky` (a standing open question since D-014). Per owner decision **D-019**, the master was `git mv`'d `assets/concept/key_art.png` → `assets/ui/key_art.png` (file content unchanged, native 2688×1536 RGB), the manifest row moved Concept → UI as `ui_key_art`, and `MainMenu.cs` now loads `ui/key_art` (fallback `stages/harbor_sky`). `sync_assets.sh` already globs `assets/ui/*.png`, so it now ships with zero script change. The `concept_key-art` row is retired (its 2026-07-18 generation record above is retained as provenance).

### char_kest_anim_{special,airrake,airslam,parry,backdash} — 2026-07-25 (session 8, deferred combat-art library, D-021)
- Model: image_to_3d (Meshy) — should_texture + enable_rigging + enable_animation, pose_mode a-pose, rigging_height_meters 1.8, rig action library per D-011
- Job IDs: special 0cbeb8ba-9e3c-4eef-8430-7befebb8a952 (action 125 Charged_Spell_Cast), airrake 76319a2e-bbf7-42e7-86d6-856e0c15ebd9 (457 Jumping_Punch), airslam 0749fda7-7b85-433f-bbc4-3c7668b23f08 (464 Leap_and_Punch), parry c0753191-6f84-4b94-bc1a-c2bed0346dba (147 Sword_Parry), backdash a8d64577-ae40-4957-a58c-f8243938cd22 (468 Back_Jump)
- Seed: 20260718 (all — same as the session-3 clip set, so they retarget onto the same shared skeleton)
- Prompt: none (all derived from char_kest_apose, job 37123379-042e-4bc0-950b-305caef34a85, fed by job_id as the image reference)
- Notes: Completes the combat-art library D-015 deferred on the 50-credit budget guard, unblocked by the credit refill (owner's session-8 command with the exact move scope). Rig actions chosen by best-fit read against the in-game move (special = a channelled charged cast, matching the card-cast VFX; airrake = a quick aerial punch; airslam = a committed leaping strike; parry = a crisp deflect; back-dash = an evasive backward hop). **First back-dash attempt failed** on action **605 Back_Jump_inplace** (the `_inplace` post-processed variants appear non-generatable — 0 credits charged) → retried on base **468 Back_Jump**. All 5 delivered valid glTF-binary v2, ~10.5–11 MB each (consistent with the session-3 rig models). **190 cr** (5 × 38; the failed attempt was not billed): 1951 → 1761. Derived-asset rule applies to all five (flip to needs-rework if char_kest_apose changes). Wiring: `special` is loaded into `FighterAnimator` via `RoundManager.ClipFiles` (the code already calls `Anim.Play("special")`, which previously fell back to `punch`); airrake/airslam/parry/backdash are synced and documented one-line-wireable there, left unloaded pending an on-device retarget/root-motion look (Back_Jump carries root translation — confirm it doesn't double-move vs the sim dash before wiring). Placeholder/inference by contract like every character asset.
