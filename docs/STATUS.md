# STATUS — Kaiju Ruin

*Last updated: 2026-07-19 (session 6). Single source of truth for project state. The top sections are rewritten every session (Done covers work since the last milestone flip — prune it when a milestone completes); the session log is append-only, newest first.*

## Snapshot

**Phase: multiplayer mode added in code — compiles clean (32 scripts), needs on-device playtest.** Per the owner directive to make the game multiplayer, session 6 added a **"Play Multiplayer"** mode — Quick Match (worldwide), Create Room, and Join-by-code — plus a data-driven **character select** where you pick your fighter *and* the fighter you want to fight. It is built as a **pluggable networking seam, not a hosted service**, so the APK still builds and plays offline (Vision §11 / ground rule 3, and ARCHITECTURE's gated-PvP rule holds). Two interfaces (`IMatchmaker`, `INetTransport`) are the whole contract; the shipping backend is offline stand-ins (`LocalMatchmaker` + `LoopbackTransport`: real room codes, real Searching…/Connecting… flow, real session hand-off — but the peer is simulated, so the opponent resolves to the **AI**). A real backend (Unity Relay/Lobby, stubbed with the exact steps in `RelayTransport.cs`) drops into the same seam and goes live with **zero UI/fight changes**. The roster is now **data** (`CharacterRoster`/`CharacterDef`): adding a champion = one entry — Kest & Tengi are just the first two rows, and the fight, HUD, VS screen and endings are all champion-agnostic (D-017). **11 new scripts + 11 touched; all 32 compile clean** under Unity 6000.0.78f1 Roslyn vs the real UnityEngine/UGUI/glTFast references (baseline of 21 re-confirmed first). **No artwork generated** (per the owner's instruction to prepare only): 2 dedicated UI assets (`ui_menu_bg`, `ui_roster_card`) are `planned` rows with **pre-composed prompts** + a per-champion art template in ASSET_MANIFEST, wired via `AssetLib.SpriteOr` so they drop in on the next sync when the owner says **"generate necessary artwork."** Assets: **69 manifest rows** (67 + 2 planned). **Git: `origin/main` at github.com/RajdeepSah/KAIJU_RUIN.** Higgsfield credits: **80.75** (unchanged this session).

## Done

- ✅ **Session 6 multiplayer mode (D-017)** — 11 new scripts + 11 touched under `game/Assets/Scripts/`. **Front-end:** MainMenu now offers **SOLO FIGHT** + **PLAY MULTIPLAYER**; new `MultiplayerMenu` (Quick Match / Create Room / Join-by-code with an on-screen `InputField`), `CharacterSelectMenu` (data-driven roster grid — pick your fighter *and* your opponent), `LobbyMenu` (runs matchmaking, shows the room code to share + live status, START when ready); `GameManager` orchestrates Solo (Menu→Select→Fight) and Online (Menu→MP→Select→Lobby→Fight) flows. **Networking seam:** `IMatchmaker` + `INetTransport` (NetTypes) with shipping offline impls (`LocalMatchmaker`+`LoopbackTransport`) and a documented live-backend stub (`RelayMatchmaker`/`RelayTransport`); `NetService` (Bootstrap singleton) selects the backend + owns the transport; `RemoteController`/`NetInputRelay` are the live-PvP drive/send seam. **Data-driven roster:** `CharacterRoster`/`CharacterDef` + `MatchConfig`; `RoundManager` spawns both fighters from the roster and assigns control (AI vs remote); `CombatSystem.Special(set,slot)` makes cards follow the chosen champion; `PlayerController`/`EnemyAI`/`Fighter`/`TouchUI`/`EndingPanel` are now champion-agnostic (HUD names/icons, VS portraits, ending by winner).
- ✅ **Compile-validated all 32 scripts** with Unity 6000.0.78f1 Roslyn against the real UnityEngine/UGUI/glTFast reference assemblies — **0 errors** (baseline of 21 confirmed first; full set re-checked after the art-seam wiring).
- ✅ **Artwork prepared, NOT generated** (per instruction): 2 `planned` rows (`ui_menu_bg`, `ui_roster_card`) with **pre-composed prompts** + a per-champion art template in ASSET_MANIFEST *Planned generation queue*; `AssetLib.SpriteOr(preferred, fallback)` wired so the game looks right on existing art today and each asset drops in on the next `sync_assets.sh` with zero code change. `check_manifest.sh` clean (planned rows need no file).
- ✅ Prior slice foundation stands: session-5 combat expansion (D-015/D-016: hit-stop/shake, ProcAnim, back-dash/air-juggle/parry/special-cancel/chain-enders, per-character feel), DESIGN_BRIEF v1.0 + moves addendum, session-4 UI/controls rework (D-014), full placeholder asset set, 6 anim clips, all audio, README build guide.

## In progress

- (nothing mid-flight — session 6 multiplayer is code-complete, compiles clean; awaiting on-device verification and the owner's "generate necessary artwork" command)

## Next up (in order)

1. **Owner: recompile in the open Unity editor and on-device playtest — now including multiplayer.** Focus the running editor (recompiles the changed scripts), Play-in-editor. **New flows to smoke-test:** Main menu → **PLAY MULTIPLAYER** → Quick Match / Create Room (shows a room code to share) / Join Room (type a code) → **character select** (pick your fighter *and* opponent) → lobby (Searching…/Connecting…, START) → fight. **Solo path:** SOLO FIGHT → character select → fight — try a **Tengi mirror** or Tengi-as-player to confirm the roster is truly data-driven (HUD name/cards, VS portraits, ending all follow the pick). Note: online currently resolves to a **local AI match** (loopback backend) — verify the UI/codes/status read right; live remote play is item 3. Combat smoke-test (from session 5) still applies. Code: `game/Assets/Scripts/` ({MainMenu,MultiplayerMenu,CharacterSelectMenu,LobbyMenu,GameManager,RoundManager,NetService,CharacterRoster,MatchConfig}).
2. **"generate necessary artwork" (owner command) — then generate.** Multiplayer `planned` rows first: `ui_menu_bg`, `ui_roster_card` — prompts are ready in **ASSET_MANIFEST → Planned generation queue** (compose per ART_DIRECTION §5, place, `sync_assets.sh`, record immediately). Optionally the deferred **combat** art from session 5 (per-move generated animation library, the `special attack` clip, `sfx_whiff`) — the bigger animation library needs a Higgsfield credit top-up (D-015). New champions: follow the per-champion art template in the same section.
3. **Live worldwide PvP (gated — owner authorisation required).** Implement `RelayMatchmaker`/`RelayTransport` (Unity Authentication + Lobby + Matchmaker + Relay + Unity Transport; **input-lockstep** suits the deterministic X-axis sim) and flip `NetService.Backend` to `Relay` — zero UI/fight changes. Kept out of the local build per ground rule 3 (a hosted relay/matchmaking service); the seam, stub, and step-by-step are already in `RelayTransport.cs` (D-017).
4. **First combat playtest fix round** — work the *First-pass tuning log* below against feel (cheap knobs).
5. **Resolve the two remaining open questions** (below) — meter-taken charge rate; key_art vs harbor_sky menu background.
6. **Polish backlog (tracked, not blocking):** hand-trim music loop points; graver announcer voice; production URP switch + editor AnimatorController (game/README deviations); wire real AnimatorController state names when the generated clip library lands.
7. **Lore bible v1** (Vision Goal 4, milestone M4) — remaining pre-production doc; placeholder designs stay inference-flagged until it exists.

## Open questions (owner input needed)

- **Meter charge rate for damage taken is halved vs the locked brief.** DESIGN_BRIEF MOVES says "a segment charges per 150 damage dealt or 80 taken", but `CombatSystem.cs` GainMeter-on-taken has a deliberate-looking `* 0.5f` (net ~1 segment per 160 taken). Still **unchanged** (session 5 D-015 tuned cadence/moves, not the meter-taken rate). Owner call: keep the halved rate (amend the brief + log a D-###) or restore to spec? Note: D-015 separately made *specials* grant 0.25× meter to the attacker (so normal-xx-special is meter-negative) — that is a deliberate, logged change, not this deviation.
- **Main-menu background: `key_art.png` (brief) vs `harbor_sky` (current).** DESIGN_BRIEF UI names `key_art.png`, but that asset is classified `concept_key-art` and `sync_assets.sh` excludes concept art from shipping, so `MainMenu.cs` substitutes the harbor sky layer. Keep the substitution, or promote a menu-specific key-art asset into the shippable set?

## First-pass tuning log (confirm on device)

Feel/number items to validate on device. Combat depth/cadence is now open (D-015 superseded the session-4 freeze), so these are live tuning knobs, not blocked.

1. **Meter-taken rate halved vs spec** — see Open questions (unchanged).
2. **Faster cadence (D-015):** normals recover in 0.20–0.62 s (was 0.25–0.8), walk 3.0. Confirm the fight reads fast-but-controllable; per-character `Fighter.AttackSpeed` (Kest 1.12 / Tengi 0.92) and `WalkSpeed` are the knobs.
3. **Parry window 160 ms** (`CombatSystem.ParryWindow`); reward = attacker stun 0.30 s + 40 meter. Confirm timing is satisfying, not fishing-dominant; window + reward are cheap knobs. A distinct *failed-parry* cost (blockstun) was left as future work (no blockstun system in the slice).
4. **Air juggle:** `MaxJuggle`=4 + damage decay + wake-up i-frames bound it. Confirm juggles feel rewarding but escapable; levers are MaxJuggle, decay, and the knockdown-stun/i-frame length.
5. **Back-dash** (`PlayerController.BackDash`): 1.2 m, 0.24 s i-frames, 0.30 s recovery. Confirm it's evasive without being spammable; i-frame/recovery lengths are the knobs.
6. **Step-in / hit-stop / shake magnitudes** (`CombatSystem` StepIn per move; `CombatFx` Stop*/Shake/Punch). Tune to taste on device — hit-stop especially reads very differently at 60 fps on a phone than in-editor.
7. **Special still animates via the shared clips + ProcAnim flourish** (dedicated special GLB deferred to the credit refill) — VFX overlay carries it. Regenerate when credits allow.

## Milestones (tracking Vision Horizon 1)

| # | Milestone | Status |
|---|---|---|
| M0 | Repo, docs workflow (mechanically enforced), art direction proposal | ✅ done |
| M1 | Style approved (D-004 accepted); style test + concept art locked | ✅ done 2026-07-18 |
| M2 | Full placeholder asset set | ✅ done 2026-07-18 (62/63 rows; special-attack clip deferred by design) |
| M3 | Design brief locked + animation pipeline decided | ✅ done 2026-07-18 (DESIGN_BRIEF v1.0, D-011/D-012/D-013) |
| M4 | Rights/adaptation terms in writing + lore bible v1 (Vision Goal 4) | ◐ rights confirmed (D-009); lore bible v1 pending |
| M5 | Unity project scaffolded; Kest vs Tengi playable gray-box | ✅ done 2026-07-18 — owner playtested; loop/architecture/env-art approved |
| M6 | Vertical slice per Vision Goal 1 (full touch combat, living stage, Horrific Endings, motion-comic) | ◐ UI/controls reworked (D-014) + combat expansion (D-015/D-016) + multiplayer mode & data-driven roster (D-017), all compile-clean; needs on-device playtest + Goal 2 bar verification. Note: multiplayer extends beyond the Goal 1 single-player slice — live PvP is gated (needs a backend). |

## Session log (append-only, newest first)

### 2026-07-19 — Session 6: multiplayer mode + data-driven roster (D-017)

- Reconciled the doc-freshness warning (only `.gitignore` was newer than the docs — build/config churn, no stale source/docs). Read STATUS + the code (all 32 gameplay scripts after this session) and the doc set before building.
- Owner directive: convert the game to multiplayer — a **Play Multiplayer** mode to choose your character, choose the opponent, invite a friend by room code, and find a random online match worldwide; UI + networking built to **scale** to more characters/matchmaking later. Also: prepare everything for Higgsfield artwork but **generate none** (a future "generate necessary artwork" command will trigger it).
- Chose the honest, buildable interpretation given Vision §11 / ground rule 3 (local APK, no hosted service; ARCHITECTURE's gated-PvP rule): build a **pluggable networking seam** (`IMatchmaker` + `INetTransport`) with a **shipping offline default** (`LocalMatchmaker`+`LoopbackTransport` — real room codes/search/session flow, opponent = AI) and a documented **live-backend drop-in** (`RelayMatchmaker`/`RelayTransport` stubs with the exact Unity Auth+Lobby+Matchmaker+Relay+UTP steps + input-lockstep note). Logged **D-017**.
- Built the front-end: MainMenu (Solo / Multiplayer), MultiplayerMenu (Quick Match / Create Room / Join-by-code via a code `InputField`), CharacterSelectMenu (data-driven roster grid — pick self + opponent), LobbyMenu (matchmaking status + shareable room code + START); GameManager orchestrates both flows; Bootstrap gains `NetService`.
- Made the fight **data-driven**: new `CharacterRoster`/`CharacterDef` + `MatchConfig`; RoundManager spawns both fighters from the roster and assigns control (EnemyAI for solo/loopback, RemoteController for a live peer), parenting fighters under the fight root (survives mirror matches); `CombatSystem.Special(set,slot)` + `Fighter.SpecialSet`/`IconKey` make PlayerController/EnemyAI/TouchUI/EndingPanel champion-agnostic (HUD name/cards, VS portraits, ending-by-winner). Adding a champion = one `CharacterDef`.
- Compile-validated with Unity 6000.0.78f1's bundled Roslyn against the real UnityEngine/UGUI/glTFast reference assemblies: **baseline 21 → 0 errors**, then the full **32 → 0 errors** (again after the art-seam wiring). Could not run the editor/Android build (owner's editor holds the project lock; `adb devices` empty) — on-device is the owner's step, same as sessions 4–5.
- **Generated no artwork** (per instruction). Prepared it instead: 2 `planned` manifest rows (`ui_menu_bg`, `ui_roster_card`) + a "Planned generation queue" with pre-composed prompts + a per-champion art template; `AssetLib.SpriteOr` wired so they drop in on the next sync. `check_manifest.sh` clean. Higgsfield credits unchanged (80.75).

- Reconciled the doc-freshness warning (build churn under `game/.utmp/` + editor layout only; no source/docs stale). Read STATUS + the combat/asset docs and the full gameplay code before starting.
- Owner directive: more action / faster combat, more moves (combos/specials/varied), more animations per character, more creative freedom — keep the loop, architecture, and environment/nature art as-is. Asked one focused question (generation budget: 84.5 cr can't fund a full animation library); owner chose **code-first + cheap VFX**. Logged **D-015** (expansion; supersedes the session-4 "numbers frozen" note for combat depth/cadence) and **D-016** (swipe-away = back-dash).
- Implemented across `game/Assets/Scripts/`: new `CombatFx.cs` (hit-stop window + camera shake/punch, deliberately not `Time.timeScale`) and `ProcAnim.cs` (per-move body-motion layer on the `Visual` child only — deterministic X-axis sim untouched). Extended Fighter (dash/i-frames/parry timing/juggle/`CharStyle`), CombatSystem (parry, i-frames, air juggle w/ decay, step-in, per-hit impact FX, new AirRake/AirSlam + FxWeight), PlayerController (back-dash, air routing, chain-cancel enders, special-cancel), TouchInput (swipe-away→back-dash), EnemyAI (air follow-ups, ramped read-parry, back-dash, varied pokes), RoundManager (camera shake/punch every frame + per-round `CombatFx.Reset` + AI ramp + ProcAnim/style setup), FighterAnimator (freeze on hit-stop), TouchUI (combo counter + parry cue/flash), MainMenu (control hint). Also generated 3 move VFX (`dash_streak`/`parry_spark`/`impact_ring`, recraft_v4_1, 3.75 cr, value-keyed to alpha locally); code-composited the rest via tints of existing sprites with graceful fallbacks.
- Compile-validated all 21 scripts with Unity 6000.0.78f1 Roslyn against the real UnityEngine/UGUI/glTFast reference assemblies — 0 errors (editor holds the project lock; `adb devices` empty, so no batch/device build, same as session 4).
- Ran a 5-dimension adversarial review workflow. The org monthly spend limit hit mid-run (8 of 13 agents; `sim-invariant` + `ergonomics-brief` finders and 3 verifiers failed) — recovered the completed findings from the workflow journal and self-reviewed the two uncovered dimensions + the 3 unverified findings against the code. **All 8 findings fixed** (juggle touch-of-death → wake-up i-frames; step-in was extending reach → moved after the whiff check; uncached `Resources.Load` on the hit frame → `AssetLib.Has` memoized; VFX GC churn → `VfxFade` object pool + sprite cache; meter-positive special-cancel → specials grant 0.25× meter; parry over-rewarding → trimmed; cancel window not consumed → consumed; camera-punch residual bug). Recompiled clean.
- Higgsfield credits 84.5 → 80.75. Owner deferred the larger art generation (per-move animation library, extra VFX) to a later session after a credit refill.

### 2026-07-18 — Session 4: gameplay UI + control-scheme rework

- Reconciled docs after the owner's Unity playtest (editor now 6000.0.78f1, glTFast 6.14.1; loop/architecture/env-art frozen as approved). Flagged the stray root-level Unity project (`2eacf9e` Library churn) as noise — only `game/` is real.
- Ran a 5-dimension adversarial review workflow (ergonomics, readability, latency, correctness, brief-compliance) over the UI/control code. The org monthly spend limit hit mid-run, so only the finder pass + 2 verify agents completed; recovered all **58 raw findings** from the workflow journal and verified the rest inline against the code (already fully read). Deduped to ~30 real issues.
- Implemented the rework (**D-014**) across 12 scripts + 1 new: **TouchInput** (swipe-at-threshold commit, Update-driven 120 ms hold, freeze/pause gesture reset, away-swipe no-op, touch-down feedback), **PlayerController** (one-slot input buffer, chain-step feedback gated on connection, per-step escalation, CastSpecial outcome reporting), **TouchUI** (dual segmented meters + enemy meter, ghost-drain HP, chain pips, block glyph, card frames/cost-pips/deny feedback, sliced pause glyph, ink-panel pause overlay, timer urgency, all-buttons gesture shield), **GameManager** (pause ownership + input/AI gating, REMATCH teardown), **CombatSystem** (distinct block deflect + whiff VFX, tintable spawn, VfxFade preserves tint), **FighterAnimator** (restart same one-shot so chain hits 2/3 show), **RoundManager** (pip refresh on KO/TIME, fresh timer during banners, KHULANDRA RISES banner, vfx prewarm), **AssetLib** (measured UI-sheet slice table), **UiKit** (ink panel, touch splash, fake-null fix), **Bootstrap** (vSync policy + PerfMonitor), **MainMenu/EndingPanel/StoryIntro** (sliced button plates, ≥48 dp SKIP). New **PerfMonitor.cs**: dev fps + gesture→impact overlay (F1) for the Goal 2 bars.
- No new art generated — the existing placeholder sheets were being drawn whole; fixed by slicing (measured sub-rects, since the hand-drawn sheets aren't grid-aligned). No combat numbers changed (frozen); the meter-taken-rate deviation and two other items are logged as Open questions / Tuning log, not fixed.
- Validated: compiled all 18 scripts with Unity 6000.0.78f1's Roslyn against the real UnityEngine/UGUI/glTFast assemblies — **0 errors, 0 relevant warnings**. Could not run the editor/Android build (the owner's editor holds the project lock; no Android device attached — `adb devices` empty). On-device verification is the owner's next step.
- Added manifest row `sfx_whiff` (`planned`); logged D-014.

### 2026-07-18 — Session 3 (continued): push unblocked, KO race fix

- Owner's `id_ed25519` SSH key unlocked GitHub: remote switched to SSH, all commits + 71 LFS objects pushed; `main` tracks `origin/main`.
- Fixed a KO tally race in RoundManager (score now increments synchronously in OnKo; the K.O. banner coroutine is presentation only).
- Clarified for the owner: Unity's "GameView reduced to a reasonable size" log is informational, not an error.

### 2026-07-18 — Session 3: intake interview, design brief, full slice build

- Ran the studio intake interview (fight format: best-of-3; Khulandra event between rounds 1-2; audio: generate everything now). Wrote and locked docs/DESIGN_BRIEF.md v1.0; owner gate passed ("Generate assets and scaffold").
- Logged D-011 (owner's animation pipeline), D-012 (business model FREE), D-013 (slice format); ARCHITECTURE animation section resolved.
- Generated 6 animation clip GLBs (38 credits each; special-attack clip skipped per the 50-credit budget guard) + 3 music loops + 8 SFX + 6 announcer VO lines; converted to D-007 containers; manifest now 63 rows, all recorded with job IDs.
- Scaffolded the full Unity project in `game/`: Packages/manifest.json (glTFast 6.9.0), near-empty Boot scene wired by GUID, 18 code-first C# scripts, README with build steps and documented slice deviations (Built-in RP for zero-config open; legacy-Animation runtime playback of D-011 clips). Wrote and ran scripts/sync_assets.sh (55 files into game/Assets).
- Credits: 330.67 → 84.5.
- Next: owner opens game/ in Unity for the first playtest.

### 2026-07-18 — Session 2: owner sign-offs + full visual placeholder production

- Applied owner answers (D-009): rights/tone/style approved, commits authorized (remote github.com/RajdeepSah/KAIJU_RUIN), business model deferred; fixed the two VISION.md §-references with owner approval; flipped D-004 to accepted.
- Logged D-010: audio generation deferred to scaffold phase (MCP restricts music/SFX models to its game pipeline).
- Git: LFS set up, first commit `be7be5e`, remote added — push failed: no GitHub credentials on this machine.
- Generated all 39 visual assets + sourced 2 fonts via Higgsfield (recraft_v4_1 with the §2 palette parameter; 3D via Meshy image_to_3d rigged+textured). Every asset recorded in the manifest at generation time.
- Credits: 685.17 → 330.67.

### 2026-07-17 — Session 1: project structure, documentation workflow, verification pass

- Read the owner's `Docs/Vision.md`; moved it to `docs/VISION.md` unchanged.
- Created the repo skeleton, initialized git, authored the doc set (CLAUDE.md, README, STATUS, ART_DIRECTION, ASSET_MANIFEST, ARCHITECTURE, DECISIONS D-001…D-008), the `/wrap-session` skill, the SessionStart freshness hook, and both check scripts.
- Ran a 4-agent adversarial verification: cold-start resume passed; 37 findings applied.
- Deliberately generated no art (owner scoped the task to structure/docs only).
