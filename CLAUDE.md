# CLAUDE.md — Kaiju Ruin (Realm of Goryo: Shadow of Giants)

2D touch fighting game for Android (Unity 6 LTS + URP), set in the *Realm of Goryo* novels. Internal codename **Kaiju Ruin**. Currently in **pre-production**: no game code exists yet.

## Session protocol

**Start of session — read in this order, nothing more:**

1. `docs/STATUS.md` — current state, in-progress work, next steps. The single source of truth for "where were we". Every Next-up item cites the doc files, sections, and IDs needed to execute it — follow those citations instead of scanning the repo.
2. Only the docs your task touches (see map below). Do not re-scan the repo or re-derive context STATUS already records.

A SessionStart hook (`.claude/settings.json` → `scripts/check_docs_fresh.sh`) warns automatically if project files are newer than the docs — the signature of a previous session that died before wrapping. If it warns, reconcile the docs *first*.

**End of session — always, before finishing:**

Run the `/wrap-session` skill (or follow `.claude/skills/wrap-session/SKILL.md` manually). Docs must describe the repo as it is *now*; a session is not done until STATUS.md says what changed, what is mid-flight, and what comes next.

## Doc map

| Doc | Contains | Update when |
|---|---|---|
| `docs/STATUS.md` | Snapshot, in-progress, next steps, open questions, milestones, session log | Every session |
| `docs/VISION.md` | Why the game exists: pillars, horizons, canon digest (owner-authored) | Owner only — never edit without explicit instruction |
| `docs/ART_DIRECTION.md` | Style bible, UI guidelines, prompt library, asset pipeline + replacement contract | When the approved style evolves |
| `docs/ASSET_MANIFEST.md` | Every asset: ID, canonical path, spec, status, subject line, generation record | Immediately when an asset is touched |
| `docs/ARCHITECTURE.md` | Stack intent, planned layout, code↔asset contract, testing intent | When tech decisions land |
| `docs/DECISIONS.md` | Append-only decision log (D-###) | Same session a decision is made |

`docs/DESIGN_BRIEF.md` does not exist yet — it is produced by the studio intake interview (see STATUS next steps) and unlocks coding.

## Ground rules

1. **No game code yet.** The Unity project in `game/` is created only after `docs/DESIGN_BRIEF.md` exists (D-006). Stack intent is already fixed by VISION.md §11.
2. **Canon is law.** VISION.md is owner-authored. Respect its pillars — especially P1 (the novels are the engine), P2 (kaiju are weather: never shrunk, never cute, never fully in frame beside a human), and P4 (readable brutality; hard lines: no sexual violence, no torture-as-reward, no extremist iconography, nothing involving minors). Character/kaiju appearances are inferences, not canon (Vision §3.5).
3. **Placeholder art is internal-only, and nothing gets deployed.** All generated assets are unpublished stand-ins pending rights-holder approval. Never publish, deploy, or share them outside this repo — and never call Higgsfield deploy/publish/website tools (`deploy_game`, `publish_game`, `deploy_website`, `create_website`) for this project. Higgsfield is generation-only here; builds are local APKs only (Vision §11).
4. **No orphan assets.** Every asset enters through the pipeline in ART_DIRECTION.md §4, and its manifest row + generation record are written **immediately after generation**, not batched to session end. `scripts/check_manifest.sh` verifies; a discrepancy is a bug.
5. **Decisions get logged.** Anything that constrains future work becomes a D-### entry in DECISIONS.md the same session.

## Asset pipeline

One home: **ART_DIRECTION.md §4** (steps: spec → prompt → generate → post-process → place → record immediately; plus the per-type replacement contract). The prompt template is ART_DIRECTION.md §5. Do not work from memory of these — open the doc.

## Conventions

- Asset IDs: `<category>_<subject>_<variant>` (e.g. `char_kest_portrait`). Files: `snake_case`, no version numbers (git is the history once commits are enabled).
- Asset statuses: `planned → placeholder → final`, plus `needs-rework`. Derived assets flip to `needs-rework` when their source changes (ART_DIRECTION §4).
- STATUS.md is rewritten freely (update its "Last updated" line); its session log and all of DECISIONS.md are append-only — with one allowed mutation: the status line of a `PROPOSED` decision may be flipped to accepted/rejected with the sign-off date.
- Every STATUS Next-up item must cite the doc files, sections, and IDs a fresh session needs to execute it.
- Git: at session end, stage all changes. Commit only once the owner has authorized session-end commits (open question in STATUS.md; log the authorization as a D-### when it lands). Until the first commit exists, treat overwriting any generated asset as destructive — there is no history yet.
- C# coding standards: add a section here when code starts (none needed until then).

## Tooling

- **Higgsfield MCP** — asset generation only: `generate_image`, `generate_audio`, `generate_3d`, `remove_background`, `outpaint_image`, `upscale_image`, `models_explore` (action `recommend`). Deploy/publish tools are off-limits (ground rule 3).
- **`game-studio` skill** — use for the studio intake interview → design brief → Unity scaffold, when the owner green-lights that step.
- **`scripts/check_docs_fresh.sh`** — doc freshness (runs at session start via hook); **`scripts/check_manifest.sh`** — manifest↔assets/ consistency (runs in /wrap-session).
