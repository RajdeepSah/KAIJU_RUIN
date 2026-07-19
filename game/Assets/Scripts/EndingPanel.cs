using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace KaijuRuin
{
    // Horrific Ending smash-cut (Vision section 5): full-screen inked splash
    // panel with a caption, then results with REMATCH / TITLE.
    public static class EndingPanel
    {
        public static IEnumerator Show(bool playerWon, int playerRounds, int enemyRounds)
        {
            AudioManager.I?.Sfx("ending_sting");
            var canvas = UiKit.NewCanvas("Ending", 40);

            string panel = playerWon ? "panels/ending_kest_01" : "panels/ending_tengi_01";
            string caption = playerWon
                ? "The fox does not bury its dead. It multiplies them."
                : "The culling spares no one. Not even the brave.";

            var img = UiKit.Image(canvas.transform, "Panel", AssetLib.Sprite(panel));
            img.preserveAspect = false;
            UiKit.Rect(img.gameObject, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);

            var capBg = UiKit.Image(canvas.transform, "CapBg", UiKit.WhiteSprite(), new Color(0, 0, 0, 0.75f));
            capBg.preserveAspect = false;
            UiKit.Rect(capBg.gameObject, new Vector2(0f, 0f), new Vector2(1f, 0.13f), Vector2.zero, Vector2.zero);
            var cap = UiKit.Label(canvas.transform, "Caption", caption, 36, AssetLib.DisplayFont, AssetLib.BonePaper);
            UiKit.Rect(cap.gameObject, new Vector2(0.02f, 0f), new Vector2(0.98f, 0.13f), Vector2.zero, Vector2.zero);

            yield return new WaitForSeconds(3.2f);

            var dim = UiKit.Image(canvas.transform, "Dim", UiKit.WhiteSprite(), new Color(0, 0, 0, 0.7f));
            dim.preserveAspect = false;
            UiKit.Rect(dim.gameObject, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);

            var verdict = UiKit.Label(canvas.transform, "Verdict", playerWon ? "VICTORY" : "DEFEAT", 120,
                AssetLib.DisplayFont, playerWon ? AssetLib.GoryoFlame : AssetLib.BloodSeal);
            UiKit.Rect(verdict.gameObject, new Vector2(0f, 0.58f), new Vector2(1f, 0.8f), Vector2.zero, Vector2.zero);

            var score = UiKit.Label(canvas.transform, "Rounds", "Rounds: " + playerRounds + " - " + enemyRounds, 44,
                AssetLib.HudFont, AssetLib.BonePaper);
            UiKit.Rect(score.gameObject, new Vector2(0f, 0.48f), new Vector2(1f, 0.57f), Vector2.zero, Vector2.zero);

            bool done = false;
            bool rematch = false;
            var rematchBtn = UiKit.ButtonSprite(canvas.transform, "Rematch", AssetLib.UiSlice("plate_light"), "REMATCH", AssetLib.HudFont,
                () => { rematch = true; done = true; }, 46);
            UiKit.Rect(rematchBtn.gameObject, new Vector2(0.22f, 0.2f), new Vector2(0.46f, 0.36f), Vector2.zero, Vector2.zero);
            var titleBtn = UiKit.ButtonSprite(canvas.transform, "Title", AssetLib.UiSlice("plate_light"), "TITLE", AssetLib.HudFont,
                () => { rematch = false; done = true; }, 46);
            UiKit.Rect(titleBtn.gameObject, new Vector2(0.54f, 0.2f), new Vector2(0.78f, 0.36f), Vector2.zero, Vector2.zero);

            while (!done) yield return null;
            Object.Destroy(canvas.gameObject);

            if (rematch) GameManager.I.StartFight(skipIntro: true);
            else GameManager.I.BackToTitle();
        }
    }
}
