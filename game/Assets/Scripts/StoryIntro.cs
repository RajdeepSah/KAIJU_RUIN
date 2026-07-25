using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace KaijuRuin
{
    // World/lore opening. Full-screen motion-comic beats that establish the
    // setting BEFORE the player picks a fighter, so a first-time player knows
    // what Goryo, Khulandra, Kest and Tengi are by the time they reach character
    // select. Shown ONCE per install on both the Solo and the Online path
    // (GameManager gates on Seen) and always skippable: SKIP top-right, tap to
    // advance a beat.
    //
    // Beat text is a plain-language digest of docs/LORE_BIBLE.md v1 sections 2-6:
    // every factual claim traces to a [CONFIRMED] line there (2061 Japan overrun,
    // the world uniting without igniting a world war, Mu / Anquisheng / the Ark,
    // Goryo-Khulandra-Raisha, Kest's run to the kaiju graveyard, Tengi's culling).
    // No canon is invented here. The ART is placeholder-by-contract and every
    // depiction is an [INFERENCE] (Vision section 3.5), so when the Lore Bible
    // reaches v2 (deep-source + author pass) this table is the single place to
    // correct.
    public class StoryIntro : MonoBehaviour
    {
        const string SeenKey = "kr.storyintro.seen";

        // Once per install. Replayable any time from the fight-select screen
        // (REPLAY STORY), which is also how the owner re-watches it in a playtest.
        public static bool Seen
        {
            get { return PlayerPrefs.GetInt(SeenKey, 0) == 1; }
            set { PlayerPrefs.SetInt(SeenKey, value ? 1 : 0); PlayerPrefs.Save(); }
        }

        // heading: the beat's eyebrow; art: full-screen backdrop; overlay: optional
        // cut-out drawn on top (used for the kaiju, who must break the frame rather
        // than sit inside it -- Pillar 2); caption: the lore line.
        struct Beat
        {
            public string Heading, Art, Overlay, Caption;
            public Beat(string heading, string art, string overlay, string caption)
            {
                Heading = heading; Art = art; Overlay = overlay; Caption = caption;
            }
        }

        static readonly Beat[] Beats =
        {
            new Beat("2061", "ui/key_art", null,
                "Japan is overrun. The threats come from the bottom of the ocean - and from distant planets."),
            new Beat("THE FOUR PILLARS", "panels/story_fourpillars_01", null,
                "The world unites to reclaim the planet. In Japan, scientists and soldiers move carefully: one wrong strike and the war becomes a world war."),
            new Beat("TOKYO HARBOR", "ui/menu_bg", null,
                "The harbor is a ruin of cracked piers, drowned shrines and coalition floodlights. People still live here. People still fight here."),
            new Beat("THE KAIJU", "stages/harbor_sky", "stages/khulandra_breach",
                "Goryo guards the realm. Khulandra rises from the depths. Raisha, the Riven Mother, is coming. Kaiju are weather - you survive them, you do not beat them."),
            new Beat("THE POWERS", "panels/story_fourpillars_02", null,
                "Mu has risen. The caudatas of Anquisheng dream of freedom. Hidden in Tokyo, Noah Sato builds the Ark - a world of forever."),
            new Beat("THE CHAMPIONS", "ui/vs_screen", null,
                "Kest the werefox runs for the kaiju graveyard to unleash Raisha. Tengi would cull the world to prepare it for what comes next."),
            new Beat("SHADOW OF GIANTS", "panels/story_fourpillars_03", null,
                "Two champions walk out of the ruin. Only one future walks back."),
        };

        public IEnumerator Run()
        {
            AudioManager.I?.Music("story_fourpillars");
            var canvas = UiKit.NewCanvas("StoryIntro", 30);
            bool skip = false;

            // Black floor so a beat whose art has alpha (or a missing texture)
            // never shows the fight/menu behind it.
            var floor = UiKit.Image(canvas.transform, "Floor", UiKit.WhiteSprite(), AssetLib.SumiInk);
            floor.preserveAspect = false;
            UiKit.Rect(floor.gameObject, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);

            var img = UiKit.Image(canvas.transform, "Panel", null);
            img.preserveAspect = false;
            UiKit.Rect(img.gameObject, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);

            // Kaiju cut-out: deliberately taller than the screen so it is cropped by
            // the frame and never sits fully in view (Pillar 2).
            var overlay = UiKit.Image(canvas.transform, "Overlay", null);
            overlay.preserveAspect = true;
            UiKit.Rect(overlay.gameObject, new Vector2(0.10f, 0f), new Vector2(0.90f, 1.25f), Vector2.zero, Vector2.zero);
            overlay.enabled = false;

            var capBg = UiKit.Image(canvas.transform, "CapBg", UiKit.WhiteSprite(), new Color(0, 0, 0, 0.74f));
            capBg.preserveAspect = false;
            UiKit.Rect(capBg.gameObject, Vector2.zero, new Vector2(1f, 0.25f), Vector2.zero, Vector2.zero);

            var heading = UiKit.Label(canvas.transform, "Heading", "", 40, AssetLib.DisplayFont, AssetLib.GoryoFlame, TextAnchor.MiddleLeft);
            UiKit.Rect(heading.gameObject, new Vector2(0.03f, 0.175f), new Vector2(0.7f, 0.245f), Vector2.zero, Vector2.zero);

            var caption = UiKit.Label(canvas.transform, "Caption", "", 32, AssetLib.HudFont, AssetLib.BonePaper, TextAnchor.UpperLeft);
            caption.horizontalOverflow = HorizontalWrapMode.Wrap;
            UiKit.Rect(caption.gameObject, new Vector2(0.03f, 0.05f), new Vector2(0.97f, 0.175f), Vector2.zero, Vector2.zero);

            var cont = UiKit.Label(canvas.transform, "Continue", "TAP TO CONTINUE", 26, AssetLib.HudFont, AssetLib.GoryoFlame, TextAnchor.MiddleRight);
            UiKit.Rect(cont.gameObject, new Vector2(0.6f, 0.005f), new Vector2(0.97f, 0.05f), Vector2.zero, Vector2.zero);
            cont.gameObject.AddComponent<UiPulse>();

            // Progress pips: a lore intro this long should show how long it is.
            var pips = new Image[Beats.Length];
            for (int i = 0; i < Beats.Length; i++)
            {
                pips[i] = UiKit.Image(canvas.transform, "Pip" + i, UiKit.WhiteSprite(), AssetLib.AshSteel);
                pips[i].preserveAspect = false;
                float x = 0.03f + i * 0.022f;
                UiKit.Rect(pips[i].gameObject, new Vector2(x, 0.018f), new Vector2(x + 0.014f, 0.032f), Vector2.zero, Vector2.zero);
            }

            var skipBtn = UiKit.ButtonSprite(canvas.transform, "Skip", AssetLib.UiSlice("plate_light"), "SKIP", AssetLib.HudFont,
                () => { AudioManager.I?.Sfx("ui_tap"); skip = true; }, 30);
            UiKit.Rect(skipBtn.gameObject, new Vector2(0.86f, 0.85f), new Vector2(0.99f, 0.99f), Vector2.zero, Vector2.zero);

            for (int i = 0; i < Beats.Length && !skip; i++)
            {
                var beat = Beats[i];
                img.sprite = AssetLib.Sprite(beat.Art);
                if (beat.Overlay != null && AssetLib.Has(beat.Overlay))
                {
                    overlay.sprite = AssetLib.Sprite(beat.Overlay);
                    overlay.enabled = true;
                }
                else overlay.enabled = false;

                heading.text = beat.Heading;
                caption.text = beat.Caption;
                for (int p = 0; p < pips.Length; p++)
                    pips[p].color = p <= i ? AssetLib.BonePaper : AssetLib.AshSteel;

                // Short fade in on the art only; text stays instantly readable.
                float t = 0f;
                while (t < 0.22f && !skip)
                {
                    t += Time.unscaledDeltaTime;
                    float a = Mathf.Clamp01(t / 0.22f);
                    img.color = new Color(1f, 1f, 1f, a);
                    if (overlay.enabled) overlay.color = new Color(1f, 1f, 1f, a);
                    yield return null;
                }
                img.color = Color.white;
                overlay.color = Color.white;

                // Wait for a tap. The fade doubles as the tap debounce, so one tap
                // cannot eat two beats.
                while (!skip)
                {
                    if (Input.GetMouseButtonDown(0) ||
                        (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began))
                    {
                        AudioManager.I?.Sfx("ui_tap", 0.6f);
                        break;
                    }
                    yield return null;
                }
                yield return null;
            }

            Seen = true;
            Destroy(canvas.gameObject);
        }
    }
}
