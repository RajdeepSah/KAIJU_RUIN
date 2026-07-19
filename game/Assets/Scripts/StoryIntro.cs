using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace KaijuRuin
{
    // Three-panel Four Pillars motion-comic intro; tap to advance, SKIP to bail.
    public class StoryIntro : MonoBehaviour
    {
        static readonly (string panel, string caption)[] Panels =
        {
            ("panels/story_fourpillars_01", "2061. Japan is overrun. The world calls it the year the weather learned to hate."),
            ("panels/story_fourpillars_02", "The Coalition burns district after district off the map, and still it is not enough."),
            ("panels/story_fourpillars_03", "Two champions walk out of the ruin. Only one future walks back."),
        };

        public IEnumerator Run()
        {
            AudioManager.I?.Music("story_fourpillars");
            var canvas = UiKit.NewCanvas("StoryIntro", 30);
            bool skip = false;

            var img = UiKit.Image(canvas.transform, "Panel", null);
            img.preserveAspect = false;
            UiKit.Rect(img.gameObject, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);

            var capBg = UiKit.Image(canvas.transform, "CapBg", UiKit.WhiteSprite(), new Color(0, 0, 0, 0.7f));
            capBg.preserveAspect = false;
            UiKit.Rect(capBg.gameObject, new Vector2(0f, 0f), new Vector2(1f, 0.14f), Vector2.zero, Vector2.zero);
            var caption = UiKit.Label(canvas.transform, "Caption", "", 34, AssetLib.HudFont, AssetLib.BonePaper);
            UiKit.Rect(caption.gameObject, new Vector2(0.03f, 0f), new Vector2(0.8f, 0.14f), Vector2.zero, Vector2.zero);

            var cont = UiKit.Label(canvas.transform, "Continue", "TAP TO CONTINUE", 26, AssetLib.HudFont, AssetLib.GoryoFlame, TextAnchor.MiddleRight);
            UiKit.Rect(cont.gameObject, new Vector2(0.6f, 0.005f), new Vector2(0.98f, 0.06f), Vector2.zero, Vector2.zero);

            var skipBtn = UiKit.ButtonSprite(canvas.transform, "Skip", AssetLib.UiSlice("plate_light"), "SKIP", AssetLib.HudFont, () => skip = true, 30);
            UiKit.Rect(skipBtn.gameObject, new Vector2(0.88f, 0.85f), new Vector2(0.99f, 0.99f), Vector2.zero, Vector2.zero);

            foreach (var (panel, text) in Panels)
            {
                if (skip) break;
                img.sprite = AssetLib.Sprite(panel);
                caption.text = text;
                float shown = Time.time;
                // Wait for a tap (with a small debounce so one tap does not skip two panels)
                while (!skip)
                {
                    if (Time.time - shown > 0.35f && (Input.GetMouseButtonDown(0) || Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began))
                        break;
                    yield return null;
                }
                yield return null;
            }
            Destroy(canvas.gameObject);
        }
    }
}
