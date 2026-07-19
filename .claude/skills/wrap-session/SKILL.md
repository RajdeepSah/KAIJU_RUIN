---
name: wrap-session
description: End-of-session documentation update for Kaiju Ruin — records what was completed, what is in progress, next steps, and decisions into docs/STATUS.md and friends, so the next session resumes without rescanning the repo. Use when the user says "wrap up", "end session", "save progress", or before ending any substantial working session.
---

Bring the docs in sync with reality, in this order:

1. **docs/STATUS.md** (always):
   - Update the "Last updated" header line (date + session number).
   - Rewrite Snapshot / Done / In progress / Next up to match the repo *right now*. Every in-progress item must say exactly where it stands and what the very next action is. Every Next-up item must cite the doc files, sections, and IDs (e.g. "D-005", "ART_DIRECTION §4") a fresh session needs to execute it.
   - Prune Done when a milestone flips (Done covers "since the last milestone"; older history lives in the session log and git).
   - Refresh Open questions and Milestones if they moved.
   - Append a session-log entry (date, one-line title, 3–6 bullets: what changed, what was decided, what comes next).
2. **docs/DECISIONS.md** — append a D-### entry for any decision made this session that constrains future work. Flip the status line of any `PROPOSED` entry the owner ruled on this session (the one allowed mutation; note the date). Never otherwise edit old entries — supersede them with a new D-###.
3. **docs/ASSET_MANIFEST.md** — generation records are written *at generation time*, not here; this step **verifies**: run `scripts/check_manifest.sh` and fix anything it reports (orphan files, missing files). Also check derived assets: if any source asset changed this session, its derived rows must be `needs-rework` (ART_DIRECTION §4).
   - Once `game/` exists: verify `game/Assets/Art/` mirrors `assets/` (run the sync script / diff timestamps); re-sync anything stale.
4. **Other docs, only if this session invalidated them** — ART_DIRECTION.md for style changes, ARCHITECTURE.md for tech changes, CLAUDE.md for workflow changes, README.md for structure changes. Do not touch docs that are still true.
5. **Git** — stage everything. Commit with a message summarizing the session *only if* the owner has authorized session-end commits (STATUS open questions / a D-### records this); otherwise leave staged and say so explicitly in the handoff.
6. **Freshness check** — run `scripts/check_docs_fresh.sh`; it must report fresh. If it warns, a doc update was missed — fix it before finishing.
7. Finish with a 3-line handoff summary: **done / open / next**.
