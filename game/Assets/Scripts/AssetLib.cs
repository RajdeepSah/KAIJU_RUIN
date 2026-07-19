using System.Collections.Generic;
using UnityEngine;

namespace KaijuRuin
{
    // Runtime asset access. Art ships as plain textures under Resources/Art
    // (textures import with the Default importer, so sprites are created here
    // rather than relying on per-asset importer settings).
    public static class AssetLib
    {
        static readonly Dictionary<string, Sprite> spriteCache = new Dictionary<string, Sprite>();

        public static Texture2D Tex(string path)
        {
            var t = Resources.Load<Texture2D>("Art/" + path);
            if (t == null) Debug.LogWarning("Missing texture: Art/" + path);
            return t;
        }

        public static Sprite Sprite(string path, float pixelsPerUnit = 100f)
        {
            var key = path + "@" + pixelsPerUnit;
            if (spriteCache.TryGetValue(key, out var s)) return s;
            var t = Tex(path);
            if (t == null) return null;
            s = UnityEngine.Sprite.Create(t, new Rect(0, 0, t.width, t.height),
                new Vector2(0.5f, 0.5f), pixelsPerUnit);
            spriteCache[key] = s;
            return s;
        }

        // Measured sub-rects for the multi-tile UI sheets (pixel coords, Unity
        // bottom-left origin). The placeholder sheets are hand-drawn and NOT
        // grid-aligned, so tiles are cut at measured content bounds instead of
        // even thirds/quarters. Re-measure these when a sheet is replaced
        // (ART_DIRECTION.md section 4 replacement contract).
        static readonly Dictionary<string, (string path, Rect rect)> uiSlices =
            new Dictionary<string, (string, Rect)>
        {
            { "pause_glyph",     ("ui/icon_sheet",          new Rect(8, 0, 190, 256)) },
            { "gear_glyph",      ("ui/icon_sheet",          new Rect(250, 0, 250, 256)) },
            { "restart_glyph",   ("ui/icon_sheet",          new Rect(528, 0, 226, 256)) },
            { "shield_glyph",    ("ui/icon_sheet",          new Rect(798, 0, 218, 256)) },
            { "plate_light",     ("ui/button_set",          new Rect(224, 258, 572, 250)) },
            { "plate_dark",      ("ui/button_set",          new Rect(224, 4, 572, 250)) },
            { "panel_frame_tight",("ui/panel_frame",        new Rect(123, 19, 310, 474)) },
            { "icon_kest_1",     ("ui/ability_icons_kest",  new Rect(358, 100, 262, 362)) },
            { "icon_kest_2",     ("ui/ability_icons_kest",  new Rect(660, 118, 260, 344)) },
            { "icon_kest_3",     ("ui/ability_icons_kest",  new Rect(936, 118, 264, 276)) },
            { "icon_tengi_1",    ("ui/ability_icons_tengi", new Rect(148, 10, 390, 490)) },
            { "icon_tengi_2",    ("ui/ability_icons_tengi", new Rect(591, 14, 359, 384)) },
            { "icon_tengi_3",    ("ui/ability_icons_tengi", new Rect(1023, 19, 377, 374)) },
        };

        public static Sprite UiSlice(string key)
        {
            if (spriteCache.TryGetValue("slice:" + key, out var cached)) return cached;
            if (!uiSlices.TryGetValue(key, out var def)) { Debug.LogWarning("Unknown UI slice: " + key); return null; }
            var t = Tex(def.path);
            if (t == null) return null;
            var r = def.rect;
            r.xMin = Mathf.Clamp(r.xMin, 0, t.width); r.xMax = Mathf.Clamp(r.xMax, 0, t.width);
            r.yMin = Mathf.Clamp(r.yMin, 0, t.height); r.yMax = Mathf.Clamp(r.yMax, 0, t.height);
            var s = UnityEngine.Sprite.Create(t, r, new Vector2(0.5f, 0.5f), 100f);
            spriteCache["slice:" + key] = s;
            return s;
        }

        public static Font HudFont =>
            Resources.Load<Font>("Fonts/hud") ?? UnityEngine.Font.CreateDynamicFontFromOSFont("Arial", 24);

        public static Font DisplayFont =>
            Resources.Load<Font>("Fonts/display") ?? HudFont;

        // Palette (ART_DIRECTION.md section 2)
        public static readonly Color SumiInk = Hex("15171C");
        public static readonly Color BonePaper = Hex("E9E2D0");
        public static readonly Color AshSteel = Hex("5A636E");
        public static readonly Color BloodSeal = Hex("A6212C");
        public static readonly Color GoryoFlame = Hex("3FB08F");
        public static readonly Color SignalAmber = Hex("C88A3A");

        static Color Hex(string h)
        {
            ColorUtility.TryParseHtmlString("#" + h, out var c);
            return c;
        }
    }
}
