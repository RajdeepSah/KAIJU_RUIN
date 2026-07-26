using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace KaijuRuin
{
    // Fight HUD per DESIGN_BRIEF.md UI section. All strings literal.
    // Session 4 rework: dual segmented meters, ghost-drain health bars, chain
    // pips, block indicator, sub-80 ms touch feedback, card cost/deny feedback,
    // sliced glyphs, and an all-buttons gesture shield (PointerOverUi).
    public class TouchUI : MonoBehaviour
    {
        public static TouchUI I { get; private set; }

        Fighter player, enemy;
        PlayerController pc;

        Canvas canvas;
        Image playerHp, enemyHp;          // instant fills (truth)
        Image playerGhost, enemyGhost;    // lagging "chunk lost" fills
        float playerGhostV = 1f, enemyGhostV = 1f;
        readonly Image[] playerMeter = new Image[3];
        readonly Image[] enemyMeter = new Image[3];
        Text timerText;
        int timerSeconds = 60;
        Text bannerText;
        Image bannerDim;
        Image blockGlyph;
        readonly Image[] chainPips = new Image[3];
        float chainPipsUntil;
        readonly List<Image> playerPips = new List<Image>();
        readonly List<Image> enemyPips = new List<Image>();
        readonly List<Button> cardButtons = new List<Button>();
        readonly List<Image> cardIcons = new List<Image>();
        Image vsImage;
        GameObject pauseOverlay;

        // Session 5 (D-015): combo counter + parry cue (share one transient label).
        Text comboText;
        int comboCount;
        float cueUntil;
        Image parryFlash;
        float parryFlashV;
        float lastPlayerHp = Fighter.MaxHp;

        // Every interactive rect the gesture layer must NOT treat as an attack.
        readonly List<RectTransform> uiHitRects = new List<RectTransform>();

        public void Build(Fighter playerF, Fighter enemyF, PlayerController controller)
        {
            I = this;
            player = playerF; enemy = enemyF; pc = controller;
            canvas = UiKit.NewCanvas("FightHud", 10);
            var hud = AssetLib.HudFont;
            var display = AssetLib.DisplayFont;

            // Parry flash: a faint full-screen tint (added first so it sits behind
            // every other HUD element). Alpha driven from OnParry.
            parryFlash = UiKit.Image(canvas.transform, "ParryFlash", UiKit.WhiteSprite(), new Color(AssetLib.GoryoFlame.r, AssetLib.GoryoFlame.g, AssetLib.GoryoFlame.b, 0f));
            parryFlash.preserveAspect = false;
            parryFlash.raycastTarget = false;
            UiKit.Rect(parryFlash.gameObject, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);

            // Health bars (with ghost drain)
            HealthBar(true);
            HealthBar(false);
            var pName = UiKit.Label(canvas.transform, "PName", playerF.DisplayName, 34, hud, AssetLib.BonePaper, TextAnchor.MiddleLeft);
            UiKit.Rect(pName.gameObject, new Vector2(0.05f, 0.86f), new Vector2(0.3f, 0.90f), Vector2.zero, Vector2.zero);
            var eName = UiKit.Label(canvas.transform, "EName", enemyF.DisplayName, 34, hud, AssetLib.BonePaper, TextAnchor.MiddleRight);
            UiKit.Rect(eName.gameObject, new Vector2(0.7f, 0.86f), new Vector2(0.95f, 0.90f), Vector2.zero, Vector2.zero);

            // Timer
            timerText = UiKit.Label(canvas.transform, "Timer", "60", 64, display, AssetLib.BonePaper);
            UiKit.Rect(timerText.gameObject, new Vector2(0.46f, 0.9f), new Vector2(0.54f, 0.99f), Vector2.zero, Vector2.zero);

            // Round pips
            for (int i = 0; i < 2; i++)
            {
                playerPips.Add(Pip(0.40f + i * 0.025f));
                enemyPips.Add(Pip(0.575f - i * 0.025f));
            }

            // Player meter (bottom-left) — 3 discrete segment cells in Goryo Flame
            BuildMeter(playerMeter, new Vector2(0.03f, 0.045f), new Vector2(0.33f, 0.095f), AssetLib.GoryoFlame);
            var meterLabel = UiKit.Label(canvas.transform, "MeterLabel", "METER", 20, hud, AssetLib.AshSteel, TextAnchor.MiddleLeft);
            UiKit.Rect(meterLabel.gameObject, new Vector2(0.03f, 0.10f), new Vector2(0.2f, 0.13f), Vector2.zero, Vector2.zero);

            // Enemy meter (under enemy health bar, left of the right-aligned
            // name) — 3 cells in Blood Seal so the player can anticipate Tengi's
            // specials (they fire with no telegraph).
            BuildMeter(enemyMeter, new Vector2(0.555f, 0.893f), new Vector2(0.79f, 0.907f), AssetLib.BloodSeal);
            var eMeterLabel = UiKit.Label(canvas.transform, "EMeterLabel", enemyF.DisplayName + " METER", 16, hud, AssetLib.AshSteel, TextAnchor.MiddleLeft);
            UiKit.Rect(eMeterLabel.gameObject, new Vector2(0.555f, 0.876f), new Vector2(0.79f, 0.893f), Vector2.zero, Vector2.zero);

            // Chain step pips (1-2-3), above the player meter; light per landed hit.
            for (int i = 0; i < 3; i++)
            {
                var pip = UiKit.Image(canvas.transform, "ChainPip", UiKit.WhiteSprite(), new Color(1, 1, 1, 0.18f));
                pip.raycastTarget = false;
                UiKit.Rect(pip.gameObject, new Vector2(0.215f + i * 0.028f, 0.10f), new Vector2(0.235f + i * 0.028f, 0.128f), Vector2.zero, Vector2.zero);
                chainPips[i] = pip;
            }

            // Block indicator — a shield glyph by the player health bar.
            blockGlyph = UiKit.Image(canvas.transform, "BlockGlyph", AssetLib.UiSlice("shield_glyph"), AssetLib.AshSteel);
            blockGlyph.raycastTarget = false;
            UiKit.Rect(blockGlyph.gameObject, new Vector2(0.455f, 0.905f), new Vector2(0.49f, 0.965f), Vector2.zero, Vector2.zero);
            blockGlyph.gameObject.SetActive(false);

            // Ability cards (frame + Kest icon tile + segment-cost pips)
            for (int slot = 1; slot <= 3; slot++)
            {
                int s = slot;
                var min = new Vector2(0.70f + (slot - 1) * 0.098f, 0.03f);
                var max = new Vector2(0.79f + (slot - 1) * 0.098f, 0.22f);

                var frame = UiKit.Image(canvas.transform, "CardFrame" + slot, AssetLib.Sprite("ui/ability_card"));
                frame.raycastTarget = true;
                UiKit.Rect(frame.gameObject, min, max, Vector2.zero, Vector2.zero);
                var btn = frame.gameObject.AddComponent<Button>();
                btn.transition = Selectable.Transition.None;
                btn.onClick.AddListener(() => pc.CastSpecial(s));   // CastSpecial owns feedback

                var icon = UiKit.Image(frame.transform, "CardIcon" + slot, AssetLib.UiSlice(playerF.IconKey + "_" + slot));
                UiKit.Rect(icon.gameObject, new Vector2(0.16f, 0.18f), new Vector2(0.84f, 0.82f), Vector2.zero, Vector2.zero);
                icon.raycastTarget = false;
                cardIcons.Add(icon);

                // Cost pips: slot N costs N segments.
                for (int c = 0; c < slot; c++)
                {
                    var cost = UiKit.Image(frame.transform, "Cost", UiKit.WhiteSprite(), AssetLib.GoryoFlame);
                    cost.raycastTarget = false;
                    float cx = 0.20f + c * 0.20f;
                    UiKit.Rect(cost.gameObject, new Vector2(cx, 0.04f), new Vector2(cx + 0.13f, 0.11f), Vector2.zero, Vector2.zero);
                }

                cardButtons.Add(btn);
                uiHitRects.Add(frame.rectTransform);
            }

            // Pause (sliced pause glyph, >=48dp target)
            var pauseBtn = UiKit.ButtonSprite(canvas.transform, "Pause", AssetLib.UiSlice("pause_glyph"), null, hud,
                () => GameManager.I.TogglePause());
            pauseBtn.GetComponent<Image>().color = AssetLib.BonePaper;
            UiKit.Rect(pauseBtn.gameObject, new Vector2(0.925f, 0.86f), new Vector2(0.995f, 0.99f), Vector2.zero, Vector2.zero);
            uiHitRects.Add(pauseBtn.GetComponent<RectTransform>());

            // Banner (hidden by default)
            bannerDim = UiKit.Image(canvas.transform, "BannerDim", UiKit.WhiteSprite(), new Color(0, 0, 0, 0.45f));
            bannerDim.preserveAspect = false;
            bannerDim.raycastTarget = false;
            UiKit.Rect(bannerDim.gameObject, new Vector2(0f, 0.4f), new Vector2(1f, 0.6f), Vector2.zero, Vector2.zero);
            bannerText = UiKit.Label(canvas.transform, "Banner", "", 110, display, AssetLib.BonePaper);
            UiKit.Rect(bannerText.gameObject, new Vector2(0f, 0.4f), new Vector2(1f, 0.6f), Vector2.zero, Vector2.zero);
            bannerDim.gameObject.SetActive(false);

            // Combo / parry cue — center-upper, transient, never blocks touches.
            comboText = UiKit.Label(canvas.transform, "Combo", "", 56, display, AssetLib.SignalAmber);
            comboText.raycastTarget = false;
            UiKit.Rect(comboText.gameObject, new Vector2(0.3f, 0.62f), new Vector2(0.7f, 0.74f), Vector2.zero, Vector2.zero);
            SetCueAlpha(0f);

            RefreshBars();
        }

        void HealthBar(bool left)
        {
            var frame = UiKit.Image(canvas.transform, left ? "PHpFrame" : "EHpFrame", AssetLib.Sprite("ui/hud_healthbar"));
            frame.preserveAspect = false;
            frame.raycastTarget = false;
            UiKit.Rect(frame.gameObject, new Vector2(left ? 0.04f : 0.55f, 0.91f), new Vector2(left ? 0.45f : 0.96f, 0.97f), Vector2.zero, Vector2.zero);

            // Container holds bg(blood) + ghost(amber) + fill(bone), stacked.
            var container = new GameObject(left ? "PHp" : "EHp", typeof(Image));
            container.transform.SetParent(canvas.transform, false);
            var bg = container.GetComponent<Image>();
            bg.color = AssetLib.BloodSeal; bg.raycastTarget = false; bg.sprite = UiKit.WhiteSprite();
            UiKit.Rect(container, new Vector2(left ? 0.045f : 0.555f, 0.92f), new Vector2(left ? 0.445f : 0.955f, 0.96f), Vector2.zero, Vector2.zero);

            var ghost = MakeFill(container.transform, "Ghost", AssetLib.SignalAmber, left);
            var fill = MakeFill(container.transform, "Fill", AssetLib.BonePaper, left);

            if (left) { playerHp = fill; playerGhost = ghost; }
            else { enemyHp = fill; enemyGhost = ghost; }
        }

        Image MakeFill(Transform parent, string name, Color color, bool left)
        {
            var go = new GameObject(name, typeof(Image));
            go.transform.SetParent(parent, false);
            var img = go.GetComponent<Image>();
            img.color = color; img.raycastTarget = false;
            img.sprite = UiKit.WhiteSprite();
            img.type = Image.Type.Filled;
            img.fillMethod = Image.FillMethod.Horizontal;
            img.fillOrigin = (int)(left ? Image.OriginHorizontal.Left : Image.OriginHorizontal.Right);
            UiKit.Rect(go, Vector2.zero, Vector2.one, new Vector2(3, 3), new Vector2(-3, -3));
            return img;
        }

        void BuildMeter(Image[] cells, Vector2 min, Vector2 max, Color fillColor)
        {
            float span = max.x - min.x;
            float gap = span * 0.03f;
            float cellW = (span - gap * 2f) / 3f;
            for (int i = 0; i < 3; i++)
            {
                float x0 = min.x + i * (cellW + gap);
                var frame = UiKit.Image(canvas.transform, "MeterCell", UiKit.WhiteSprite(), new Color(0, 0, 0, 0.6f));
                frame.preserveAspect = false; frame.raycastTarget = false;
                UiKit.Rect(frame.gameObject, new Vector2(x0, min.y), new Vector2(x0 + cellW, max.y), Vector2.zero, Vector2.zero);
                var fill = UiKit.Image(frame.transform, "Fill", UiKit.WhiteSprite(), fillColor);
                fill.preserveAspect = false; fill.raycastTarget = false;
                fill.type = Image.Type.Filled;
                fill.fillMethod = Image.FillMethod.Horizontal;
                UiKit.Rect(fill.gameObject, Vector2.zero, Vector2.one, new Vector2(2, 2), new Vector2(-2, -2));
                cells[i] = fill;
            }
        }

        Image Pip(float x)
        {
            var pip = UiKit.Image(canvas.transform, "Pip", UiKit.WhiteSprite(), AssetLib.AshSteel);
            pip.raycastTarget = false;
            UiKit.Rect(pip.gameObject, new Vector2(x, 0.86f), new Vector2(x + 0.018f, 0.89f), Vector2.zero, Vector2.zero);
            return pip;
        }

        public void RefreshBars()
        {
            if (player == null || enemy == null) return;
            // Taking a clean hit breaks the player's combo streak.
            if (player.Hp < lastPlayerHp - 0.5f) ResetCombo();
            lastPlayerHp = player.Hp;
            float pHp = player.Hp / Fighter.MaxHp;
            float eHp = enemy.Hp / Fighter.MaxHp;
            if (playerHp != null) playerHp.fillAmount = pHp;
            if (enemyHp != null) enemyHp.fillAmount = eHp;
            // Ghost never rises above a heal-free bar; it only lags downward.
            if (pHp > playerGhostV) playerGhostV = pHp;
            if (eHp > enemyGhostV) enemyGhostV = eHp;

            float pSeg = player.Meter / Fighter.MeterPerSegment;
            float eSeg = enemy.Meter / Fighter.MeterPerSegment;
            for (int i = 0; i < 3; i++)
            {
                if (playerMeter[i] != null) playerMeter[i].fillAmount = Mathf.Clamp01(pSeg - i);
                if (enemyMeter[i] != null) enemyMeter[i].fillAmount = Mathf.Clamp01(eSeg - i);
            }
            for (int i = 0; i < cardButtons.Count; i++)
            {
                var img = cardButtons[i].GetComponent<Image>();
                bool affordable = player.MeterSegments >= i + 1;
                img.color = affordable ? Color.white : new Color(1, 1, 1, 0.4f);
                if (i < cardIcons.Count)
                    cardIcons[i].color = affordable ? Color.white : new Color(1, 1, 1, 0.5f);
            }
        }

        void Update()
        {
            // Ghost health drain: lag toward the true fill.
            if (playerGhost != null)
            {
                playerGhostV = Mathf.MoveTowards(playerGhostV, playerHp.fillAmount, Time.deltaTime * 0.6f);
                playerGhost.fillAmount = playerGhostV;
            }
            if (enemyGhost != null)
            {
                enemyGhostV = Mathf.MoveTowards(enemyGhostV, enemyHp.fillAmount, Time.deltaTime * 0.6f);
                enemyGhost.fillAmount = enemyGhostV;
            }

            // Timer urgency pulse under 10 s.
            if (timerText != null)
            {
                if (timerSeconds <= 10 && timerSeconds > 0)
                {
                    timerText.color = AssetLib.BloodSeal;
                    float p = 1f + 0.12f * Mathf.Abs(Mathf.Sin(Time.unscaledTime * 6f));
                    timerText.transform.localScale = Vector3.one * p;
                }
                else
                {
                    timerText.color = AssetLib.BonePaper;
                    timerText.transform.localScale = Vector3.one;
                }
            }

            // Chain pips fade after the 0.6 s window.
            if (Time.time > chainPipsUntil)
                for (int i = 0; i < 3; i++)
                    if (chainPips[i] != null)
                    {
                        var c = chainPips[i].color; c.a = Mathf.MoveTowards(c.a, 0.18f, Time.deltaTime * 2f); chainPips[i].color = c;
                    }

            // Combo / parry cue fade (snap in, ease out; clear text once faded).
            if (comboText != null)
            {
                float target = Time.time < cueUntil ? 1f : 0f;
                float a = Mathf.MoveTowards(comboText.color.a, target, Time.deltaTime * (target > 0f ? 8f : 3f));
                SetCueAlpha(a);
                // Once faded out, clear the label AND the streak — the combo counter
                // reflects a live streak, not lifetime hits.
                if (a <= 0.01f && Time.time >= cueUntil) { if (comboText.text.Length > 0) comboText.text = ""; comboCount = 0; }
            }

            // Parry screen flash decay.
            if (parryFlash != null)
            {
                parryFlashV = Mathf.MoveTowards(parryFlashV, 0f, Time.deltaTime * 1.8f);
                var pc = parryFlash.color; pc.a = parryFlashV; parryFlash.color = pc;
            }
        }

        public void SetTimer(int seconds)
        {
            timerSeconds = seconds;
            if (timerText != null) timerText.text = seconds.ToString();
        }

        // Which guard the player is holding (D-024). The stance must be visible: the
        // two stances stop opposite halves of the moveset, so a player who cannot
        // tell which one is up cannot read the mix-up. Standing sits at full height
        // in Goryō teal; crouch drops the glyph and tints it amber, matching the
        // sunken silhouette on the fighter.
        public void SetGuardIndicator(Fighter.GuardKind guard)
        {
            if (blockGlyph == null) return;
            bool on = guard != Fighter.GuardKind.None;
            blockGlyph.gameObject.SetActive(on);
            if (!on) return;
            bool crouch = guard == Fighter.GuardKind.Crouch;
            blockGlyph.color = crouch ? AssetLib.SignalAmber : AssetLib.GoryoFlame;
            UiKit.Rect(blockGlyph.gameObject,
                       new Vector2(0.455f, crouch ? 0.885f : 0.905f),
                       new Vector2(0.49f, crouch ? 0.935f : 0.965f),
                       Vector2.zero, Vector2.zero);
        }

        // landedStep: 1/2/3 when a chain hit connects, 0 to clear.
        public void SetChainStep(int landedStep)
        {
            chainPipsUntil = Time.time + 0.6f;
            for (int i = 0; i < 3; i++)
            {
                if (chainPips[i] == null) continue;
                chainPips[i].color = i < landedStep
                    ? AssetLib.GoryoFlame
                    : new Color(1, 1, 1, 0.18f);
            }
        }

        // A clean hit landed. Player hits build the combo counter; if the player
        // is the one struck, the combo drops (also handled in RefreshBars).
        public void OnHitLanded(Fighter attacker)
        {
            if (attacker == null) return;
            if (attacker == player)
            {
                comboCount++;
                if (comboCount >= 2) ShowCue(comboCount + " HITS", AssetLib.SignalAmber, 1.4f);
            }
        }

        // A perfect guard. The player's parry gets a screen flash + cue.
        public void OnParry(Fighter defender)
        {
            if (defender == player)
            {
                ShowCue("PARRY!", AssetLib.GoryoFlame, 0.8f);
                parryFlashV = 0.35f;   // brief, non-intrusive full-screen tint
            }
        }

        void ResetCombo() { comboCount = 0; }

        void ShowCue(string text, Color color, float seconds)
        {
            if (comboText == null) return;
            comboText.text = text;
            comboText.color = new Color(color.r, color.g, color.b, 1f);
            cueUntil = Time.time + seconds;
        }

        void SetCueAlpha(float a)
        {
            if (comboText == null) return;
            var c = comboText.color; c.a = a; comboText.color = c;
        }

        public void TouchFeedback(Vector2 screenPos, bool leftZone)
        {
            UiKit.Splash(canvas, screenPos, leftZone ? AssetLib.AshSteel : AssetLib.BonePaper, leftZone ? 70f : 90f);
        }

        // Called by PlayerController.CastSpecial with the actual outcome:
        // a distinct visual + audio answer so "cast" and "not enough meter" read
        // differently (they previously both played ui_tap).
        public void CardResult(int slot, bool cast)
        {
            int idx = slot - 1;
            if (idx < 0 || idx >= cardButtons.Count) return;
            var img = cardButtons[idx].GetComponent<Image>();
            StartCoroutine(CardFlash(img, cast));
            AudioManager.I?.Sfx(cast ? "ui_tap" : "block", cast ? 0.5f : 0.35f);
        }

        IEnumerator CardFlash(Image img, bool cast)
        {
            var flash = cast ? AssetLib.GoryoFlame : AssetLib.BloodSeal;
            float t = 0f;
            while (t < 0.16f)
            {
                t += Time.unscaledDeltaTime;
                img.color = Color.Lerp(flash, Color.white, t / 0.16f);
                yield return null;
            }
            RefreshBars();   // restores the true affordability tint
        }

        public void SetRoundPips(int playerRounds, int enemyRounds)
        {
            ResetCombo();
            lastPlayerHp = Fighter.MaxHp;
            for (int i = 0; i < 2; i++)
            {
                bool pLit = i < playerRounds, eLit = i < enemyRounds;
                playerPips[i].color = pLit ? AssetLib.GoryoFlame : AssetLib.AshSteel;
                enemyPips[i].color = eLit ? AssetLib.BloodSeal : AssetLib.AshSteel;
            }
        }

        public IEnumerator Banner(string text, float seconds)
        {
            bannerDim.gameObject.SetActive(true);
            bannerText.text = text;
            yield return new WaitForSeconds(seconds);
            bannerText.text = "";
            bannerDim.gameObject.SetActive(false);
        }

        public IEnumerator ShowVsScreen(float seconds)
        {
            vsImage = UiKit.Image(canvas.transform, "VsScreen", AssetLib.Sprite("ui/vs_screen"));
            vsImage.preserveAspect = false;
            UiKit.Rect(vsImage.gameObject, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            var pPortrait = UiKit.Image(vsImage.transform, "P", AssetLib.Sprite(MatchConfig.Local.PortraitPath));
            UiKit.Rect(pPortrait.gameObject, new Vector2(0.06f, 0.25f), new Vector2(0.42f, 0.85f), Vector2.zero, Vector2.zero);
            var ePortrait = UiKit.Image(vsImage.transform, "E", AssetLib.Sprite(MatchConfig.Opponent.PortraitPath));
            UiKit.Rect(ePortrait.gameObject, new Vector2(0.58f, 0.25f), new Vector2(0.94f, 0.85f), Vector2.zero, Vector2.zero);
            var vs = UiKit.Label(vsImage.transform, "VS", "VS", 160, AssetLib.DisplayFont, AssetLib.BonePaper);
            UiKit.Rect(vs.gameObject, new Vector2(0.4f, 0.35f), new Vector2(0.6f, 0.65f), Vector2.zero, Vector2.zero);
            yield return new WaitForSeconds(seconds);
            Destroy(vsImage.gameObject);
        }

        // True if the screen point is over ANY interactive HUD element (cards,
        // pause, or the active pause-overlay buttons) — the sole gesture shield.
        public bool PointerOverUi(Vector2 screenPos)
        {
            for (int i = uiHitRects.Count - 1; i >= 0; i--)
            {
                var rt = uiHitRects[i];
                if (rt == null) { uiHitRects.RemoveAt(i); continue; }
                if (rt.gameObject.activeInHierarchy &&
                    RectTransformUtility.RectangleContainsScreenPoint(rt, screenPos))
                    return true;
            }
            return false;
        }

        public void ShowPauseOverlay(bool show)
        {
            AudioManager.I?.Sfx("ui_tap", 0.5f);
            if (show && pauseOverlay == null)
            {
                pauseOverlay = new GameObject("PauseOverlay");
                pauseOverlay.transform.SetParent(canvas.transform, false);
                UiKit.Rect(pauseOverlay, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
                var dim = UiKit.Image(pauseOverlay.transform, "Dim", UiKit.WhiteSprite(), new Color(0, 0, 0, 0.75f));
                dim.preserveAspect = false;
                UiKit.Rect(dim.gameObject, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);

                var panel = UiKit.Image(pauseOverlay.transform, "Panel", AssetLib.UiSlice("panel_frame_tight"));
                panel.preserveAspect = false;
                UiKit.Rect(panel.gameObject, new Vector2(0.33f, 0.16f), new Vector2(0.67f, 0.84f), Vector2.zero, Vector2.zero);

                var t = UiKit.Label(pauseOverlay.transform, "T", "PAUSED", 90, AssetLib.DisplayFont, AssetLib.BonePaper);
                UiKit.Rect(t.gameObject, new Vector2(0f, 0.6f), new Vector2(1f, 0.78f), Vector2.zero, Vector2.zero);

                var resume = UiKit.ButtonSprite(pauseOverlay.transform, "Resume", AssetLib.UiSlice("plate_light"), "RESUME", AssetLib.HudFont,
                    () => GameManager.I.TogglePause(), 44);
                UiKit.Rect(resume.gameObject, new Vector2(0.36f, 0.40f), new Vector2(0.64f, 0.53f), Vector2.zero, Vector2.zero);
                uiHitRects.Add(resume.GetComponent<RectTransform>());

                var quit = UiKit.ButtonSprite(pauseOverlay.transform, "Quit", AssetLib.UiSlice("plate_light"), "QUIT TO TITLE", AssetLib.HudFont,
                    () => GameManager.I.BackToTitle(), 40);
                UiKit.Rect(quit.gameObject, new Vector2(0.36f, 0.22f), new Vector2(0.64f, 0.35f), Vector2.zero, Vector2.zero);
                uiHitRects.Add(quit.GetComponent<RectTransform>());
            }
            else if (!show && pauseOverlay != null)
            {
                Destroy(pauseOverlay);
                pauseOverlay = null;
            }
        }
    }
}
