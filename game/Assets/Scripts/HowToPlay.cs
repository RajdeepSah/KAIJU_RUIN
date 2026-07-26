using UnityEngine;
using UnityEngine.UI;

namespace KaijuRuin
{
    // Controls primer (D-022). Auto-opens once after the story intro on a first
    // run, and stays available from FightSelectMenu's HOW TO PLAY button. Drawn on
    // a canvas ABOVE the front-end screens with a raycast-blocking dim, so it can
    // be layered over fight select without tearing that screen down.
    //
    // The lines mirror DESIGN_BRIEF.md TOUCH CONTROLS + MOVES v2 — one thumb, real
    // decisions (Pillar 3). Keep them in sync when a binding changes.
    public class HowToPlay : MonoBehaviour
    {
        Canvas canvas;

        // Kept short deliberately: each line must fit one column at 24 pt without
        // wrapping into its neighbour's row. Left column = stance and defence, right
        // column = the moveset, because the guard rules are what make the moveset
        // mean anything (D-024).
        static readonly string[] Left =
        {
            "LEFT SIDE, drag: walk toward or away.",
            "Hold AWAY: retreat with a standing guard up.",
            "Hold DOWN-BACK: crouch guard. DOWN: crouch.",
            "Standing guard stops highs and mids - not lows.",
            "Crouch guard stops lows - not overhead slams.",
            "Grapples ignore BOTH guards. Only range stops one.",
            "RIGHT SIDE hold: committed guard, and it can PARRY -",
            "guard as the hit lands: no damage, foe stunned, +meter.",
        };

        static readonly string[] Right =
        {
            "RIGHT SIDE, tap: claw jab, cross, hook (3-hit chain).",
            "Swipe toward: haymaker. Swipe up: rising claw.",
            "Swipe down: tail sweep (LOW). Away: back-dash.",
            "Up-and-in: OVERHEAD slam. Down-and-in: haunch bash.",
            "Up-and-away: GRAPPLE throw. Down-and-away: leg sweep (LOW).",
            "Crouching: tap = leg sweep, swipe in = tail roundhouse.",
            "Foe airborne? Tap = Air Rake, up = Air Slam.",
            "Cards (bottom right): specials, 1-3 meter segments.",
        };

        public void Show(System.Action onClose)
        {
            canvas = UiKit.NewCanvas("HowToPlay", 32);

            var dim = UiKit.Image(canvas.transform, "Dim", UiKit.WhiteSprite(), new Color(0, 0, 0, 0.72f));
            dim.preserveAspect = false;
            dim.raycastTarget = true;    // swallow taps meant for the screen underneath
            UiKit.Rect(dim.gameObject, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);

            var content = UiKit.InkPanel(canvas.transform, "Panel", new Vector2(0.07f, 0.08f), new Vector2(0.93f, 0.94f));

            var head = UiKit.Label(content.transform, "Head", "HOW TO PLAY", 56, AssetLib.DisplayFont, AssetLib.BonePaper);
            UiKit.Rect(head.gameObject, new Vector2(0f, 0.85f), new Vector2(1f, 0.97f), Vector2.zero, Vector2.zero);

            var lead = UiKit.Label(content.transform, "Lead",
                "One thumb. The screen is split: move on the left, fight on the right.",
                28, AssetLib.HudFont, AssetLib.GoryoFlame);
            UiKit.Rect(lead.gameObject, new Vector2(0.04f, 0.77f), new Vector2(0.96f, 0.85f), Vector2.zero, Vector2.zero);

            Column(content.transform, "L", Left, 0.05f, 0.49f);
            Column(content.transform, "R", Right, 0.51f, 0.95f);

            var kaiju = UiKit.Label(content.transform, "Kaiju",
                "Best of 3 rounds, 60 s each; PAUSE is top-right. Khulandra breaches the harbor after round one and floods the stage - the water is weather, not an enemy.",
                22, AssetLib.HudFont, AssetLib.SignalAmber);
            kaiju.horizontalOverflow = HorizontalWrapMode.Wrap;
            UiKit.Rect(kaiju.gameObject, new Vector2(0.05f, 0.19f), new Vector2(0.95f, 0.3f), Vector2.zero, Vector2.zero);

            var close = UiKit.ButtonSprite(content.transform, "Close", AssetLib.UiSlice("plate_light"), "GOT IT", AssetLib.HudFont,
                () => { AudioManager.I?.Sfx("ui_tap"); Destroy(canvas.gameObject); onClose(); }, 42);
            UiKit.Rect(close.gameObject, new Vector2(0.38f, 0.04f), new Vector2(0.62f, 0.16f), Vector2.zero, Vector2.zero);
        }

        static void Column(Transform parent, string tag, string[] lines, float xMin, float xMax)
        {
            // 8 rows at 0.055 bottom out at 0.31 — just clear of the amber note at 0.30.
            const float top = 0.75f, rowH = 0.055f;
            for (int i = 0; i < lines.Length; i++)
            {
                var t = UiKit.Label(parent, tag + i, lines[i], 24, AssetLib.HudFont, AssetLib.BonePaper, TextAnchor.MiddleLeft);
                t.horizontalOverflow = HorizontalWrapMode.Wrap;
                float y1 = top - i * rowH;
                UiKit.Rect(t.gameObject, new Vector2(xMin, y1 - rowH), new Vector2(xMax, y1), Vector2.zero, Vector2.zero);
            }
        }
    }
}
