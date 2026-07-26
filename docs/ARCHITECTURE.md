# ARCHITECTURE — Kaiju Ruin (v0.1)

*The Unity project is scaffolded and code-complete for the vertical slice (32 C# scripts in `game/`). This doc records the stack (fixed by VISION.md §11), the layout, the code↔asset contract, the animation pipeline (D-011), and the multiplayer/networking seam (D-017).*

## Stack (locked by Vision §11)

- **Unity 6 LTS + URP**, Android-first, landscape, ARM64, minimum API level ~26, IL2CPP release builds.
- Performance bars: 60 fps on 2021 mid-range hardware (Snapdragon 695-class), input-to-impact under 80 ms, install under 300 MB at launch.
- Characters: rigged 3D on a side-locked camera and a 2D gameplay plane. Story and finishers: 2D illustrated comic panels.
- Offline-first single-player and async modes. Real-time PvP stays a separate, **gated** project — the build never *depends* on a hosted service. A pluggable networking **seam** now exists (D-017, see §Multiplayer) with an offline default; live worldwide PvP lights up only when a backend is dropped into that seam, which remains unauthorised/out of the local build.
- Every milestone deliverable is a complete Unity project that builds locally to an APK; nothing depends on a hosted service to play. No deploy/publish of any build or asset (CLAUDE.md ground rule 3).

## Planned repo layout

- `assets/` — engine-agnostic master art/audio. Source of truth, tracked in ASSET_MANIFEST.md.
- `scripts/` — repo maintenance checks (`check_docs_fresh.sh`, `check_manifest.sh`); the assets→game sync script joins them at scaffold time.
- `game/` — the Unity project (scaffolded 2026-07-18 from DESIGN_BRIEF.md).
  - `game/Assets/Scripts/` — 35 C# files, everything built code-first from a near-empty Boot scene (no hand-authored prefabs). Includes the multiplayer front-end + networking seam (D-017), the onboarding front end (D-022), the distance/spacing layer (D-023), and the moveset + guard-stance layer (D-024).
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

## Multiplayer / networking (D-017)

Built as a **seam, not a service** — the game still builds to a local APK and plays offline. The gated-PvP rule above holds: nothing in the shipping build depends on a hosted service.

- **Two interfaces are the whole contract.** `IMatchmaker` (create/join room, quick match — coroutine-shaped for animated lobby status) and `INetTransport` (carries a per-tick `FighterInputCmd` between peers). Everything else — UI, roster, fight — talks only to these.
- **Shipping backend = offline stand-ins.** `LocalMatchmaker` + `LoopbackTransport`: real room codes, real search/connect flow, real session hand-off, but a simulated peer, so the opponent resolves to the AI (`MatchConfig.RemoteOpponent = false`). `NetService` (Bootstrap singleton, `DontDestroyOnLoad`) selects the backend (one field), owns the transport lifetime, and mirrors the negotiated session into `MatchConfig`.
- **Live backend = drop-in.** Implement `RelayMatchmaker` / `RelayTransport` (stubbed with the integration steps: Unity Authentication + Lobby + Matchmaker + Relay + Unity Transport; **input-lockstep** fits the deterministic X-axis sim, with host-authoritative state sync as the fallback if `Time.time`/`Random` determinism proves fragile) and flip `NetService.Backend` to `Relay`. Zero UI or fight changes.
- **Roster is data.** `CharacterRoster`/`CharacterDef` hold every per-champion value; `RoundManager` spawns both fighters from `MatchConfig` via the roster, and `CombatSystem.Special(set, slot)` selects cards by set — so `PlayerController`/`EnemyAI` are champion-agnostic. **Add a champion = add one `CharacterDef`** (+ its art rows).
- **Control assignment (in `RoundManager`).** Local player → `PlayerController` + `TouchInput`. Opponent → `EnemyAI` (solo + loopback online) *or* `RemoteController` (a real transport is connected) which replays remote `FighterInputCmd` through the same `PlayerController` verbs with `Local = false` (never touches the local HUD). `NetInputRelay` is the local send counterpart. Fighters are parented under the fight root so cleanup survives mirror matches.
- **Distance model** (**D-023**). `root.transform.position.x` remains the sole sim truth, and one formula owns every range read: `CombatSystem.EffectiveReach(attacker, target, attack)` = the move's baseline (Kest) reach adjusted by the attacker's measured arm reach and the target's measured hurt depth, with per-champion body metrics carried on `CharacterDef` → `Fighter`. Hit resolution, AI spacing, step-in gaps, the gap-closing special's landing and the on-screen ground cues all resolve through it, so a silhouette and its ranges cannot drift apart. **`CombatSystem.Separate` is the single push-out authority**, run once per frame from `RoundManager.LateUpdate` after every controller's `Update` — that ordering is what guarantees the gap the player sees is the gap the hit check read. `GroundCues` and the camera are ticked from the same LateUpdate chain, after separation, for the same reason. `ProcAnim`'s airborne lift and `Contact()` snap stay render-only (Visual child), per the deterministic-sim contract.
- **Guard is a stance, and one helper owns the rule** (**D-024**). `Fighter.GuardKind { None, Standing, Crouch }` replaced the old block bool (`Blocking` survives as the "guarding at all" read). `CombatSystem.GuardStops(target, attack)` is the single place the mirror lives — standing stops everything but lows, crouch stops everything but overheads, a grab ignores both, airborne guards nothing — so hit resolution, the AI's stance choice and the HUD glyph cannot disagree. Attacking drops the guard for its own recovery (`Fighter.OpenUpUntil` / `CanGuard`), which is what stops a stance being held through the strikes thrown out of it. `CombatSystem.LongestNormal` is the one place "the longest normal" is named (AI threat radius, outer ground band).
- **One clip per move, fitted to the move** (**D-024**). The generated action clips run 0.6–4.8 s while a normal recovers in 0.20–0.70 s, so `FighterAnimator.PlayFor(state, seconds)` scales playback to the move's own window (clamped 1–6×); at speed 1 a fighter was still winding up after its hit had resolved. `FighterAnimator.Current` exists so held poses (guard stances) can be re-asserted without restarting the clip every frame. Root translation inside a clip moves the model within its silhouette only — never the sim X, which no animation may touch.
- **Front-end flow** (restructured by **D-022**; `GameManager` owns it). First run: buttonless **Title** → **StoryIntro** (7 lore beats, skippable, once per install via a `PlayerPrefs` flag) → **HowToPlay** → **FightSelect**. Afterwards: Title → FightSelect. Then Solo: FightSelect → CharacterSelect → Fight. Online: FightSelect → MultiplayerMenu → CharacterSelect → Lobby → Fight. Ending → Title. The intro is a **front-end** step, no longer part of `FightFlow` (so it plays once, on both paths, instead of before every solo fight); `HowToPlay` and `REPLAY STORY` keep both onboarding steps reachable from FightSelect. `MatchConfig` is the shared blackboard (defaults reproduce the original single-player fight).

## Testing intent (expand when code exists)

- Fight-sim core (inputs → deterministic state) built engine-independent and unit-testable; the render layer stays thin.
- Performance validated on real hardware per Vision Goal 2 — profiled on-device, not in-editor.
- A manual playtest checklist will live here once there is a build to test.

## Repo and tooling notes

- Git initialized 2026-07-17, **zero commits so far** — session-end commits await owner authorization (STATUS open questions). Until the first commit lands there is no version history: treat overwriting any file as destructive.
- Before `assets/` fills with binaries: **Git LFS** — `git lfs install && git lfs track "*.png" "*.glb" "*.ogg" "*.wav"` (tracked as a STATUS Next-up gate).
