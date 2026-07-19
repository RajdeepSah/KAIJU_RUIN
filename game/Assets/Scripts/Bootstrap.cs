using UnityEngine;

namespace KaijuRuin
{
    // Entry point. The Boot scene contains only this component; everything else
    // is constructed in code so the project has no fragile hand-authored assets.
    public class Bootstrap : MonoBehaviour
    {
        void Awake()
        {
            // Drive frame pacing from targetFrameRate, not vSync, so the 60 fps
            // cap is honoured cleanly. Held at 60 (not the panel refresh) to keep
            // the Snapdragon-695 thermal budget of Pillar 7 -- uncapping to 90/120
            // would cook a mid-range device over a 30-minute session.
            QualitySettings.vSyncCount = 0;
            Application.targetFrameRate = 60;
            Screen.orientation = ScreenOrientation.AutoRotation;
            Screen.autorotateToLandscapeLeft = true;
            Screen.autorotateToLandscapeRight = true;
            Screen.autorotateToPortrait = false;
            Screen.autorotateToPortraitUpsideDown = false;

            var game = new GameObject("Game");
            DontDestroyOnLoad(game);
            game.AddComponent<AudioListener>();   // single persistent listener (fight camera adds none)
            game.AddComponent<AudioManager>();
            game.AddComponent<PerfMonitor>();     // dev-only fps + input-to-impact overlay (F1)
            game.AddComponent<GameManager>();
        }
    }
}
