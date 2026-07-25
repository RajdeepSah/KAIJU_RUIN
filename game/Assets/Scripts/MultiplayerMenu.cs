using UnityEngine;
using UnityEngine.UI;

namespace KaijuRuin
{
    // Online hub: Quick Match (find a worldwide opponent), Create Room (invite by
    // code), Join Room (enter a code). Records the match intent into MatchConfig,
    // then hands off to character select. Scalable: a new matchmaking option = one
    // more button + a MatchKind branch — no other screen changes (D-017).
    public class MultiplayerMenu : MonoBehaviour
    {
        Canvas canvas;
        GameObject joinDim, joinBorder;
        InputField codeField;
        Text joinNote;
        System.Action onProceed, onBack;

        public void Show(System.Action proceed, System.Action back)
        {
            onProceed = proceed; onBack = back;
            canvas = UiKit.NewCanvas("MultiplayerMenu", 22);

            var bg = UiKit.Image(canvas.transform, "Bg", AssetLib.SpriteOr("ui/menu_bg", "stages/harbor_sky") ?? UiKit.WhiteSprite());
            bg.preserveAspect = false;
            UiKit.Rect(bg.gameObject, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            bg.color = new Color(0.16f, 0.17f, 0.20f);

            var title = UiKit.Label(canvas.transform, "Title", "MULTIPLAYER", 72, AssetLib.DisplayFont, AssetLib.BonePaper);
            UiKit.Rect(title.gameObject, new Vector2(0f, 0.8f), new Vector2(1f, 0.94f), Vector2.zero, Vector2.zero);

            MakeButton("Quick", "QUICK MATCH", 0.60f, () => Begin(MatchKind.QuickMatch, NetRole.Client, ""));
            MakeButton("Create", "CREATE ROOM", 0.475f, () => Begin(MatchKind.PrivateRoom, NetRole.Host, ""));
            MakeButton("Join", "JOIN ROOM", 0.35f, ShowJoin);

            var sub = UiKit.Label(canvas.transform, "Sub",
                "Quick Match finds an opponent worldwide - Create or Join a room to fight a friend by code",
                24, AssetLib.HudFont, AssetLib.AshSteel);
            UiKit.Rect(sub.gameObject, new Vector2(0f, 0.27f), new Vector2(1f, 0.31f), Vector2.zero, Vector2.zero);

            var backBtn = UiKit.ButtonSprite(canvas.transform, "Back", AssetLib.UiSlice("plate_light"), "BACK", AssetLib.HudFont,
                () => { AudioManager.I?.Sfx("ui_tap"); Destroy(canvas.gameObject); onBack(); }, 38);
            UiKit.Rect(backBtn.gameObject, new Vector2(0.03f, 0.04f), new Vector2(0.16f, 0.12f), Vector2.zero, Vector2.zero);
        }

        void MakeButton(string name, string label, float yCenter, UnityEngine.Events.UnityAction onClick)
        {
            var b = UiKit.ButtonSprite(canvas.transform, name, AssetLib.UiSlice("plate_light"), label, AssetLib.HudFont, onClick, 44);
            UiKit.Rect(b.gameObject, new Vector2(0.36f, yCenter - 0.052f), new Vector2(0.64f, yCenter + 0.052f), Vector2.zero, Vector2.zero);
        }

        void Begin(MatchKind kind, NetRole role, string code)
        {
            AudioManager.I?.Sfx("ui_tap");
            MatchConfig.Mode = GameMode.Online;
            MatchConfig.Kind = kind;
            MatchConfig.Role = role;
            MatchConfig.RoomCode = code ?? "";
            MatchConfig.RemoteOpponent = false;
            Destroy(canvas.gameObject);
            onProceed();
        }

        // Room-code entry overlay for JOIN ROOM.
        void ShowJoin()
        {
            if (joinBorder != null) return;
            AudioManager.I?.Sfx("ui_tap");

            joinDim = UiKit.Image(canvas.transform, "JoinDim", UiKit.WhiteSprite(), new Color(0, 0, 0, 0.62f)).gameObject;
            joinDim.GetComponent<Image>().preserveAspect = false;
            joinDim.GetComponent<Image>().raycastTarget = true;
            UiKit.Rect(joinDim, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);

            var content = UiKit.InkPanel(canvas.transform, "JoinPanel", new Vector2(0.28f, 0.30f), new Vector2(0.72f, 0.70f));
            joinBorder = content.transform.parent.gameObject;   // the border wraps the content fill

            var head = UiKit.Label(content.transform, "Head", "ENTER ROOM CODE", 40, AssetLib.DisplayFont, AssetLib.BonePaper);
            UiKit.Rect(head.gameObject, new Vector2(0f, 0.74f), new Vector2(1f, 0.92f), Vector2.zero, Vector2.zero);

            codeField = UiKit.CodeInput(content.transform, "Code", "CODE", 6, AssetLib.HudFont);
            UiKit.Rect(codeField.gameObject, new Vector2(0.16f, 0.44f), new Vector2(0.84f, 0.66f), Vector2.zero, Vector2.zero);

            joinNote = UiKit.Label(content.transform, "Note", "6-character code from your friend", 22, AssetLib.HudFont, AssetLib.AshSteel);
            UiKit.Rect(joinNote.gameObject, new Vector2(0f, 0.30f), new Vector2(1f, 0.40f), Vector2.zero, Vector2.zero);

            var connect = UiKit.ButtonSprite(content.transform, "Connect", AssetLib.UiSlice("plate_light"), "CONNECT", AssetLib.HudFont, () =>
            {
                string code = (codeField.text ?? "").Trim().ToUpperInvariant();
                if (code.Length < 4) { joinNote.text = "Enter a valid room code"; joinNote.color = AssetLib.BloodSeal; return; }
                Begin(MatchKind.PrivateRoom, NetRole.Client, code);
            }, 38);
            UiKit.Rect(connect.gameObject, new Vector2(0.54f, 0.08f), new Vector2(0.86f, 0.24f), Vector2.zero, Vector2.zero);

            var cancel = UiKit.ButtonSprite(content.transform, "Cancel", AssetLib.UiSlice("plate_light"), "CANCEL", AssetLib.HudFont, CloseJoin, 38);
            UiKit.Rect(cancel.gameObject, new Vector2(0.14f, 0.08f), new Vector2(0.46f, 0.24f), Vector2.zero, Vector2.zero);
        }

        void CloseJoin()
        {
            AudioManager.I?.Sfx("ui_tap");
            if (joinDim != null) Destroy(joinDim);
            if (joinBorder != null) Destroy(joinBorder);
            joinDim = null; joinBorder = null; codeField = null; joinNote = null;
        }
    }
}
