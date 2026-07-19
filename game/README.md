# Realm of Goryo: Shadow of Giants - Vertical Slice (Unity project)

Local Unity project for Android. Nothing here is deployed or published; you build the APK on your own machine. Design source of truth: `../docs/DESIGN_BRIEF.md`.

## Requirements

- Unity Hub with **Unity 6 LTS (6000.0.x)** and the **Android Build Support** module (including SDK, NDK, and OpenJDK checkboxes).
- An Android device with USB debugging enabled, or an emulator.

## Open the project

1. Unity Hub -> Add -> select this `game/` folder.
2. Open it with Unity 6000.0.x. First open takes a few minutes while the Package Manager resolves `com.unity.cloud.gltfast` (GLB import) and compiles scripts.
3. Open the scene `Assets/Scenes/Boot.unity`.
4. Press Play in the editor to test: mouse works as touch (left half of the Game view = movement drag; right half = tap / swipe / hold).

If assets look missing, run `../scripts/sync_assets.sh` from the repo root first - it copies the master art, audio, fonts, and GLB models from `../assets/` into this project. Never edit the synced copies here; replace the masters and re-sync.

## Android build steps

1. File -> Build Settings (Build Profiles on newer editors) -> Android -> Switch Platform.
2. Edit -> Project Settings -> Player, set:
   - Company/Product: Nyalia / Shadow of Giants
   - Package name: `com.nyalia.kaijuruin`
   - Default orientation: Landscape Left (or Auto Rotation with portrait disabled)
   - Minimum API Level: 26
   - Scripting backend: Mono for quick test builds; IL2CPP + ARM64 for release-grade builds
   - App icon: `Assets/Resources/Art/ui/emblem.png`
3. Connect the device (or pick a running emulator) and Build And Run. The APK lands where you choose; the game boots into the title screen.

## What is in here

- `Assets/Scripts/` - all game code, built scene-free: `Bootstrap` (Boot scene entry), `GameManager` (menu -> intro -> fight flow), `RoundManager` (best-of-3, banners, Khulandra event, KO), `PlayerController` / `EnemyAI` / `Fighter` / `CombatSystem` (deterministic fight sim), `StageManager` (parallax layers + flooded swap), `TouchInput` (tap/swipe/hold gestures), `TouchUI` / `MainMenu` / `StoryIntro` / `EndingPanel` (all UI in code), `GltfCharacterLoader` + `FighterAnimator` (glTFast runtime GLB + clip playback), `AudioManager`, `AssetLib`, `UiKit`.
- `Assets/Resources/` - synced art (as plain textures; sprites are created at runtime), audio, fonts.
- `Assets/StreamingAssets/Models/` - rigged character GLBs plus per-move animation clip GLBs, loaded at runtime by glTFast.

## Known slice deviations from production intent (documented, deliberate)

- **Render pipeline:** the slice runs the Built-in pipeline so the project opens with zero configuration. Production intent is URP (VISION.md section 11); switching later means installing the URP package and creating a pipeline asset - no code changes expected beyond materials.
- **Animation playback:** D-011 intends Animator Controller + blend trees. AnimatorController authoring is an editor-only API, so the runtime loader plays the same clips through a legacy Animation component with crossfades (`FighterAnimator`). When you import the GLBs in-editor for production, wire the same state names (idle, walk, punch, block, hit, death) into a real AnimatorController and swap `FighterAnimator` internals.
- **Missing generation:** any audio or clip rows still `planned` in `../docs/ASSET_MANIFEST.md` simply stay silent / fall back (special reuses punch + VFX). Regenerate per `../docs/ART_DIRECTION.md` section 4 and re-run the sync.

## Performance bars (Vision Goal 2)

60 fps on Snapdragon 695-class hardware, input-to-impact under 80 ms, install under 300 MB. Profile on-device, not in-editor.
