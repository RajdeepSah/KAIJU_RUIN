using UnityEngine;
using UnityEngine.UI;

namespace KaijuRuin
{
    // Fight selection (D-022): the mode picker that used to live on the title
    // screen. Solo, or multiplayer (quick match / rooms, chosen on the next
    // screen). Also the permanent home of the two onboarding entry points, so a
    // once-only story intro is never lost: HOW TO PLAY and REPLAY STORY.
    // Scaling rule is unchanged from D-017 — a new mode is one more button here.
    public class FightSelectMenu : MonoBehaviour
    {
        Canvas canvas;

        public void Show(System.Action onSolo, System.Action onMultiplayer,
            System.Action onHowToPlay, System.Action onReplayStory, System.Action back)
        {
            AudioManager.I?.Music("title_theme");
            canvas = UiKit.NewCanvas("FightSelect", 21);

            var bg = UiKit.Image(canvas.transform, "Bg", AssetLib.SpriteOr("ui/menu_bg", "stages/harbor_sky") ?? UiKit.WhiteSprite());
            bg.preserveAspect = false;
            UiKit.Rect(bg.gameObject, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            bg.color = new Color(0.18f, 0.19f, 0.22f);

            var title = UiKit.Label(canvas.transform, "Title", "CHOOSE YOUR FIGHT", 72, AssetLib.DisplayFont, AssetLib.BonePaper);
            UiKit.Rect(title.gameObject, new Vector2(0f, 0.8f), new Vector2(1f, 0.94f), Vector2.zero, Vector2.zero);

            Plate("Solo", "SOLO FIGHT", 0.615f, 48, () => { Destroy(canvas.gameObject); onSolo(); });
            Plate("Multiplayer", "PLAY MULTIPLAYER", 0.475f, 44, () => { Destroy(canvas.gameObject); onMultiplayer(); });

            var sub = UiKit.Label(canvas.transform, "Sub",
                "Solo fights the AI - Multiplayer finds a worldwide opponent or a friend by room code",
                24, AssetLib.HudFont, AssetLib.AshSteel);
            UiKit.Rect(sub.gameObject, new Vector2(0f, 0.355f), new Vector2(1f, 0.4f), Vector2.zero, Vector2.zero);

            // Onboarding stays reachable forever, even though the intro auto-plays once.
            var how = UiKit.ButtonSprite(canvas.transform, "HowToPlay", AssetLib.UiSlice("plate_light"), "HOW TO PLAY", AssetLib.HudFont,
                () => { AudioManager.I?.Sfx("ui_tap"); onHowToPlay(); }, 34);
            UiKit.Rect(how.gameObject, new Vector2(0.28f, 0.235f), new Vector2(0.48f, 0.325f), Vector2.zero, Vector2.zero);

            var story = UiKit.ButtonSprite(canvas.transform, "ReplayStory", AssetLib.UiSlice("plate_light"), "REPLAY STORY", AssetLib.HudFont,
                () => { AudioManager.I?.Sfx("ui_tap"); Destroy(canvas.gameObject); onReplayStory(); }, 34);
            UiKit.Rect(story.gameObject, new Vector2(0.52f, 0.235f), new Vector2(0.72f, 0.325f), Vector2.zero, Vector2.zero);

            var hint = UiKit.Label(canvas.transform, "Hint",
                "Tap: chain - Swipe in: heavy - Up: launch - Down: sweep - Away: dodge - Hold: block/parry - Cards: specials",
                24, AssetLib.HudFont, AssetLib.BonePaper);
            UiKit.Rect(hint.gameObject, new Vector2(0f, 0.15f), new Vector2(1f, 0.2f), Vector2.zero, Vector2.zero);

            var version = UiKit.Label(canvas.transform, "Version",
                "Shadow of Giants slice v0.1 - internal placeholder build",
                22, AssetLib.HudFont, AssetLib.AshSteel);
            UiKit.Rect(version.gameObject, new Vector2(0f, 0.02f), new Vector2(1f, 0.07f), Vector2.zero, Vector2.zero);

            var backBtn = UiKit.ButtonSprite(canvas.transform, "Back", AssetLib.UiSlice("plate_light"), "BACK", AssetLib.HudFont,
                () => { AudioManager.I?.Sfx("ui_tap"); Destroy(canvas.gameObject); back(); }, 38);
            UiKit.Rect(backBtn.gameObject, new Vector2(0.03f, 0.04f), new Vector2(0.16f, 0.12f), Vector2.zero, Vector2.zero);
        }

        void Plate(string name, string label, float yCenter, int fontSize, UnityEngine.Events.UnityAction onClick)
        {
            var b = UiKit.ButtonSprite(canvas.transform, name, AssetLib.UiSlice("plate_light"), label, AssetLib.HudFont,
                () => { AudioManager.I?.Sfx("ui_tap"); onClick(); }, fontSize);
            UiKit.Rect(b.gameObject, new Vector2(0.35f, yCenter - 0.055f), new Vector2(0.65f, yCenter + 0.055f), Vector2.zero, Vector2.zero);
        }
    }
}
