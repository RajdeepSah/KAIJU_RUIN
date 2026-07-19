# ART DIRECTION — Kaiju Ruin (v0.1, PROPOSED)

*Status: proposal awaiting owner approval (D-004), which happens by judging the style test (D-005). Everything here is placeholder direction: cohesive enough to carry a vertical slice, designed to be replaced by the official art team asset-for-asset.*

Source constraints (VISION.md): comic-book-styled, mature (M-rated within Pillar 4's hard lines), MK4-era grimness as tonal reference, heavy-ink comic panels for story and finishers, rigged-3D-on-a-2D-plane for in-fight characters, Japanese folklore fused with a near-future 2061 technothriller, readable on a 6-inch screen at arm's length (Pillars 3 and 7).

## 1. Style bible

**One line:** *ink-heavy seinen comic noir — a mature graphic novel about gods and the people living under them.*

- **Line:** bold, brutal ink work; thick exterior contours, expressive hatching inside. Nothing soft or airbrushed.
- **Light:** dramatic chiaroscuro; one strong key light per shot; shadows collapse to near-black ink mass.
- **Texture:** screentone dots and dry-brush grit. Never photographic texture.
- **Color:** a desaturated world with saturated accents (§2). Silhouette first — every character and threat must read at arm's length on a phone.
- **Scale language (Pillar 2, hard line):** kaiju are framed as *environment* — cut by the frame edge, wreathed in sea, storm, or smoke, lit like weather. A kaiju is never fully contained in frame beside a human. A full-body kaiju shot is permitted only when the framing itself amplifies scale (extreme wide, humans as specks, weather-like lighting); if a shot reads as shrinking a kaiju, reshoot it.
- **Folklore fusion:** yōkai iconography (goryō spirit-flame, kitsune, tengu silhouettes), torii gates, flood-myth water imagery — woven through 2061 tech: kanji-lit ruins, drone swarms, coalition exo-armor.

**Not this:** bright anime cel-shading; superhero primaries; photorealism; pixel art; cute or chibi anything — above all not the kaiju (Pillar 2 hard line).

## 2. Palette

| Swatch | Hex | Use |
|---|---|---|
| Sumi Ink | `#15171C` | line work, shadow mass, night skies, backgrounds |
| Bone Paper | `#E9E2D0` | highlights, comic-panel paper, primary UI text |
| Ash Steel | `#5A636E` | midtones, city, sea, coalition tech |
| Blood Seal | `#A6212C` | damage, Horrific Endings, danger UI |
| Goryō Flame | `#3FB08F` | spirit/kaiju glow, special meter, supernatural accents |
| Signal Amber | `#C88A3A` | fire, city light, secondary UI accent |

Rule of thumb: a frame is ~70% Sumi/Ash, ~20% Bone, ~10% accents. Blood Seal and Goryō Flame never both dominate the same frame — except at match point, where that collision is the drama.

## 3. UI guidelines

- Landscape, one-thumb reach (Pillar 3): interactive elements live in the bottom corners; top of screen is information only. Minimum touch target 48dp.
- Panels and cards use comic-panel language: sharp corners, 2–3px ink-rule borders, Bone Paper on Sumi Ink.
- HUD: health bar in Bone Paper revealing Blood Seal as damage accrues; special meter fills in Goryō Flame.
- Type: condensed grotesque sans for HUD and numbers; rough brush display face for titles and Horrific Ending captions. Fonts are tracked assets with license fields — see ASSET_MANIFEST.md §Fonts.
- Every tap answers visually within 80 ms (Pillar 3) — ink-splash or flash, no easing delays on input feedback.
- Icons: single-weight ink glyphs, no gradients, readable at 24dp.

## 4. Asset pipeline (generate → place → replace)

The point of this pipeline: **any asset can be swapped by overwriting one file and flipping one manifest cell.** That is how the official art team will replace every placeholder later. This section is the single home of the pipeline and the replacement contract — other docs point here instead of restating it.

1. **Spec** — find the asset's row in ASSET_MANIFEST.md (ID, canonical path, spec, subject line). No row? Add one first.
2. **Prompt** — compose the final prompt exactly per the template in §5.
3. **Generate** — Higgsfield MCP: `generate_image` for art, `generate_audio` for music/SFX, `generate_3d` for GLB meshes (feed it the character's A-pose sheet image). Unsure which model fits: `models_explore` with action `recommend`. Generate 2–4 candidates per asset and keep the most on-style (a candidate batch counts as ONE asset for gating purposes, D-005). Request the row's aspect ratio; any resolution at the right aspect that meets or exceeds spec is acceptable — exact pixel match is not required for placeholders.
4. **Post-process** — `remove_background` for anything needing a cutout (characters, props, UI elements, VFX); `outpaint_image` to extend extreme aspects (the 5:1 ground strips, uncropping); `upscale_image` to reach spec. For tiling strips, verify the seam actually tiles before recording. For audio, convert to the row's container (see contract below) before saving — `ffmpeg` if the generator returns a different format.
5. **Place** — save to the canonical path under `assets/`. Paths are stable forever; no version suffixes (git is the version history once commits are enabled — see STATUS open questions).
6. **Record — immediately, before generating the next asset:** flip the manifest row's status to `placeholder` and append the generation record (model, job/generation ID, seed if reported, date, full final prompt verbatim, post-processing applied) to ASSET_MANIFEST.md §Generation records. Never batch records to session end — a session that dies mid-batch loses the prompts forever.

### Replacement contract (single source of truth)

A replacement — placeholder regenerated, or official art team delivering final — means: overwrite the file at the same canonical path, flip the manifest status, note the date. Nothing in code changes (ARCHITECTURE.md's code↔asset contract). Per asset type:

- **Images:** same aspect ratio at the same or higher resolution; same anchor/pivot intent (e.g. characters grounded at bottom-center); alpha channel wherever the spec says so.
- **GLB meshes:** glTF conventions (meters, +Y up, +Z forward), origin at bottom-center between the feet; triangle budget set at Unity scaffold time. Once rigs exist, a replacement mesh must keep the placeholder's skeleton/bone naming — a different rig is *not* a drop-in swap and must be flagged, not silently placed.
- **Audio:** same container as the canonical path (music `.ogg`, SFX `.wav` — D-007); 44.1 kHz; music must loop seamlessly (trimmed exactly to loop); duration within ±20% of spec; loudness around −16 LUFS, mixed for phone speakers.
- **Stage layers:** the harbor layers must register with each other — shared horizon line, dusk key light from screen-left, consistent haze density; ground strips must tile seamlessly.

**Derived assets invalidate with their source:** when an asset's file or status changes, every row that lists it as a source (e.g. `char_kest_model` is derived from `char_kest_apose`) flips to `needs-rework` in the same edit. The manifest marks derivations explicitly.

**Unity hand-off (once `game/` exists):** masters live in `assets/`; a one-way sync copies them into `game/Assets/Art/`, mirroring the folder structure. Replace the master, re-sync, Unity picks it up. The sync script is written at scaffold time; until then the copy is manual. Never edit the copy inside `game/`.

## 5. Higgsfield prompt library

### Prompt template (authoritative — all other docs point here)

> **final prompt = STYLE CORE, category recipe, subject line (from the manifest row), avoid-list — joined with commas, in that order.**

Record the composed prompt verbatim in the asset's generation record.

### STYLE CORE

> mature dark comic book illustration, heavy black ink lines, dramatic chiaroscuro lighting, gritty screentone shading, desaturated palette of ash gray and bone white with blood red and spectral teal accents, Japanese folklore meets near-future 2061 technothriller, cinematic composition, high contrast

### Universal avoid-list

> no text, no watermark, no signature, no photorealism, no bright anime cel shading, no chibi proportions

### Category recipes

| Category | Recipe fragment |
|---|---|
| Concept / key art | `full illustrated scene, epic scale, human figures dwarfed by a colossal kaiju that breaks the frame` |
| Character sheet (feeds `generate_3d`) | `full body character concept, neutral A-pose, front view, isolated on a plain light gray background, entire figure visible head to toe, no cropping` |
| Portrait (HUD / select screen) | `head and shoulders portrait, three-quarter view, intense expression, isolated on a plain dark background` |
| Stage layer | `wide 2D fighting game stage background, horizontal composition, clear flat ground plane, layered depth with atmospheric haze, no characters` |
| Comic panel (story / Horrific Ending) | `single full-page comic splash panel, heavy ink, extreme dramatic angle, screentone texture, empty space at the bottom for a caption, no speech bubbles, no lettering` |
| UI element | `flat game UI element, single-weight ink line style, on a plain solid background` → then `remove_background` |
| VFX sprite | `stylized 2D fight effect, ink splash energy, centered on a plain black background` |
| Music | cinematic hybrid of taiko percussion, low brass, distorted shamisen, and dark electronics; dread and forward momentum; loopable |
| SFX | describe the physical event, then: `punchy, short tail, mixed for mobile speakers` |

Per-asset **subject lines** live in ASSET_MANIFEST.md, one per row — prompt provenance and asset state stay in one place.

## 6. Canon cautions for artists (human or AI)

- Character and kaiju appearances are **not canon-confirmed** (Vision §3.5). Every design here is an inference to be validated against the lore bible and rights holder — which is exactly why every asset must stay swappable. This includes the yōkai readings (Kest as kitsune, Tengi's tengu-mask, Vision §3.3) and Goryo's guardian role (Vision §3.1: *strongly implied*, not stated).
- Caudatas as salamander-folk is an *inference*; design accordingly but keep it revisable.
- Pillar 4 hard lines apply to imagery: no sexual violence, no torture-as-reward, no real-world extremist iconography, nothing involving minors. Gore serves fear or story, or it goes.
- Kaiju scale (Pillar 2): the rule in §1 is absolute — never fully contained in frame beside a human; any deliberate exception needs owner sign-off because it touches a pillar.
- Placeholder art is internal-only: never published, deployed, or shared outside this repo before rights-holder approval.
