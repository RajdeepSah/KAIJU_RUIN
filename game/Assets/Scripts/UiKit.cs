using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace KaijuRuin
{
    // Small helpers for building uGUI in code.
    public static class UiKit
    {
        public static Canvas NewCanvas(string name, int sortOrder = 0)
        {
            var go = new GameObject(name, typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            var canvas = go.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = sortOrder;
            var scaler = go.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;
            EnsureEventSystem();
            return canvas;
        }

        static void EnsureEventSystem()
        {
            if (Object.FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>() != null) return;
            new GameObject("EventSystem",
                typeof(UnityEngine.EventSystems.EventSystem),
                typeof(UnityEngine.EventSystems.StandaloneInputModule));
        }

        public static RectTransform Rect(GameObject go, Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax)
        {
            // Explicit Unity-null check: `?? AddComponent` relies on reference
            // equality and skips UnityEngine.Object's overloaded ==, which can
            // mis-handle the fake-null wrapper. GameObjects created bare (e.g. the
            // pause overlay) have a Transform but no RectTransform.
            var rt = go.GetComponent<RectTransform>();
            if (rt == null) rt = go.AddComponent<RectTransform>();
            rt.anchorMin = anchorMin; rt.anchorMax = anchorMax;
            rt.offsetMin = offsetMin; rt.offsetMax = offsetMax;
            return rt;
        }

        // Comic-panel frame: Sumi Ink fill with a Bone Paper ink-rule border
        // (ART_DIRECTION section 3). Returns the content root to parent into.
        public static GameObject InkPanel(Transform parent, string name,
            Vector2 anchorMin, Vector2 anchorMax, float borderPx = 3f)
        {
            var border = Image(parent, name, WhiteSprite(), AssetLib.BonePaper);
            border.preserveAspect = false;
            Rect(border.gameObject, anchorMin, anchorMax, Vector2.zero, Vector2.zero);
            var fill = Image(border.transform, "Fill", WhiteSprite(), AssetLib.SumiInk);
            fill.preserveAspect = false;
            Rect(fill.gameObject, Vector2.zero, Vector2.one,
                new Vector2(borderPx, borderPx), new Vector2(-borderPx, -borderPx));
            return fill.gameObject;
        }

        public static Image Image(Transform parent, string name, Sprite sprite, Color? color = null)
        {
            var go = new GameObject(name, typeof(Image));
            go.transform.SetParent(parent, false);
            var img = go.GetComponent<Image>();
            img.sprite = sprite;
            img.color = color ?? Color.white;
            img.preserveAspect = true;
            return img;
        }

        public static Text Label(Transform parent, string name, string text, int size, Font font, Color color,
            TextAnchor anchor = TextAnchor.MiddleCenter)
        {
            var go = new GameObject(name, typeof(Text));
            go.transform.SetParent(parent, false);
            var t = go.GetComponent<Text>();
            t.text = text;
            t.font = font;
            t.fontSize = size;
            t.color = color;
            t.alignment = anchor;
            t.horizontalOverflow = HorizontalWrapMode.Overflow;
            t.verticalOverflow = VerticalWrapMode.Overflow;
            t.raycastTarget = false;
            return t;
        }

        public static Button ButtonSprite(Transform parent, string name, Sprite sprite, string label, Font font,
            UnityAction onClick, int fontSize = 40)
        {
            var img = Image(parent, name, sprite);
            img.raycastTarget = true;
            var btn = img.gameObject.AddComponent<Button>();
            btn.onClick.AddListener(onClick);
            if (!string.IsNullOrEmpty(label))
            {
                var t = Label(img.transform, "Label", label, fontSize, font, AssetLib.SumiInk);
                Rect(t.gameObject, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            }
            return btn;
        }

        public static Image Bar(Transform parent, string name, Color fillColor)
        {
            var frame = new GameObject(name, typeof(Image));
            frame.transform.SetParent(parent, false);
            frame.GetComponent<Image>().color = new Color(0, 0, 0, 0.55f);
            var fillGo = new GameObject("Fill", typeof(Image));
            fillGo.transform.SetParent(frame.transform, false);
            var fill = fillGo.GetComponent<Image>();
            fill.color = fillColor;
            fill.type = UnityEngine.UI.Image.Type.Filled;
            fill.fillMethod = UnityEngine.UI.Image.FillMethod.Horizontal;
            fill.sprite = WhiteSprite();
            Rect(fillGo, Vector2.zero, Vector2.one, new Vector2(4, 4), new Vector2(-4, -4));
            return fill;
        }

        static Sprite white;
        public static Sprite WhiteSprite()
        {
            if (white != null) return white;
            var t = new Texture2D(4, 4);
            var px = new Color[16];
            for (int i = 0; i < 16; i++) px[i] = Color.white;
            t.SetPixels(px); t.Apply();
            white = UnityEngine.Sprite.Create(t, new Rect(0, 0, 4, 4), new Vector2(0.5f, 0.5f));
            return white;
        }

        static Sprite ring;
        // Soft ring used for the touch-down ink acknowledgement.
        public static Sprite RingSprite()
        {
            if (ring != null) return ring;
            const int n = 64;
            var t = new Texture2D(n, n) { wrapMode = TextureWrapMode.Clamp };
            var px = new Color[n * n];
            float c = (n - 1) * 0.5f;
            for (int y = 0; y < n; y++)
                for (int x = 0; x < n; x++)
                {
                    float d = Mathf.Sqrt((x - c) * (x - c) + (y - c) * (y - c)) / c; // 0..~1
                    float a = Mathf.Clamp01(1f - Mathf.Abs(d - 0.78f) * 6f);          // bright ring
                    px[y * n + x] = new Color(1f, 1f, 1f, a);
                }
            t.SetPixels(px); t.Apply();
            ring = UnityEngine.Sprite.Create(t, new Rect(0, 0, n, n), new Vector2(0.5f, 0.5f));
            return ring;
        }

        // Spawn a fading ring at a screen-space pixel on an overlay canvas.
        public static void Splash(Canvas overlayCanvas, Vector2 screenPos, Color color, float size = 90f)
        {
            if (overlayCanvas == null) return;
            var img = Image(overlayCanvas.transform, "TouchSplash", RingSprite(), color);
            img.raycastTarget = false;
            var rt = img.rectTransform;
            rt.anchorMin = rt.anchorMax = Vector2.zero;
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = new Vector2(size, size);
            float sf = overlayCanvas.scaleFactor <= 0f ? 1f : overlayCanvas.scaleFactor;
            rt.anchoredPosition = screenPos / sf;
            img.gameObject.AddComponent<UiFade>();
        }
    }

    // One-shot UI graphic: expand + fade, then destroy. Uses unscaled time so
    // touch feedback still animates if the game is paused mid-gesture.
    public class UiFade : MonoBehaviour
    {
        public float Life = 0.18f;      // done well under the 200 ms buffer window
        public float Grow = 2.4f;
        Graphic g;
        float t;
        RectTransform rt;
        void Awake() { g = GetComponent<Graphic>(); rt = GetComponent<RectTransform>(); }
        void Update()
        {
            t += Time.unscaledDeltaTime;
            float k = Mathf.Clamp01(t / Life);
            if (rt != null) rt.localScale = Vector3.one * Mathf.Lerp(0.6f, Grow, k);
            if (g != null) { var c = g.color; c.a = Mathf.Clamp01(1f - k); g.color = c; }
            if (t >= Life) Destroy(gameObject);
        }
    }
}
