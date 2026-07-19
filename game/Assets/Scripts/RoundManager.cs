using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KaijuRuin
{
    // Best-of-3 match flow (D-013): VS splash, round banners, 60 s timer,
    // the Khulandra event between rounds 1 and 2, KO/TIME resolution, and the
    // Horrific Ending smash-cut on match point.
    public class RoundManager : MonoBehaviour
    {
        public static RoundManager I { get; private set; }
        public static bool RoundFrozen = true;

        public const float RoundSeconds = 60f;

        Fighter player;
        Fighter enemy;
        EnemyAI enemyAi;
        Camera cam;

        int round;                  // 1-based
        int playerRounds, enemyRounds;
        float roundEndsAt;
        bool roundOver;
        bool koBannerDone = true;
        bool matchOver;

        public float TimeLeft => Mathf.Max(0f, roundEndsAt - Time.time);

        void Awake() { I = this; }

        public IEnumerator RunMatch()
        {
            RoundFrozen = true;
            AudioManager.I?.Music("fight_harbor");

            cam = new GameObject("FightCamera", typeof(Camera)).GetComponent<Camera>();
            cam.transform.position = new Vector3(0f, 1.7f, -7.5f);
            cam.fieldOfView = 42f;
            cam.backgroundColor = AssetLib.SumiInk;
            cam.clearFlags = CameraClearFlags.SolidColor;

            var lightGo = new GameObject("KeyLight", typeof(Light));
            var light = lightGo.GetComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1.15f;
            light.color = new Color(1f, 0.88f, 0.75f);
            lightGo.transform.rotation = Quaternion.Euler(35f, 130f, 0f);   // dusk key from screen-left

            var stage = new GameObject("Stage").AddComponent<StageManager>();
            stage.Build(cam);

            // Fighters (rigged GLBs + clip GLBs generated on Kest's rig; Tengi
            // reuses the same skeleton's clips per D-011 slice rule).
            var clipFiles = new Dictionary<string, string> {
                { "idle", "kest_anim_idle.glb" }, { "walk", "kest_anim_walk.glb" },
                { "punch", "kest_anim_punch.glb" }, { "block", "kest_anim_block.glb" },
                { "hit", "kest_anim_hit.glb" }, { "death", "kest_anim_death.glb" },
            };

            var loadP = GltfCharacterLoader.LoadCharacter("kest_model.glb", clipFiles, new Vector3(-2.5f, 0f, 0f), true, null);
            while (!loadP.IsCompleted) yield return null;
            var kestGo = loadP.Result;

            var loadE = GltfCharacterLoader.LoadCharacter("tengi_model.glb", clipFiles, new Vector3(2.5f, 0f, 0f), false, null);
            while (!loadE.IsCompleted) yield return null;
            var tengiGo = loadE.Result;

            player = kestGo.AddComponent<Fighter>();
            player.DisplayName = "KEST";
            player.Anim = kestGo.GetComponent<FighterAnimator>();
            var pc = kestGo.AddComponent<PlayerController>();

            enemy = tengiGo.AddComponent<Fighter>();
            enemy.DisplayName = "TENGI";
            enemy.Anim = tengiGo.GetComponent<FighterAnimator>();
            enemy.FacingRight = false;
            enemyAi = tengiGo.AddComponent<EnemyAI>();

            player.Opponent = enemy;
            enemy.Opponent = player;

            var input = gameObject.AddComponent<TouchInput>();
            input.Player = pc;

            var ui = new GameObject("HUD").AddComponent<TouchUI>();
            ui.Build(player, enemy, pc);

            Prewarm();

            yield return VsSplash();

            round = 0; playerRounds = 0; enemyRounds = 0; matchOver = false;
            while (!matchOver)
            {
                round++;
                yield return StartRound();
                yield return RunRound();
                yield return ResolveRound();

                if (playerRounds == 2 || enemyRounds == 2)
                {
                    matchOver = true;
                }
                else if (round == 1)
                {
                    RoundFrozen = true;
                    // The signature living-stage beat gets its spec'd banner
                    // (DESIGN_BRIEF banners list) as the breach rises.
                    if (TouchUI.I != null) TouchUI.I.StartCoroutine(TouchUI.I.Banner("KHULANDRA RISES", 1.8f));
                    yield return StageManager.I.KhulandraEvent();
                }
            }

            bool playerWon = playerRounds > enemyRounds;
            yield return EndingPanel.Show(playerWon, playerRounds, enemyRounds);
        }

        IEnumerator VsSplash()
        {
            yield return TouchUI.I.ShowVsScreen(2.2f);
        }

        IEnumerator StartRound()
        {
            RoundFrozen = true;
            player.ResetForRound(-2.5f);
            enemy.ResetForRound(2.5f);
            if (enemyAi != null)
            {
                enemyAi.ReactionDelay = round == 1 ? 0.32f : round == 2 ? 0.26f : 0.20f;
                enemyAi.BlockRate = 0.45f + 0.10f * (round - 1);
            }
            TouchUI.I.SetRoundPips(playerRounds, enemyRounds);
            TouchUI.I.RefreshBars();
            TouchUI.I.SetTimer((int)RoundSeconds);   // show fresh time through the banners, not last round's value

            string banner = round == 1 ? "ROUND ONE" : round == 2 ? "ROUND TWO" : "FINAL ROUND";
            string vo = round == 1 ? "announcer_round_one" : round == 2 ? "announcer_round_two" : "announcer_final_round";
            AudioManager.I?.Announce(vo);
            yield return TouchUI.I.Banner(banner, 1.4f);
            AudioManager.I?.Announce("announcer_fight");
            yield return TouchUI.I.Banner("FIGHT", 0.7f);

            roundOver = false;
            roundEndsAt = Time.time + RoundSeconds;
            RoundFrozen = false;
        }

        IEnumerator RunRound()
        {
            while (!roundOver)
            {
                if (TimeLeft <= 0f)
                {
                    roundOver = true;
                    RoundFrozen = true;
                    AudioManager.I?.Sfx("ending_sting", 0.6f);
                    if (player.Hp >= enemy.Hp) playerRounds++; else enemyRounds++;
                    TouchUI.I.SetRoundPips(playerRounds, enemyRounds);
                    yield return TouchUI.I.Banner("TIME", 1.2f);
                    yield break;
                }
                TouchUI.I.SetTimer(Mathf.CeilToInt(TimeLeft));
                UpdateCamera();
                yield return null;
            }
        }

        public void OnKo(Fighter winner, Fighter loser)
        {
            if (roundOver) return;
            roundOver = true;
            // Tally synchronously so the match loop never reads a stale score;
            // the KO banner coroutine is presentation only.
            if (winner == player) playerRounds++; else enemyRounds++;
            TouchUI.I?.SetRoundPips(playerRounds, enemyRounds);
            koBannerDone = false;
            StartCoroutine(KoSequence());
        }

        IEnumerator KoSequence()
        {
            RoundFrozen = true;
            AudioManager.I?.Announce("announcer_ko");
            yield return new WaitForSeconds(0.4f);      // hit freeze beat
            yield return TouchUI.I.Banner("K.O.", 1.4f);
            koBannerDone = true;
        }

        IEnumerator ResolveRound()
        {
            while (!roundOver || !koBannerDone) yield return null;
            yield return new WaitForSeconds(0.6f);
        }

        // Load impact VFX sprites once up front so the first hit of the match
        // doesn't pay a synchronous Resources.Load on the frame it lands.
        void Prewarm()
        {
            foreach (var v in new[] { "hit_spark", "ink_blood", "kest_foxfire", "tengi_bladewave", "meter_flare", "kaiju_shockwave" })
                AssetLib.Sprite("vfx/" + v, 256f);
        }

        void UpdateCamera()
        {
            if (cam == null || player == null || enemy == null) return;
            float midX = (player.transform.position.x + enemy.transform.position.x) * 0.5f;
            float dist = Mathf.Abs(player.transform.position.x - enemy.transform.position.x);
            float z = Mathf.Lerp(-6.6f, -7.5f, Mathf.InverseLerp(2.5f, 7f, dist));   // 10% tighter when close
            var target = new Vector3(Mathf.Clamp(midX, -3.5f, 3.5f), 1.7f, z);
            cam.transform.position = Vector3.Lerp(cam.transform.position, target, Time.deltaTime * 5f);
        }
    }
}
