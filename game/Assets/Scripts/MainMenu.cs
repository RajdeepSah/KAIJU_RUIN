using UnityEngine;
using UnityEngine.UI;

namespace KaijuRuin
{
    // Title screen. Strings are literal per DESIGN_BRIEF.md.
    public class MainMenu : MonoBehaviour
    {
        Canvas canvas;

        public void Show(System.Action onPlay)
        {
            AudioManager.I?.Music("title_theme");
            canvas = UiKit.NewCanvas("MainMenu", 20);

            // Concept art never ships (manifest rule), so the title backdrop is the harbor sky layer.
            var bg = UiKit.Image(canvas.transform, "Bg", AssetLib.Sprite("stages/harbor_sky") ?? AssetLib.Sprite("ui/vs_screen"));
            bg.preserveAspect = false;
            UiKit.Rect(bg.gameObject, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            bg.color = new Color(0.75f, 0.75f, 0.78f);

            var emblem = UiKit.Image(canvas.transform, "Emblem", AssetLib.Sprite("ui/emblem"));
            UiKit.Rect(emblem.gameObject, new Vector2(0.42f, 0.62f), new Vector2(0.58f, 0.95f), Vector2.zero, Vector2.zero);

            var title = UiKit.Label(canvas.transform, "Title", "REALM OF GORYO", 96, AssetLib.DisplayFont, AssetLib.BonePaper);
            UiKit.Rect(title.gameObject, new Vector2(0f, 0.46f), new Vector2(1f, 0.62f), Vector2.zero, Vector2.zero);
            var subtitle = UiKit.Label(canvas.transform, "Subtitle", "SHADOW OF GIANTS", 52, AssetLib.DisplayFont, AssetLib.GoryoFlame);
            UiKit.Rect(subtitle.gameObject, new Vector2(0f, 0.38f), new Vector2(1f, 0.47f), Vector2.zero, Vector2.zero);

            var play = UiKit.ButtonSprite(canvas.transform, "Play", AssetLib.UiSlice("plate_light"), "TAP TO FIGHT", AssetLib.HudFont, () =>
            {
                AudioManager.I?.Sfx("ui_tap");
                Destroy(canvas.gameObject);
                onPlay();
            }, 52);
            UiKit.Rect(play.gameObject, new Vector2(0.36f, 0.18f), new Vector2(0.64f, 0.34f), Vector2.zero, Vector2.zero);

            var hint = UiKit.Label(canvas.transform, "Hint",
                "Tap: light chain - Swipe: heavy - Hold: block - Cards: specials",
                30, AssetLib.HudFont, AssetLib.BonePaper);
            UiKit.Rect(hint.gameObject, new Vector2(0f, 0.09f), new Vector2(1f, 0.15f), Vector2.zero, Vector2.zero);

            var version = UiKit.Label(canvas.transform, "Version",
                "Shadow of Giants slice v0.1 - internal placeholder build",
                22, AssetLib.HudFont, AssetLib.AshSteel);
            UiKit.Rect(version.gameObject, new Vector2(0f, 0.02f), new Vector2(1f, 0.07f), Vector2.zero, Vector2.zero);
        }
    }
}
