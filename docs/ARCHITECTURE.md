# ARCHITECTURE — Kaiju Ruin (v0.1)

*No code exists yet. This doc records the intended stack (fixed by VISION.md §11), the planned layout, and the one contract that already binds: how code will reference assets. It expands when the Unity project is scaffolded; C# coding standards go into CLAUDE.md at that point.*

## Stack (locked by Vision §11)

- **Unity 6 LTS + URP**, Android-first, landscape, ARM64, minimum API level ~26, IL2CPP release builds.
- Performance bars: 60 fps on 2021 mid-range hardware (Snapdragon 695-class), input-to-impact under 80 ms, install under 300 MB at launch.
- Characters: rigged 3D on a side-locked camera and a 2D gameplay plane. Story and finishers: 2D illustrated comic panels.
- Offline-first single-player and async modes. Real-time PvP is explicitly a separate, gated future project — never assume it in architecture.
- Every milestone deliverable is a complete Unity project that builds locally to an APK; nothing depends on a hosted service to play. No deploy/publish of any build or asset (CLAUDE.md ground rule 3).

## Planned repo layout

- `assets/` — engine-agnostic master art/audio. Source of truth, tracked in ASSET_MANIFEST.md.
- `scripts/` — repo maintenance checks (`check_docs_fresh.sh`, `check_manifest.sh`); the assets→game sync script joins them at scaffold time.
- `game/` — the Unity project (scaffolded 2026-07-18 from DESIGN_BRIEF.md).
  - `game/Assets/Scripts/` — 18 C# files, everything built code-first from a near-empty Boot scene (no hand-authored prefabs).
  - `game/Assets/Resources/{Art,Audio,Fonts}` — one-way synced copies of `assets/` masters via `scripts/sync_assets.sh` (concept art excluded; textures load as Texture2D, sprites created at runtime).
  - `game/Assets/StreamingAssets/Models/` — rigged GLBs + animation clip GLBs, loaded at runtime by glTFast.
  - Slice deviations from production intent (Built-in RP instead of URP; legacy-Animation playback of the D-011 clips because AnimatorController authoring is editor-only) are documented in `game/README.md`.

## The code↔asset contract (binding from day one)

1. Code, prefabs, and scenes reference assets **only by the canonical path/ID** listed in ASSET_MANIFEST.md — never ad-hoc filenames.
2. Canonical paths are stable forever; swaps follow the per-type replacement contract in ART_DIRECTION.md §4 (single source — images, GLB, audio, stage-layer registration). Zero code changes per swap.
3. A new asset gets its manifest row *before* the file lands; its generation record is written immediately after generation.
4. Sync is one-way: `assets/` (masters) → `game/Assets/Art/` (imports). Never edit the copy inside `game/` directly.

## Animation pipeline (decided — D-011, owner, 2026-07-18)

Higgsfield image-to-3D rigged models + the rig action library (idle, walk, punch, block, special attack, hit reaction, death), imported via **glTFast** (`com.unity.cloud.gltfast`), driven by an **Animator Controller with blend trees** (idle↔walk locomotion) whose triggers are wired to PlayerController/EnemyAI events. Clips are generated on Kest's rig and retargeted to Tengi via Unity Humanoid avatars (shared Meshy skeleton). Non-standard rigs (caudatas, kaiju) will get custom action selections instead of humanoid retargeting. Runtime loading and Animator wiring live in `game/Assets/Scripts/GltfCharacterLoader.cs`. Full spec: DESIGN_BRIEF.md §Animation.

## Testing intent (expand when code exists)

- Fight-sim core (inputs → deterministic state) built engine-independent and unit-testable; the render layer stays thin.
- Performance validated on real hardware per Vision Goal 2 — profiled on-device, not in-editor.
- A manual playtest checklist will live here once there is a build to test.

## Repo and tooling notes

- Git initialized 2026-07-17, **zero commits so far** — session-end commits await owner authorization (STATUS open questions). Until the first commit lands there is no version history: treat overwriting any file as destructive.
- Before `assets/` fills with binaries: **Git LFS** — `git lfs install && git lfs track "*.png" "*.glb" "*.ogg" "*.wav"` (tracked as a STATUS Next-up gate).
