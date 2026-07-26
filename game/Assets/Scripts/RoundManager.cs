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
        GroundCues cues;
        Camera cam;
        Vector3 camBase;        // pre-shake camera target (shake/punch add on top)

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
            camBase = cam.transform.position;

            var lightGo = new GameObject("KeyLight", typeof(Light));
            var light = lightGo.GetComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1.15f;
            light.color = new Color(1f, 0.88f, 0.75f);
            lightGo.transform.rotation = Quaternion.Euler(35f, 130f, 0f);   // dusk key from screen-left

            var stage = new GameObject("Stage").AddComponent<StageManager>();
            stage.Build(cam);

            // Fighters are data-driven from the negotiated match (MatchConfig ->
            // CharacterRoster, D-017). Champions share Kest's rig clips per the D-011
            // slice rule via AnimClipPrefix. Parented under the fight root so they die
            // with it on cleanup (survives mirror matches with duplicate model names).
            var localDef = MatchConfig.Local;
            var oppDef = MatchConfig.Opponent;

            var loadP = GltfCharacterLoader.LoadCharacter(localDef.ModelGlb, ClipFiles(localDef), new Vector3(-2.5f, 0f, 0f), true, transform);
            while (!loadP.IsCompleted) yield return null;
            var playerGo = loadP.Result;

            var loadE = GltfCharacterLoader.LoadCharacter(oppDef.ModelGlb, ClipFiles(oppDef), new Vector3(2.5f, 0f, 0f), false, transform);
            while (!loadE.IsCompleted) yield return null;
            var oppGo = loadE.Result;

            player = ConfigureFighter(playerGo, localDef, faceRight: true);
            var pc = playerGo.AddComponent<PlayerController>();

            enemy = ConfigureFighter(oppGo, oppDef, faceRight: false);

            player.Opponent = enemy;
            enemy.Opponent = player;

            // Slow motion dilates the LOCAL clock, which is an effect in a solo or
            // loopback match and a desync in a lockstep one — there is no way to hold
            // two peers' clocks together through an unsynchronised time ramp. Gated on
            // the backend rather than on "is a transport open", because loopback is a
            // local AI wearing a transport and has no second clock to disagree with
            // (D-017/D-026). Wire this off in RelayTransport's peer handshake if the
            // live path ever wants shared cinematics.
            Cinematics.OnlineLockout = NetService.I != null && NetService.I.Active == NetService.Backend.Relay;

            // Opponent control: a live remote peer if a real transport is connected,
            // otherwise the AI (solo + the shipping loopback online path, D-017).
            if (MatchConfig.RemoteOpponent && NetService.I != null && NetService.I.Transport != null)
            {
                var oc = oppGo.AddComponent<PlayerController>();
                oc.Local = false;                          // remote inputs never touch the local HUD
                var rc = oppGo.AddComponent<RemoteController>();
                rc.Control = oc;
                rc.Bind(NetService.I.Transport);
                // Local send seam (symmetric with RemoteController): local input
                // capture -> relay.Push is completed alongside the RelayTransport backend (D-017).
                playerGo.AddComponent<NetInputRelay>().Bind(NetService.I.Transport);
            }
            else
            {
                enemyAi = oppGo.AddComponent<EnemyAI>();
            }

            var input = gameObject.AddComponent<TouchInput>();
            input.Player = pc;

            var ui = new GameObject("HUD").AddComponent<TouchUI>();
            ui.Build(player, enemy, pc);

            // Ground shadows + the local player's reach guide (D-023). Parented to
            // the fight root so it tears down with the match.
            var cuesGo = new GameObject("GroundCues");
            cuesGo.transform.SetParent(transform, false);
            cues = cuesGo.AddComponent<GroundCues>();
            cues.Build(player, enemy);

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
            CombatFx.Reset();                 // no freeze/shake leaks across rounds
            TimeDirector.Abort();             // ...and no cinematic still ramping into round 2
            Cinematics.ResetRound();          // budget + cooldown are per round (D-026)
            player.ResetForRound(-2.5f);
            enemy.ResetForRound(2.5f);
            if (enemyAi != null)
            {
                enemyAi.Round = round;
                enemyAi.ReactionDelay = round == 1 ? 0.32f : round == 2 ? 0.26f : 0.20f;
                enemyAi.BlockRate = 0.45f + 0.10f * (round - 1);
                enemyAi.ParryChance = round >= 2 ? 0.18f + 0.10f * (round - 2) : 0f;
                enemyAi.DashChance = 0.10f + 0.05f * (round - 1);
                // Fewer breathing gaps between pokes as the match escalates (D-023).
                enemyAi.IdleChance = round == 1 ? 0.22f : round == 2 ? 0.16f : 0.10f;
                // How hard he punishes a held guard, and how often he takes the crouch
                // stance himself (D-024). Round 1 lets a new player get away with
                // turtling; by the final round it costs them.
                enemyAi.MixupRate = round == 1 ? 0.45f : round == 2 ? 0.65f : 0.85f;
                enemyAi.CrouchGuardRate = 0.25f + 0.10f * (round - 1);
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
                    // A cinematic can still be ramping when the clock runs out (a
                    // critical hit landed a moment before TIME). Hand time back first:
                    // the banner below waits on SCALED seconds, so 1.2 s would run
                    // ~4 s if the round ended at 0.3x.
                    TimeDirector.Release(0.25f);
                    yield return TimeDirector.WaitForRelease(0f);
                    AudioManager.I?.Sfx("ending_sting", 0.6f);
                    if (player.Hp >= enemy.Hp) playerRounds++; else enemyRounds++;
                    TouchUI.I.SetRoundPips(playerRounds, enemyRounds);
                    yield return TouchUI.I.Banner("TIME", 1.2f);
                    yield break;
                }
                TouchUI.I.SetTimer(Mathf.CeilToInt(TimeLeft));
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
            // Fired here rather than from CombatSystem because this is the only place
            // that knows whether the blow took the MATCH — which earns the deeper,
            // longer shot (D-026). Read after the tally, so 2 rounds means match point.
            Cinematics.OnKo(playerRounds == 2 || enemyRounds == 2);
            TouchUI.I?.SetRoundPips(playerRounds, enemyRounds);
            koBannerDone = false;
            StartCoroutine(KoSequence());
        }

        IEnumerator KoSequence()
        {
            RoundFrozen = true;
            AudioManager.I?.Announce("announcer_ko");
            // REAL time, and until the cinematic has released. A WaitForSeconds here
            // is stretched by the very slow motion it is waiting on — the 0.4 s beat
            // would run ~2.7 s at the match-point shot's 0.15x, and the K.O. banner
            // (itself a scaled wait) would then sit on screen for nine seconds.
            yield return TimeDirector.WaitForRelease(0.4f);
            yield return TouchUI.I.Banner("K.O.", 1.4f);
            koBannerDone = true;
        }

        IEnumerator ResolveRound()
        {
            while (!roundOver || !koBannerDone) yield return null;
            yield return new WaitForSeconds(0.6f);
        }

        // Clip-GLB set for a character (all share Kest's rig per D-011; AnimClipPrefix
        // lets a future champion ship its own clips without touching this code).
        static Dictionary<string, string> ClipFiles(CharacterDef def)
        {
            string p = def.AnimClipPrefix;
            return new Dictionary<string, string> {
                { "idle", p + "idle.glb" }, { "walk", p + "walk.glb" },
                { "punch", p + "punch.glb" }, { "block", p + "block.glb" },
                { "hit", p + "hit.glb" }, { "death", p + "death.glb" },

                // Per-move clips for the D-024 moveset (generated session 10 on the
                // same Kest rig, seed 20260718). One clip per move is the point: the
                // low/overhead mix-up is only fair if the wind-ups look different.
                // Any of these can be dropped by commenting the line out —
                // FighterAnimator falls back to `idle`, so nothing breaks; and
                // several clips carry root translation (the roundhouse, the sweeps,
                // the grab step), which shows up as the model sliding within its own
                // silhouette and never as sim movement.
                { "clawjab", p + "clawjab.glb" }, { "clawcross", p + "clawcross.glb" },
                { "clawhook", p + "clawhook.glb" }, { "clawupper", p + "clawupper.glb" },
                { "clawslam", p + "clawslam.glb" }, { "haymaker", p + "haymaker.glb" },
                { "tailround", p + "tailround.glb" }, { "tailsweep", p + "tailsweep.glb" },
                { "legsweep", p + "legsweep.glb" }, { "haunchbash", p + "haunchbash.glb" },
                { "grab", p + "grab.glb" }, { "throw", p + "throw.glb" },
                { "crouchguard", p + "crouchguard.glb" },
                // Dedicated special-attack clip (generated session 8 — Meshy Charged_Spell_Cast,
                // D-011/D-015). Code already calls Anim.Play("special"); loading it here makes
                // the special animate on its own clip instead of Resolve() falling back to "punch".
                { "special", p + "special.glb" },
                // Also generated + synced this session and one-line-wireable, but left UNloaded
                // pending an on-device look (retarget + root-motion reads are unverifiable offline):
                //   { "airrake",  p + "airrake.glb"  },  then PlayerController/EnemyAI air-rake  -> Anim.Play("airrake")
                //   { "airslam",  p + "airslam.glb"  },  then air-slam  -> Anim.Play("airslam")
                //   { "parry",    p + "parry.glb"    },  then parry     -> Anim.Play("parry")   (now uses "block")
                //   { "backdash", p + "backdash.glb" },  then back-dash -> Anim.Play("backdash"); NB Back_Jump clip
                //     carries root translation, so confirm it doesn't double-move vs the sim dash before wiring.
            };
        }

        // Apply a CharacterDef's identity + feel knobs to a loaded fighter (D-017).
        static Fighter ConfigureFighter(GameObject go, CharacterDef def, bool faceRight)
        {
            var f = go.AddComponent<Fighter>();
            f.DisplayName = def.DisplayName;
            f.Anim = go.GetComponent<FighterAnimator>();
            f.Proc = go.AddComponent<ProcAnim>();
            f.Proc.AmpMul = def.ProcAmp;
            f.Proc.DurMul = def.ProcDur;
            f.AttackSpeed = def.AttackSpeed;
            f.WalkSpeed = def.WalkSpeed;
            f.Theme = def.Theme;
            f.SpecialSet = def.SpecialSet;
            f.IconKey = def.IconKey;
            f.FacingRight = faceRight;

            // Body metrics measured off the rigged GLB (D-023). Every distance the
            // fight reads — hit reach, push-out, AI spacing, the ground cues —
            // resolves through these, so a champion's silhouette and its ranges
            // cannot drift apart.
            f.ArmReach = def.ArmReach;
            f.HurtDepth = def.HurtDepth;
            f.PushDepth = def.PushDepth;
            f.ChestY = def.ChestY;
            f.ModelHeight = def.ModelHeight;
            f.Proc.AirLift = def.ModelHeight * 0.30f;
            return f;
        }

        // Load impact VFX sprites once up front so the first hit of the match
        // doesn't pay a synchronous Resources.Load on the frame it lands.
        void Prewarm()
        {
            foreach (var v in new[] { "hit_spark", "ink_blood", "kest_foxfire", "tengi_bladewave", "meter_flare",
                                      "kaiju_shockwave", "dash_streak", "parry_spark", "impact_ring" })
                if (AssetLib.Has("vfx/" + v)) AssetLib.Sprite("vfx/" + v, 256f);
        }

        // One deterministic post-movement chain, in LateUpdate so every controller's
        // Update has already run: separate the bodies, then let the camera and the
        // ground cues read positions that are final for the frame. Doing this in
        // LateUpdate (rather than each component minding itself) is what guarantees
        // the gap the player sees is the gap the hit check read.
        //
        // Camera runs every frame (not just in RunRound) so hit-stop shake and the
        // KO punch still read during banners/freeze. Paused holds it entirely.
        void LateUpdate()
        {
            if (GameManager.Paused) return;
            if (!CombatFx.Frozen && !RoundFrozen) CombatSystem.Separate(player, enemy);
            if (cam != null) UpdateCamera();
            cues?.Tick();
        }

        void UpdateCamera()
        {
            if (cam == null || player == null || enemy == null) return;
            float midX = (player.transform.position.x + enemy.transform.position.x) * 0.5f;
            float dist = Mathf.Abs(player.transform.position.x - enemy.transform.position.x);
            float z = Mathf.Lerp(-6.6f, -7.5f, Mathf.InverseLerp(2.5f, 7f, dist));   // 10% tighter when close
            var target = new Vector3(Mathf.Clamp(midX, -3.5f, 3.5f), 1.7f, z);
            // Framing tracks on SCALED time deliberately: the camera is part of the
            // world, so it should drift as slowly as the fighters during a cinematic.
            // The slow-motion dolly below is the exception — it rides the unscaled
            // envelope, because it is the shot moving, not the fight.
            camBase = Vector3.Lerp(camBase, target, Time.deltaTime * 5f);
            var pos = camBase + CombatFx.ShakeOffset();
            pos.z += CombatFx.PunchZ()      // dolly toward the action on heavy impacts
                   + TimeDirector.DollyZ;   // sustained push-in for the length of a cinematic (D-026)
            cam.transform.position = pos;
        }
    }
}
