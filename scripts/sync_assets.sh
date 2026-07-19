#!/usr/bin/env bash
# One-way sync: assets/ masters -> game/Assets (ARCHITECTURE.md contract).
# Never edit the copies in game/; replace the master and re-run this.
# Concept art is reference-only and deliberately NOT synced.
cd "$(dirname "$0")/.." || exit 1

ART=game/Assets/Resources/Art
AUD=game/Assets/Resources/Audio
FNT=game/Assets/Resources/Fonts
MDL=game/Assets/StreamingAssets/Models

mkdir -p "$ART/stages" "$ART/panels" "$ART/ui" "$ART/vfx" "$ART/characters" \
         "$AUD/music" "$AUD/sfx" "$AUD/vo" "$FNT" "$MDL"

cp -f assets/stages/*.png       "$ART/stages/"     2>/dev/null
cp -f assets/panels/*.png       "$ART/panels/"     2>/dev/null
cp -f assets/ui/*.png           "$ART/ui/"         2>/dev/null
cp -f assets/vfx/*.png          "$ART/vfx/"        2>/dev/null
cp -f assets/characters/*_portrait.png "$ART/characters/" 2>/dev/null
cp -f assets/audio/music/*      "$AUD/music/"      2>/dev/null
cp -f assets/audio/sfx/*        "$AUD/sfx/"        2>/dev/null
cp -f assets/audio/vo/*         "$AUD/vo/"         2>/dev/null
cp -f assets/fonts/*.ttf        "$FNT/"            2>/dev/null
cp -f assets/characters/*.glb   "$MDL/"            2>/dev/null

echo "Synced masters into game/Assets:"
find "$ART" "$AUD" "$FNT" "$MDL" -type f | wc -l
exit 0
