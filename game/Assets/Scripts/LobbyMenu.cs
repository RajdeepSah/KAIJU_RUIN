using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace KaijuRuin
{
    // Runs the chosen matchmaking verb (create / join / quick) through NetService,
    // shows live status + the room code to share, and enables START once a session
    // is negotiated. Under the shipping loopback backend the opponent resolves to
    // the AI; the flow, codes, and status are all real (D-017).
    public class LobbyMenu : MonoBehaviour
    {
        Canvas canvas;
        System.Action onStart, onBack;
        Text statusLbl, codeLbl, foeLbl;
        Button startBtn;

        public void Show(System.Action start, System.Action back)
        {
            onStart = start; onBack = back;
            canvas = UiKit.NewCanvas("Lobby", 26);

            var bg = UiKit.Image(canvas.transform, "Bg", AssetLib.SpriteOr("ui/menu_bg", "stages/harbor_sky") ?? UiKit.WhiteSprite());
            bg.preserveAspect = false;
            UiKit.Rect(bg.gameObject, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            bg.color = new Color(0.13f, 0.14f, 0.17f);

            string heading = MatchConfig.Kind == MatchKind.QuickMatch ? "QUICK MATCH"
                           : MatchConfig.Role == NetRole.Host ? "CREATE ROOM" : "JOIN ROOM";
            var title = UiKit.Label(canvas.transform, "Title", heading, 64, AssetLib.DisplayFont, AssetLib.BonePaper);
            UiKit.Rect(title.gameObject, new Vector2(0f, 0.82f), new Vector2(1f, 0.95f), Vector2.zero, Vector2.zero);

            var youLbl = UiKit.Label(canvas.transform, "You", "You:  " + MatchConfig.Local.DisplayName, 36, AssetLib.HudFont, AssetLib.GoryoFlame);
            UiKit.Rect(youLbl.gameObject, new Vector2(0f, 0.68f), new Vector2(1f, 0.76f), Vector2.zero, Vector2.zero);

            foeLbl = UiKit.Label(canvas.transform, "Foe", "Opponent:  …", 36, AssetLib.HudFont, AssetLib.BloodSeal);
            UiKit.Rect(foeLbl.gameObject, new Vector2(0f, 0.60f), new Vector2(1f, 0.68f), Vector2.zero, Vector2.zero);

            codeLbl = UiKit.Label(canvas.transform, "Code", "", 44, AssetLib.DisplayFont, AssetLib.SignalAmber);
            UiKit.Rect(codeLbl.gameObject, new Vector2(0f, 0.48f), new Vector2(1f, 0.58f), Vector2.zero, Vector2.zero);

            statusLbl = UiKit.Label(canvas.transform, "Status", "Connecting…", 28, AssetLib.HudFont, AssetLib.BonePaper);
            UiKit.Rect(statusLbl.gameObject, new Vector2(0f, 0.36f), new Vector2(1f, 0.44f), Vector2.zero, Vector2.zero);

            startBtn = UiKit.ButtonSprite(canvas.transform, "Start", AssetLib.UiSlice("plate_light"), "START", AssetLib.HudFont, () =>
            {
                if (!startBtn.interactable) return;
                AudioManager.I?.Sfx("ui_tap");
                StopAllCoroutines();
                Destroy(canvas.gameObject);
                onStart();
            }, 46);
            UiKit.Rect(startBtn.gameObject, new Vector2(0.54f, 0.12f), new Vector2(0.78f, 0.24f), Vector2.zero, Vector2.zero);
            startBtn.interactable = false;
            startBtn.GetComponent<Image>().color = new Color(1f, 1f, 1f, 0.4f);

            var cancelBtn = UiKit.ButtonSprite(canvas.transform, "Cancel", AssetLib.UiSlice("plate_light"), "CANCEL", AssetLib.HudFont, () =>
            {
                AudioManager.I?.Sfx("ui_tap");
                StopAllCoroutines();
                Destroy(canvas.gameObject);
                onBack();
            }, 46);
            UiKit.Rect(cancelBtn.gameObject, new Vector2(0.22f, 0.12f), new Vector2(0.46f, 0.24f), Vector2.zero, Vector2.zero);

            StartCoroutine(Run());
        }

        IEnumerator Run()
        {
            var mm = NetService.I != null ? NetService.I.Matchmaker : new LocalMatchmaker();
            string local = MatchConfig.LocalCharId;

            System.Action<NetStatus, string> onStatus = (st, msg) => { if (statusLbl != null) statusLbl.text = msg; };
            NetSessionInfo result = null;
            System.Action<NetSessionInfo> onDone = s => result = s;

            IEnumerator verb;
            if (MatchConfig.Kind == MatchKind.QuickMatch)
                verb = mm.QuickMatch(local, onStatus, onDone);
            else if (MatchConfig.Role == NetRole.Host)
                verb = mm.CreateRoom(local, onStatus, onDone);
            else
                verb = mm.JoinRoom(MatchConfig.RoomCode, local, onStatus, onDone);

            yield return StartCoroutine(verb);

            if (result == null)
            {
                foeLbl.text = "No opponent found";
                yield break;   // BACK/CANCEL stays available
            }

            NetService.I?.SetSession(result);
            if (!string.IsNullOrEmpty(result.RoomCode))
                codeLbl.text = "ROOM CODE:  " + result.RoomCode;
            foeLbl.text = "Opponent:  " + MatchConfig.Opponent.DisplayName;
            statusLbl.text = "Ready — press START";
            startBtn.interactable = true;
            startBtn.GetComponent<Image>().color = Color.white;
        }
    }
}
