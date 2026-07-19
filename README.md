# Kaiju Ruin — *Realm of Goryo: Shadow of Giants*

> Internal codename: **Kaiju Ruin**. Working title: **Realm of Goryo: Shadow of Giants** (pending rights-holder validation).

A mature, comic-book-styled 2D fighting game for Android, set in the *Realm of Goryo* novels: human, caudata, and stranger champions battle in the shadow of god-sized kaiju. Built in Unity 6 LTS, touch-native, playable seriously with one thumb.

**Stage: pre-production.** Vision locked, art direction proposed, no code yet.

## Repository map

| Path | What it is |
|---|---|
| [CLAUDE.md](CLAUDE.md) | AI-collaborator entry point: session protocol and ground rules |
| [docs/STATUS.md](docs/STATUS.md) | **Start here** — current state, next steps, session log |
| [docs/VISION.md](docs/VISION.md) | Product vision: why the game exists, pillars, three-year horizons |
| [docs/ART_DIRECTION.md](docs/ART_DIRECTION.md) | Style bible, UI guidelines, prompt library, asset pipeline |
| [docs/ASSET_MANIFEST.md](docs/ASSET_MANIFEST.md) | Ledger of every asset: ID, path, status, provenance |
| [docs/ARCHITECTURE.md](docs/ARCHITECTURE.md) | Tech stack and the code↔asset contract |
| [docs/DECISIONS.md](docs/DECISIONS.md) | Append-only log of decisions and their rationale |
| [assets/](assets/) | Master art/audio library (currently empty — Higgsfield placeholders first, official art later) |
| [scripts/](scripts/) | Repo checks: doc freshness, manifest↔assets consistency |
| [game/](game/) | Unity project — created once the design brief is locked |

## How this repo stays resumable

Every working session ends by bringing the docs in sync with reality (the `/wrap-session` skill automates the checklist, and a session-start hook warns if a previous session died before wrapping). A returning collaborator — human or AI — reads `docs/STATUS.md` and knows what is done, what is in flight, what comes next, and why past decisions were made, without re-reading anything else.

All placeholder art will be generated with Higgsfield and tracked in the asset manifest so the official art team can later replace any asset by overwriting a single file (contract: `docs/ART_DIRECTION.md` §4). Nothing generated here is published or shared outside this repo.
