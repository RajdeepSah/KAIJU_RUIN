# STATUS — Kaiju Ruin

*Last updated: 2026-07-18 (session 4). Single source of truth for project state. The top sections are rewritten every session (Done covers work since the last milestone flip — prune it when a milestone completes); the session log is append-only, newest first.*

## Snapshot

**Phase: UI + control-scheme rework landed in code — needs on-device playtest.** The owner playtested the slice in Unity **6000.0.78f1** (ProjectVersion bumped from 6000.0.40f1; glTFast resolved to 6.14.1) and froze the **gameplay loop, architecture, and environment/nature art as approved**. Session 4 reworked the gameplay UI and touch controls per the owner directive (D-014): swipe-at-threshold commit, one-slot input buffer, pause that actually gates combat, dual segmented meters (enemy meter added), ghost-drain health bars, chain/block/whiff feedback, correctly-sliced UI sprites, sub-80 ms touch acknowledgement, and a dev PerfMonitor overlay for the Goal 2 bars. **All 18 scripts compile clean** (validated with Unity's Roslyn against the real reference assemblies — the editor was open and holds the project lock, so no batch build was possible). No new art was generated (existing placeholder sheets slice fine); combat numbers untouched (frozen). Design brief locked (DESIGN_BRIEF v1.0, D-013); assets: **64 manifest rows** (63 `placeholder`, +1 new `planned` `sfx_whiff`). **Git: `origin/main` live at github.com/RajdeepSah/KAIJU_RUIN.** Higgsfield credits: 84.5 (unspent this session). Note: root-level `Library/` churn in commit `2eacf9e` is a stray Unity 6000.5.1f1 open of the repo root — only `game/` is the real project.

## Done

- ✅ **Session 4 UI/controls rework (D-014)** — 12 scripts touched under `game/Assets/Scripts/`, 1 new (`PerfMonitor.cs`). Controls: swipe commits at threshold-crossing (−80–300 ms latency), one-slot input buffer (no more dropped mash inputs / chain dead-zone), 120 ms hold-block now Update-driven, gesture reset on round-freeze/pause, away-swipe is a no-op, every touch-down gets an ink-splash. UI: dual 3-cell segmented meters + **enemy meter**, ghost-drain HP bars, chain-step pips (landed hits only), block glyph, distinct block/whiff VFX, card cost pips + cast/deny feedback, timer urgency, the missing **KHULANDRA RISES** banner, ≥48 dp pause target with a **sliced** glyph, ink-panel pause overlay. Flow: pause now gates input + AI (no more attacking/KO behind the overlay); REMATCH tears down the old fight (no ghost-fight stacking). Perf: `vSyncCount=0`, 60 fps cap held (thermals), `PerfMonitor` overlay (F1) for fps + gesture→impact.
- ✅ Compile-validated all scripts with Unity 6000.0.78f1 Roslyn + real UnityEngine/UGUI/glTFast references — 0 errors, 0 relevant warnings.
- ✅ Prior slice foundation stands: DESIGN_BRIEF v1.0 (D-011/012/013), full placeholder asset set, 6 anim clips, all audio, `sync_assets.sh`, README build guide.

## In progress

- (nothing mid-flight — session 4 rework is code-complete and compiles; awaiting on-device verification)

## Next up (in order)

1. **Owner: recompile in the open Unity editor and on-device playtest.** Focus the running editor (it will recompile the changed scripts), Play-in-editor to smoke-test the new HUD/controls (mouse: left half = move drag, right half = tap/swipe/hold; F1 toggles the PerfMonitor overlay), then **Android Build And Run** to read the real bars. Verify on device (per game/README §Performance): fps mean/1%-low ≥ 60 and gesture→impact "same frame" on the overlay; the overlay's in-process latency is ~0 by design, so also eyeball responsiveness and, for the true 80 ms budget, a 240 fps slow-mo of finger+screen. Code: `game/Assets/Scripts/` (TouchInput, TouchUI, PlayerController, PerfMonitor); bars in game/README.md.
2. **Resolve the session-4 open questions** (below) — meter-charge-rate deviation, key_art menu background, away-swipe reservation.
3. **First playtest fix round** — work the *First-pass tuning log* below against feel; adjust `HoldTime` / swipe threshold / buffer window in TouchInput and PlayerController if the new controls need it.
4. **Polish backlog (tracked, not blocking):** generate `sfx_whiff` (ASSET_MANIFEST, `planned`) and the "special attack" clip when credits allow; hand-trim music loop points; graver announcer voice; production URP switch + editor AnimatorController (game/README deviations).
5. **Lore bible v1** (Vision Goal 4, milestone M4) — remaining pre-production doc; placeholder designs stay inference-flagged until it exists.

## Open questions (owner input needed)

- **Meter charge rate for damage taken is halved vs the locked brief.** DESIGN_BRIEF MOVES says "a segment charges per 150 damage dealt or 80 taken", but `CombatSystem.cs` GainMeter-on-taken has a deliberate-looking `* 0.5f` (net ~1 segment per 160 taken). Left **unchanged** this session (combat numbers frozen). Owner call: keep the halved rate (rebalance — amend the brief + log a D-###) or restore to spec? Affects how often the player can spend ability cards.
- **Main-menu background: `key_art.png` (brief) vs `harbor_sky` (current).** DESIGN_BRIEF UI names `key_art.png`, but that asset is classified `concept_key-art` and `sync_assets.sh` excludes concept art from shipping, so `MainMenu.cs` substitutes the harbor sky layer. Keep the substitution, or promote a menu-specific key-art asset into the shippable set?
- **Away-swipe (right half) is now a no-op** — reserve it for a future back-dash, or leave unbound?

## First-pass tuning log (confirm on device — NOT fixed this session)

Feel/number items surfaced by the session-4 review, from static reading of the frozen DESIGN_BRIEF numbers. Combat numbers are frozen, so these are logged, not changed — work them in the "first playtest fix round".

1. **Meter-taken rate halved vs spec** — see Open questions. The `* 0.5f` in `CombatSystem.cs` slows how fast getting hit charges meter; card economy will feel stingier than the brief implies.
2. **Chain cadence is now recovery-limited** — with the new input buffer, chain hits 2/3 fire the frame jab/cross recovery (0.25 s) ends, i.e. a deterministic ~250 ms between taps. Confirm this feels responsive rather than sluggish; if too slow, the lever is jab/cross `Recovery` (frozen number — needs a decision) not the buffer.
3. **Hold-to-block engages at 120 ms** (`TouchInput.HoldTime`). A slow, stationary tap held past 120 ms now becomes a block instead of a jab. Confirm this isn't triggering accidental blocks; `HoldTime` is a cheap tuning knob if so.
4. **Swipe threshold = 4% screen width** (`SwipeMinPixelsFactor`). Now that swipes commit at threshold-crossing, this doubles as the commit trigger — verify heavies/launchers/sweeps aren't too twitchy (misreads as swipe) or too stiff (reads as tap). Tunable.
5. **Special attack still animates as the punch clip** (special GLB deferred, D-013 budget guard) — visual only; the VFX overlay carries it. Regenerate when credits allow.

## Milestones (tracking Vision Horizon 1)

| # | Milestone | Status |
|---|---|---|
| M0 | Repo, docs workflow (mechanically enforced), art direction proposal | ✅ done |
| M1 | Style approved (D-004 accepted); style test + concept art locked | ✅ done 2026-07-18 |
| M2 | Full placeholder asset set | ✅ done 2026-07-18 (62/63 rows; special-attack clip deferred by design) |
| M3 | Design brief locked + animation pipeline decided | ✅ done 2026-07-18 (DESIGN_BRIEF v1.0, D-011/D-012/D-013) |
| M4 | Rights/adaptation terms in writing + lore bible v1 (Vision Goal 4) | ◐ rights confirmed (D-009); lore bible v1 pending |
| M5 | Unity project scaffolded; Kest vs Tengi playable gray-box | ✅ done 2026-07-18 — owner playtested; loop/architecture/env-art approved |
| M6 | Vertical slice per Vision Goal 1 (full touch combat, living stage, Horrific Endings, motion-comic) | ◐ UI/controls reworked + compile-clean (D-014); needs on-device playtest + Goal 2 bar verification |

## Session log (append-only, newest first)

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
