#!/usr/bin/env bash
# Self-healing doc-freshness check for Kaiju Ruin.
# Warns when project files changed after the docs were last updated —
# the signature of a session that ended without /wrap-session.
# Run by the SessionStart hook (.claude/settings.json) and by /wrap-session step 7.
cd "$(dirname "$0")/.." || exit 0

if [ ! -f docs/STATUS.md ]; then
  echo "WARNING: docs/STATUS.md is missing — the resume anchor is gone. Recreate it before doing anything else."
  exit 0
fi

# Reference point: the newest doc (any doc touched at wrap time counts).
ref=$(ls -t docs/*.md CLAUDE.md 2>/dev/null | head -1)

stale=$(find . -type f \
  ! -path './.git/*' ! -path './docs/*' ! -path './scratch/*' \
  ! -path './.vscode/*' ! -path './.idea/*' \
  ! -path './game/Library/*' ! -path './game/Temp/*' ! -path './game/[Oo]bj/*' ! -path './game/[Ll]ogs/*' \
  ! -name 'CLAUDE.md' ! -name '.gitkeep' \
  -newer "$ref" 2>/dev/null | sort | head -20)

if [ -n "$stale" ]; then
  echo "DOC FRESHNESS WARNING: the following files are newer than every doc — the previous session likely ended without /wrap-session. Reconcile docs/STATUS.md (and ASSET_MANIFEST/DECISIONS if assets or decisions changed) before starting new work:"
  echo "$stale"
else
  echo "Docs are fresh: no project file is newer than the documentation."
fi
exit 0
