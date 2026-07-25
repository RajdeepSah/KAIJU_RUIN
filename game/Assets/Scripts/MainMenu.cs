using UnityEngine;
using UnityEngine.UI;

namespace KaijuRuin
{
    // Title screen. Deliberately BUTTONLESS (D-022): key art, emblem, title,
    // and a tap-anywhere prompt. Tapping hands off to the front-end chain
    // (first run: story intro -> how to play -> fight select; afterwards straight
    // to fight select). Mode choice lives on FightSelectMenu, not here.
    // Strings are literal per DESIGN_BRIEF.md.
    public class MainMenu : MonoBehaviour
    {
        Canvas canvas;

        public void Show(System.Action onContinue)
        {
            AudioManager.I?.Music("title_theme");
            canvas = UiKit.NewCanvas("MainMenu", 20);

            // Main-menu key art is a shippable UI asset (ui/key_art, promoted from concept — D-019); harbor sky is the fallback.
            var bg = UiKit.Image(canvas.transform, "Bg", AssetLib.Sprite("ui/key_art") ?? AssetLib.Sprite("stages/harbor_sky") ?? AssetLib.Sprite("ui/vs_screen"));
            bg.preserveAspect = false;
            UiKit.Rect(bg.gameObject, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            bg.color = new Color(0.85f, 0.85f, 0.88f);   // gentle dim so the title/subtitle text stays legible over the art

            // The whole key art IS the tap target — that is what keeps the screen
            // buttonless. Transition None so the art does not flash on press.
            bg.raycastTarget = true;
            var tap = bg.gameObject.AddComponent<Button>();
            tap.transition = Selectable.Transition.None;
            tap.onClick.AddListener(() =>
            {
                AudioManager.I?.Sfx("ui_tap");
                Destroy(canvas.gameObject);
                onContinue();
            });

            var emblem = UiKit.Image(canvas.transform, "Emblem", AssetLib.Sprite("ui/emblem"));
            emblem.raycastTarget = false;    // must not punch a dead spot in the tap-anywhere art
            UiKit.Rect(emblem.gameObject, new Vector2(0.42f, 0.62f), new Vector2(0.58f, 0.95f), Vector2.zero, Vector2.zero);

            var title = UiKit.Label(canvas.transform, "Title", "REALM OF GORYO", 96, AssetLib.DisplayFont, AssetLib.BonePaper);
            UiKit.Rect(title.gameObject, new Vector2(0f, 0.46f), new Vector2(1f, 0.62f), Vector2.zero, Vector2.zero);
            var subtitle = UiKit.Label(canvas.transform, "Subtitle", "SHADOW OF GIANTS", 52, AssetLib.DisplayFont, AssetLib.GoryoFlame);
            UiKit.Rect(subtitle.gameObject, new Vector2(0f, 0.38f), new Vector2(1f, 0.47f), Vector2.zero, Vector2.zero);

            var prompt = UiKit.Label(canvas.transform, "Prompt", "TAP ANYWHERE TO BEGIN", 44, AssetLib.HudFont, AssetLib.BonePaper);
            UiKit.Rect(prompt.gameObject, new Vector2(0f, 0.2f), new Vector2(1f, 0.3f), Vector2.zero, Vector2.zero);
            prompt.gameObject.AddComponent<UiPulse>();

            var version = UiKit.Label(canvas.transform, "Version",
                "Shadow of Giants slice v0.1 - internal placeholder build",
                22, AssetLib.HudFont, AssetLib.AshSteel);
            UiKit.Rect(version.gameObject, new Vector2(0f, 0.02f), new Vector2(1f, 0.07f), Vector2.zero, Vector2.zero);
        }
    }
}
