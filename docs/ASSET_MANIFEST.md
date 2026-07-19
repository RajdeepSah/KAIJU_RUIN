# ASSET MANIFEST — Kaiju Ruin

*The ledger of every art, audio, and font asset. One row per asset. No file exists in `assets/` without a row here; no row changes status unless the file actually changed (verify with `scripts/check_manifest.sh`). Prompts are composed per the template in ART_DIRECTION.md §5; the pipeline and full replacement contract live in ART_DIRECTION.md §4 — this doc only states: overwrite the file at the canonical path, flip the status, record the date.*

**Statuses:** `planned` → `placeholder` (Higgsfield stand-in) → `final` (official art team). `needs-rework` flags an asset that must be regenerated.

**Canon note (Vision §3.3 / §3.5):** every character and kaiju design below is an *inference*, not confirmed canon — including Kest's kitsune features, Tengi's tengu-mask, and Goryo's guardian role. All of it is placeholder-by-contract, pending the lore bible and rights-holder review.

**Derived assets:** a row marked "derived from X" flips to `needs-rework` in the same edit whenever X's file or status changes (ART_DIRECTION §4).

Scope: this list covers the **vertical slice** (Vision Goal 1: full touch combat between Kest and Tengi, one living harbor stage with a Khulandra event, one Horrific Ending each, one motion-comic sequence) plus core UI and audio. It grows with the design brief. **Known gap, owned by the design brief:** character *animation* (idle, tap-chain lights, swipe heavies/launcher, block, hit-react, KO per champion) cannot be asset-listed until the animation sourcing decision is made — see ARCHITECTURE.md §Animation pipeline.

## Concept art (reference only — never shipped in builds)

| ID | Canonical path | Spec | Status | Subject line |
|---|---|---|---|---|
| concept_style-test_fight-scene | assets/concept/style_test_fight_scene.png | 1920×1080 PNG | placeholder | Kest the werefox and Tengi the culler mid-duel on a ruined harbor pier at dusk, the colossal silhouette of Khulandra rising from the sea behind them, frame-breaking scale |
| concept_key-art | assets/concept/key_art.png | 2048×1152 PNG | placeholder | two rival champions back to back, small in frame, the kaiju Goryo towering into storm clouds above a half-drowned 2061 Tokyo skyline |
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

## Stage: Harbor Ruins (living stage with Khulandra event)

| ID | Canonical path | Spec | Status | Subject line |
|---|---|---|---|---|
| stage_harbor_sky | assets/stages/harbor_sky.png | 2560×1440 PNG | placeholder | far background: bruised dusk sky over a dark sea, storm cell on the horizon, faint silhouette of something vast beneath the waves |
| stage_harbor_mid | assets/stages/harbor_mid.png | 2560×1440 PNG, alpha | placeholder | midground layer: ruined harbor buildings, cranes, half-sunken torii gate, kanji-lit signage, no ground plane, no characters |
| stage_harbor_ground | assets/stages/harbor_ground.png | 2560×512 PNG, tiles horizontally | placeholder | foreground fight plane: cracked concrete pier surface strip, seamless horizontal tiling, debris and standing water detail |
| stage_harbor_ground_flooded | assets/stages/harbor_ground_flooded.png | 2560×512 PNG, tiles horizontally | placeholder | the same cracked pier strip after Khulandra's tide surge: ankle-deep black water, floating debris, rippled reflections — post-event state so the stage visibly changes |
| stage_khulandra_breach | assets/stages/khulandra_breach.png | 2048×2048 PNG, alpha | placeholder | Khulandra's head and foreclaw breaching upward with cascading black water, cut by the frame, for the mid-fight stage event |

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
| ui_vs_screen | assets/ui/vs_screen.png | 1920×1080 PNG | placeholder | VS screen background: split ink wash, Blood Seal left, Goryō Flame right, empty portrait zones both sides |

## VFX

| ID | Canonical path | Spec | Status | Subject line |
|---|---|---|---|---|
| vfx_hit_spark | assets/vfx/hit_spark.png | 512×512 PNG, alpha | placeholder | radial ink-splash impact burst, Bone Paper core with Signal Amber fringe |
| vfx_ink_blood | assets/vfx/ink_blood.png | 512×512 PNG, alpha | placeholder | stylized Blood Seal ink-splatter burst, heavy droplets, screentone falloff |
| vfx_meter_flare | assets/vfx/meter_flare.png | 512×512 PNG, alpha | placeholder | spectral teal spirit-flame flare, wisping upward like a goryō |
| vfx_kest_foxfire | assets/vfx/kest_foxfire.png | 512×512 PNG, alpha | placeholder | Kest special-move effect: streaking spectral teal fox-fire trail with ember wisps |
| vfx_tengi_bladewave | assets/vfx/tengi_bladewave.png | 1024×512 PNG, alpha | placeholder | Tengi special-move effect: dark crescent blade wave edged in Blood Seal, trailing crow feathers |
| vfx_kaiju_shockwave | assets/vfx/kaiju_shockwave.png | 1024×256 PNG, alpha | placeholder | horizontal ground shockwave of dust, seawater, and debris for the Khulandra stage event |

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
