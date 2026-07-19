#!/usr/bin/env bash
# No-orphan-assets check for Kaiju Ruin.
# Diffs the assets/ tree against the canonical paths in docs/ASSET_MANIFEST.md, both directions:
#   1) files on disk with no manifest row (orphans)
#   2) rows whose status is beyond `planned` but whose file is missing
# Run by /wrap-session step 3. Exit 0 = clean, exit 1 = discrepancies listed below.
cd "$(dirname "$0")/.." || exit 1
manifest=docs/ASSET_MANIFEST.md
fail=0

files=$(find assets -type f ! -name '.gitkeep' 2>/dev/null | LC_ALL=C sort)
paths=$(grep -oE 'assets/[A-Za-z0-9_/.-]+\.[a-z0-9]+' "$manifest" | LC_ALL=C sort -u)

orphans=$(comm -23 <(printf '%s\n' "$files") <(printf '%s\n' "$paths") | sed '/^$/d')
if [ -n "$orphans" ]; then
  echo "FAIL: files in assets/ with NO manifest row (orphans):"
  echo "$orphans"
  fail=1
fi

missing=$(grep -E '\|\s*(placeholder|final|needs-rework)\s*\|' "$manifest" \
  | grep -oE 'assets/[A-Za-z0-9_/.-]+\.[a-z0-9]+' | sort -u \
  | while read -r p; do [ -f "$p" ] || echo "$p"; done)
if [ -n "$missing" ]; then
  echo "FAIL: manifest rows beyond 'planned' whose file is MISSING on disk:"
  echo "$missing"
  fail=1
fi

[ "$fail" -eq 0 ] && echo "Manifest and assets/ agree: no orphans, no missing files."
exit "$fail"
