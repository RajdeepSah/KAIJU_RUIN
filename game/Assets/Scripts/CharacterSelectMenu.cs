using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace KaijuRuin
{
    // Pick YOUR fighter and the fighter you want to FIGHT. Both columns are built
    // straight from CharacterRoster.All, so adding a champion to the roster makes it
    // appear here with zero UI changes (D-017). Used by Solo and Online alike; for
    // Online Quick Match the opponent pick is a preference the matchmaker may override.
    public class CharacterSelectMenu : MonoBehaviour
    {
        Canvas canvas;
        System.Action<string, string> onConfirm;
        System.Action onBack;
        GameMode mode;

        int localIdx, oppIdx;
        readonly List<Image> localCards = new List<Image>();
        readonly List<Image> oppCards = new List<Image>();
        Image leftPreview, rightPreview;
        Text leftName, rightName;

        static readonly Color CardOff = new Color(0.10f, 0.11f, 0.13f, 0.92f);
        static readonly Color CardOnYou = new Color(0.14f, 0.34f, 0.29f, 0.98f);   // teal — you
        static readonly Color CardOnFoe = new Color(0.36f, 0.13f, 0.15f, 0.98f);   // crimson — foe

        public void Show(GameMode m, System.Action<string, string> confirm, System.Action back)
        {
            mode = m; onConfirm = confirm; onBack = back;
            localIdx = CharacterRoster.IndexOf(MatchConfig.LocalCharId);
            oppIdx = CharacterRoster.IndexOf(MatchConfig.OpponentCharId);
            if (oppIdx == localIdx) oppIdx = (localIdx + 1) % CharacterRoster.Count;

            canvas = UiKit.NewCanvas("CharacterSelect", 24);
            var bg = UiKit.Image(canvas.transform, "Bg", AssetLib.SpriteOr("ui/menu_bg", "stages/harbor_sky") ?? UiKit.WhiteSprite());
            bg.preserveAspect = false;
            UiKit.Rect(bg.gameObject, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            bg.color = new Color(0.14f, 0.15f, 0.18f);

            var title = UiKit.Label(canvas.transform, "Title", "SELECT FIGHTERS", 64, AssetLib.DisplayFont, AssetLib.BonePaper);
            UiKit.Rect(title.gameObject, new Vector2(0f, 0.9f), new Vector2(1f, 0.99f), Vector2.zero, Vector2.zero);

            var youHdr = UiKit.Label(canvas.transform, "YouHdr", "YOUR FIGHTER", 30, AssetLib.HudFont, AssetLib.GoryoFlame, TextAnchor.MiddleLeft);
            UiKit.Rect(youHdr.gameObject, new Vector2(0.04f, 0.82f), new Vector2(0.34f, 0.88f), Vector2.zero, Vector2.zero);
            var foeHdr = UiKit.Label(canvas.transform, "FoeHdr", "OPPONENT", 30, AssetLib.HudFont, AssetLib.BloodSeal, TextAnchor.MiddleRight);
            UiKit.Rect(foeHdr.gameObject, new Vector2(0.66f, 0.82f), new Vector2(0.96f, 0.88f), Vector2.zero, Vector2.zero);

            BuildColumn(side: 0, xMin: 0.04f, xMax: 0.34f, cards: localCards);
            BuildColumn(side: 1, xMin: 0.66f, xMax: 0.96f, cards: oppCards);

            // Middle VS preview.
            leftPreview = UiKit.Image(canvas.transform, "LeftPreview", null);
            UiKit.Rect(leftPreview.gameObject, new Vector2(0.36f, 0.34f), new Vector2(0.5f, 0.8f), Vector2.zero, Vector2.zero);
            rightPreview = UiKit.Image(canvas.transform, "RightPreview", null);
            UiKit.Rect(rightPreview.gameObject, new Vector2(0.5f, 0.34f), new Vector2(0.64f, 0.8f), Vector2.zero, Vector2.zero);
            rightPreview.rectTransform.localScale = new Vector3(-1f, 1f, 1f);   // face inward

            var vs = UiKit.Label(canvas.transform, "VS", "VS", 84, AssetLib.DisplayFont, AssetLib.BonePaper);
            UiKit.Rect(vs.gameObject, new Vector2(0.42f, 0.54f), new Vector2(0.58f, 0.72f), Vector2.zero, Vector2.zero);

            leftName = UiKit.Label(canvas.transform, "LeftName", "", 34, AssetLib.DisplayFont, AssetLib.GoryoFlame);
            UiKit.Rect(leftName.gameObject, new Vector2(0.34f, 0.28f), new Vector2(0.5f, 0.34f), Vector2.zero, Vector2.zero);
            rightName = UiKit.Label(canvas.transform, "RightName", "", 34, AssetLib.DisplayFont, AssetLib.BloodSeal);
            UiKit.Rect(rightName.gameObject, new Vector2(0.5f, 0.28f), new Vector2(0.66f, 0.34f), Vector2.zero, Vector2.zero);

            if (mode == GameMode.Online && MatchConfig.Kind == MatchKind.QuickMatch)
            {
                var note = UiKit.Label(canvas.transform, "MmNote",
                    "Quick Match: the online opponent's fighter is set by matchmaking",
                    22, AssetLib.HudFont, AssetLib.SignalAmber);
                UiKit.Rect(note.gameObject, new Vector2(0f, 0.2f), new Vector2(1f, 0.25f), Vector2.zero, Vector2.zero);
            }

            var confirmBtn = UiKit.ButtonSprite(canvas.transform, "Confirm", AssetLib.UiSlice("plate_light"),
                mode == GameMode.Online ? "FIND MATCH" : "FIGHT", AssetLib.HudFont, () =>
                {
                    AudioManager.I?.Sfx("ui_tap");
                    string local = CharacterRoster.All[localIdx].Id;
                    string opp = CharacterRoster.All[oppIdx].Id;
                    Destroy(canvas.gameObject);
                    onConfirm(local, opp);
                }, 46);
            UiKit.Rect(confirmBtn.gameObject, new Vector2(0.38f, 0.05f), new Vector2(0.62f, 0.15f), Vector2.zero, Vector2.zero);

            var backBtn = UiKit.ButtonSprite(canvas.transform, "Back", AssetLib.UiSlice("plate_light"), "BACK", AssetLib.HudFont,
                () => { AudioManager.I?.Sfx("ui_tap"); Destroy(canvas.gameObject); onBack(); }, 36);
            UiKit.Rect(backBtn.gameObject, new Vector2(0.03f, 0.04f), new Vector2(0.15f, 0.12f), Vector2.zero, Vector2.zero);

            Refresh();
        }

        void BuildColumn(int side, float xMin, float xMax, List<Image> cards)
        {
            const float top = 0.80f, rowH = 0.135f, gap = 0.02f;
            for (int i = 0; i < CharacterRoster.Count; i++)
            {
                var def = CharacterRoster.All[i];
                float y1 = top - i * (rowH + gap);
                float y0 = y1 - rowH;

                var card = UiKit.Image(canvas.transform, "Card" + side + "_" + i, AssetLib.SpriteOr("ui/roster_card", null) ?? UiKit.WhiteSprite(), CardOff);
                card.preserveAspect = false;
                card.raycastTarget = true;
                UiKit.Rect(card.gameObject, new Vector2(xMin, y0), new Vector2(xMax, y1), Vector2.zero, Vector2.zero);
                int idx = i;
                var btn = card.gameObject.AddComponent<Button>();
                btn.onClick.AddListener(() => { AudioManager.I?.Sfx("ui_tap"); Select(side, idx); });

                var port = UiKit.Image(card.transform, "P", AssetLib.Sprite(def.PortraitPath));
                port.raycastTarget = false;
                UiKit.Rect(port.gameObject, new Vector2(0.03f, 0.06f), new Vector2(0.34f, 0.94f), Vector2.zero, Vector2.zero);

                var nm = UiKit.Label(card.transform, "N", def.DisplayName, 30, AssetLib.HudFont, AssetLib.BonePaper, TextAnchor.LowerLeft);
                UiKit.Rect(nm.gameObject, new Vector2(0.38f, 0.45f), new Vector2(0.98f, 0.92f), Vector2.zero, Vector2.zero);
                var tg = UiKit.Label(card.transform, "T", def.Tagline, 17, AssetLib.HudFont, AssetLib.AshSteel, TextAnchor.UpperLeft);
                tg.horizontalOverflow = HorizontalWrapMode.Wrap;
                UiKit.Rect(tg.gameObject, new Vector2(0.38f, 0.08f), new Vector2(0.98f, 0.46f), Vector2.zero, Vector2.zero);

                cards.Add(card);
            }
        }

        void Select(int side, int idx)
        {
            if (side == 0) localIdx = idx; else oppIdx = idx;
            Refresh();
        }

        void Refresh()
        {
            for (int i = 0; i < localCards.Count; i++)
                localCards[i].color = i == localIdx ? CardOnYou : CardOff;
            for (int i = 0; i < oppCards.Count; i++)
                oppCards[i].color = i == oppIdx ? CardOnFoe : CardOff;

            var ld = CharacterRoster.All[localIdx];
            var rd = CharacterRoster.All[oppIdx];
            leftPreview.sprite = AssetLib.Sprite(ld.PortraitPath);
            rightPreview.sprite = AssetLib.Sprite(rd.PortraitPath);
            leftName.text = ld.DisplayName;
            rightName.text = rd.DisplayName;
        }
    }
}
