using UnityEngine;

namespace KaijuRuin
{
    // Dev-only on-screen instrumentation for the Vision Goal 2 bars:
    //   - 60 fps floor (mean + 1% low from an unscaled-time ring buffer)
    //   - input-to-impact under 80 ms
    //
    // The recognizer commit -> CombatSystem.Resolve path is synchronous, so the
    // IN-PROCESS gesture->impact delta is ~0 frames; this overlay proves that (no
    // inserted frames) and reports frame health. The touch-panel-scan and display
    // latency that make up most of the real 80 ms budget CANNOT be seen in-process
    // -- measure those on-device with a 240 fps slow-mo capture of finger+screen,
    // or adb Perfetto (input/gfx tags). Shown in dev builds; toggle with F1.
    public class PerfMonitor : MonoBehaviour
    {
        public static bool Show;

        const int Window = 180;              // ~3 s at 60 fps
        readonly float[] frames = new float[Window];
        int head, count;

        static float lastInputAt = -1f, lastImpactMs = -1f;
        static int inputFrame = -1, impactFrame = -1;
        static bool sameFrame;

        GUIStyle style;

        void Awake() { Show = Debug.isDebugBuild; }

        // Called by TouchInput the frame a gesture commits to an attack.
        public static void MarkInput()
        {
            lastInputAt = Time.realtimeSinceStartup;
            inputFrame = Time.frameCount;
        }

        // Called by CombatSystem.Resolve when damage/anim/vfx are applied.
        public static void MarkImpact()
        {
            if (lastInputAt < 0f) return;
            lastImpactMs = (Time.realtimeSinceStartup - lastInputAt) * 1000f;
            impactFrame = Time.frameCount;
            sameFrame = impactFrame == inputFrame;
            lastInputAt = -1f;
        }

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.F1)) Show = !Show;
            frames[head] = Time.unscaledDeltaTime;
            head = (head + 1) % Window;
            if (count < Window) count++;
        }

        void OnGUI()
        {
            if (!Show || count < 2) return;
            if (style == null)
                style = new GUIStyle(GUI.skin.label) { fontSize = 22, richText = true };

            float sum = 0f, worst = 0f;
            for (int i = 0; i < count; i++) { sum += frames[i]; if (frames[i] > worst) worst = frames[i]; }
            float meanFps = count / sum;
            float lowFps = worst > 0f ? 1f / worst : 0f;      // 1% low ~ slowest frame in window

            string fpsColor = meanFps >= 58f ? "#3FB08F" : meanFps >= 45f ? "#C88A3A" : "#A6212C";
            string latColor = lastImpactMs < 0f ? "#5A636E" : lastImpactMs <= 80f ? "#3FB08F" : "#A6212C";

            var r = new Rect(12, 12, 560, 96);
            GUI.color = new Color(0, 0, 0, 0.55f);
            GUI.DrawTexture(r, Texture2D.whiteTexture);
            GUI.color = Color.white;
            string lat = lastImpactMs < 0f ? "--" : $"{lastImpactMs:0.0} ms ({(sameFrame ? "same frame" : "+frames!")})";
            GUI.Label(new Rect(20, 16, 560, 30),
                $"<color={fpsColor}>fps {meanFps:0} mean / {lowFps:0} low</color>   target 60", style);
            GUI.Label(new Rect(20, 46, 560, 30),
                $"<color={latColor}>gesture->impact {lat}</color>   bar 80 ms", style);
            GUI.Label(new Rect(20, 74, 560, 24),
                "<color=#5A636E>in-process only; slow-mo/Perfetto for true latency  (F1)</color>",
                new GUIStyle(style) { fontSize = 15 });
        }
    }
}
