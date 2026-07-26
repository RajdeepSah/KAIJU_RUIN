using System.Collections;
using UnityEngine;

namespace KaijuRuin
{
    // The single owner of Time.timeScale (D-026). Two things used to write it —
    // GameManager.TogglePause (0/1) and nothing else — and hit-stop deliberately
    // avoided it for exactly that reason (CombatFx's header). Slow motion is a
    // third writer, so ownership moves here and pause routes through it: two
    // systems setting the same global is how a game ends up paused at 0.35x.
    //
    // WHY timeScale rather than CombatFx's freeze-window pattern. Every deadline in
    // this fight is an absolute `Time.time` stamp (AttackLockUntil, StunUntil,
    // InvulnUntil, BlockStartedAt, chain/cancel/buffer windows, roundEndsAt) and
    // every movement integrates `Time.deltaTime`. Both are scaled time, so one
    // timeScale write dilates the recovery, the stun, the walk, the legacy clip on
    // the rig, the ProcAnim envelope and the VFX fade by the same factor, in step.
    // A hand-rolled slow-motion layer would have to touch all 35 scripts and would
    // still drift the moment one of them was missed -- and drift is precisely what
    // reads as fake.
    //
    // WHAT MAKES IT LOOK REAL. Four decisions, in order of how much they matter:
    //
    //  1. The ramps run on UNSCALED time. Easing out of slow motion on scaled time
    //     is the classic bug: the exit is itself slowed, so a 0.4 s release takes
    //     over a second and the fight feels like it is wading back.
    //  2. The interpolation is LOGARITHMIC. Time scale is perceived multiplicatively
    //     (1.0 -> 0.5 is the same felt step as 0.5 -> 0.25), so what the eye tracks is
    //     the frame-to-frame RATIO of the scale, not its difference. Modelled over the
    //     deepest shot's ease-in at 60 fps, that ratio under log interpolation is a
    //     symmetric arc peaking in the middle of the ramp, exactly where smootherstep
    //     intends it; under linear interpolation the same curve's perceptual peak
    //     drifts to ~75% of the way through and then kinks off. Similar magnitude,
    //     wrong shape -- the linear version slows hardest just before it stops slowing.
    //  3. The envelope is ASYMMETRIC IN DURATION, not in curve shape: ~0.15 s in,
    //     2.5-3.5x longer out. Impact grabs time; time is released. Both ramps use
    //     smootherstep, whose first derivative is zero at BOTH ends, so where a ramp
    //     meets a flat stretch (real time before, the hold after) there is no kink in
    //     the rate of change. The first attempt front-loaded the entry with an
    //     ease-out cubic on the theory that impact should feel abrupt; modelled at the
    //     60 fps cap that put a 0.17 scale jump in the FIRST frame and spent the
    //     other four crawling -- a cut wearing a curve's name. The bite belongs in the
    //     hit-stop that precedes the ramp, which is already a hard freeze; adding a
    //     second discontinuity behind it is what reads as a dropped frame. No ease-in
    //     is shorter than ~9 frames, because no curve is smooth across 5.
    //  4. It starts AFTER the hit-stop bite, not instead of it. The lead phase simply
    //     waits for CombatFx to unfreeze, so the beat is: impact -> freeze -> time
    //     pours out -> hold -> return. The two systems never fight over the frame.
    //
    // Audio is part of the illusion, not a garnish: silent slow motion reads as a
    // dropped frame. AudioManager.SetTimeStretch drags the SFX pitch with the scale
    // and dips the music, and is driven from here every frame.
    //
    // WHAT this does NOT do: pick when to fire. That is Cinematics.cs, deliberately
    // separate -- this file is "how a shot looks", that one is "which hits earn one".
    [DefaultExecutionOrder(-100)]
    public class TimeDirector : MonoBehaviour
    {
        public static TimeDirector I { get; private set; }

        // One cinematic slow-motion move. Every duration is UNSCALED seconds, so a
        // shot's wall-clock length is what it says regardless of how deep it goes.
        public readonly struct Shot
        {
            public readonly string Name;
            public readonly int Priority;   // a stronger shot may interrupt a weaker one
            public readonly float Scale;    // time scale at the hold
            public readonly float EaseIn;
            public readonly float Hold;
            public readonly float EaseOut;
            public readonly float Dolly;    // metres of extra camera push-in at full depth

            public Shot(string name, int priority, float scale, float easeIn, float hold, float easeOut, float dolly)
            {
                Name = name; Priority = priority;
                Scale = Mathf.Clamp(scale, 0.05f, 1f);
                EaseIn = easeIn; Hold = hold; EaseOut = easeOut; Dolly = dolly;
            }

            public float Wall => EaseIn + Hold + EaseOut;
            public bool Valid => Name != null;
        }

        // ---- Shot table -----------------------------------------------------
        // Tuned as a set, not individually: each tier must be visibly deeper and
        // longer than the one below it, or the game has one slow-motion effect that
        // fires at four different times instead of a hierarchy of moments.
        //
        // Ease-in lengths are floored at ~9 frames of the 60 fps cap (Bootstrap) —
        // below that the ramp has too few samples to be a ramp, however good the curve.
        //
        //   name          pri  scale  in     hold   out    dolly    wall-clock
        public static readonly Shot Critical = new Shot("critical", 1, 0.32f, 0.14f, 0.16f, 0.34f, 0.35f);  // 0.64 s
        public static readonly Shot Super    = new Shot("super",    2, 0.26f, 0.16f, 0.26f, 0.42f, 0.55f);  // 0.84 s
        public static readonly Shot Ko       = new Shot("ko",       3, 0.20f, 0.18f, 0.34f, 0.52f, 0.70f);  // 1.04 s
        public static readonly Shot MatchKo  = new Shot("match-ko", 4, 0.15f, 0.20f, 0.55f, 0.70f, 0.90f);  // 1.45 s

        // Master switch: the owner's preference (F3 / PlayerPrefs) AND the online
        // lockout both feed it. Off means Play() is a no-op -- never a half-applied
        // scale.
        public static bool Enabled = true;
        const string PrefKey = "kr.slowmo";

        // How long the lead phase will wait for hit-stop to release before starting
        // anyway. A safety net: if a freeze is ever left dangling the cinematic must
        // still happen, just without the bite in front of it.
        const float LeadCap = 0.25f;

        enum Phase { Idle, Lead, In, Hold, Out }

        static Phase phase = Phase.Idle;
        static Shot shot;
        static float t;            // unscaled seconds inside the current phase
        static float fromScale = 1f;
        static float scale = 1f;   // the slow-motion scale alone; pause is applied on top
        static float weight;       // 0..1 depth of the shot, for the dolly and any grade
        static bool paused;

        /// The live slow-motion factor (1 = real time). Pause is NOT folded in.
        public static float Scale => scale;

        /// 0..1 how deep the current shot is. Presentation hook: the camera dolly
        /// reads it, and a colour grade could without touching this file.
        public static float Weight => weight;

        public static bool Active => phase != Phase.Idle;

        /// Extra camera push-in, in metres, for RoundManager's fight camera. Rides
        /// the unscaled envelope, so the move keeps its own pace while the world slows.
        public static float DollyZ => phase == Phase.Idle ? 0f : weight * shot.Dolly;

        void Awake()
        {
            I = this;
            Enabled = PlayerPrefs.GetInt(PrefKey, 1) == 1;
            Abort();
        }

        void OnDestroy() { if (I == this) I = null; }

        /// Request a cinematic. A shot of equal or higher priority takes over from
        /// whatever is running, blending out of the CURRENT scale rather than
        /// snapping back to 1 first (a KO landing during a critical-hit shot must
        /// deepen the moment, not restart it). A weaker request is dropped.
        public static void Play(in Shot s)
        {
            if (!Enabled || I == null || !s.Valid) return;
            if (phase != Phase.Idle && s.Priority < shot.Priority) return;
            shot = s;
            fromScale = scale;
            t = 0f;
            // Skip the lead if time is already dilated: the bite belongs in front of
            // the FIRST shot, not between two of them.
            phase = scale < 0.999f ? Phase.In : Phase.Lead;
            Apply();
        }

        /// Ease back to real time over `seconds` from wherever the scale is now.
        /// Used when something else needs the fight back at speed (round over).
        public static void Release(float seconds)
        {
            if (phase == Phase.Idle) return;
            shot = new Shot(shot.Name, shot.Priority, shot.Scale, shot.EaseIn, 0f, Mathf.Max(0.05f, seconds), shot.Dolly);
            fromScale = scale;
            t = 0f;
            phase = Phase.Out;
        }

        /// Drop any cinematic immediately (round reset, pause, disable). Snaps —
        /// only ever called at a moment the screen is already changing.
        public static void Abort()
        {
            phase = Phase.Idle;
            scale = 1f; fromScale = 1f; weight = 0f; t = 0f;
            Apply();
        }

        /// Pause takes precedence over every cinematic: GameManager routes its
        /// timeScale write here so the two can never disagree about the global.
        public static void SetPaused(bool value)
        {
            paused = value;
            if (value) Abort(); else Apply();
        }

        /// Full reset for fight teardown/startup.
        public static void HardReset()
        {
            paused = false;
            Abort();
        }

        void Update()
        {
            // Dev/accessibility toggle, alongside F1 (PerfMonitor) and F2 (GroundCues).
            if (Input.GetKeyDown(KeyCode.F3))
            {
                Enabled = !Enabled;
                PlayerPrefs.SetInt(PrefKey, Enabled ? 1 : 0);
                PlayerPrefs.Save();
                if (!Enabled) Abort();
            }

            if (phase == Phase.Idle || paused) return;

            float dt = Time.unscaledDeltaTime;
            t += dt;

            switch (phase)
            {
                case Phase.Lead:
                    // Let the hit-stop bite land first. timeScale is still 1 here, so
                    // CombatFx's freeze window expires on its own schedule and we pick
                    // up the instant it does -- freeze straight into slow motion, with
                    // no gap where the fight briefly runs at full speed.
                    scale = 1f; weight = 0f;
                    if (!CombatFx.Frozen || t >= LeadCap) { phase = Phase.In; t = 0f; fromScale = 1f; }
                    break;

                case Phase.In:
                    // Short, and eased at both ends so it neither snaps away from real
                    // time nor slams into the hold. Its speed relative to the exit is
                    // what carries "impact grabbed the moment".
                    Advance(Smootherstep(t / Mathf.Max(0.01f, shot.EaseIn)), fromScale, shot.Scale, Phase.Hold, shot.EaseIn);
                    break;

                case Phase.Hold:
                    scale = shot.Scale; weight = 1f;
                    if (t >= shot.Hold) { phase = Phase.Out; t = 0f; fromScale = scale; }
                    break;

                case Phase.Out:
                    // Long: time is released, never snapped back. Zero derivative at
                    // t=1 means the frame the fight reaches full speed is not the frame
                    // it accelerates hardest — the return has no detectable last step.
                    Advance(Smootherstep(t / Mathf.Max(0.01f, shot.EaseOut)), fromScale, 1f, Phase.Idle, shot.EaseOut);
                    break;
            }

            Apply();
        }

        // Drive one eased phase and hand over when it completes.
        void Advance(float eased, float from, float to, Phase next, float dur)
        {
            scale = LogLerp(from, to, eased);
            weight = Mathf.InverseLerp(1f, shot.Scale, scale);
            if (t < dur) return;
            scale = to;
            weight = next == Phase.Idle ? 0f : 1f;
            phase = next;
            t = 0f;
            fromScale = scale;
            if (next == Phase.Idle) { scale = 1f; weight = 0f; }
        }

        // Push the resolved scale at the two globals that consume it. Called from
        // every mutator as well as Update so the state can never be one frame stale.
        static void Apply()
        {
            float s = paused ? 0f : scale;
            Time.timeScale = s;
            // Audio is what sells it. Deliberately fed the slow-motion scale, not the
            // paused one: pause already aborts any shot, and pitching every source to
            // zero on pause would be a different (worse) effect.
            AudioManager.I?.SetTimeStretch(scale);
        }

        // ---- Curves ----------------------------------------------------------

        // Perceptually even ramp: equal steps in log(scale) feel like equal steps of
        // slowing. Linear interpolation of the scale itself spends most of a ramp
        // near real time and then falls off a cliff.
        static float LogLerp(float a, float b, float e)
            => Mathf.Exp(Mathf.Lerp(Mathf.Log(Mathf.Max(0.01f, a)), Mathf.Log(Mathf.Max(0.01f, b)), Mathf.Clamp01(e)));

        // C1-continuous at both ends (zero first derivative at 0 and 1), which is the
        // property that matters: a ramp meeting a flat stretch with a non-zero
        // derivative is a kink, and a kink in the rate of time is exactly what the eye
        // reads as a dropped frame.
        static float Smootherstep(float x)
        {
            x = Mathf.Clamp01(x);
            return x * x * x * (x * (x * 6f - 15f) + 10f);
        }

        // ---- Coroutine helper -------------------------------------------------

        /// Wait, in REAL time, for at least `minSeconds` and until any running shot
        /// has released. Sequencing coroutines (the K.O. beat) must use this rather
        /// than WaitForSeconds: a scaled wait is stretched by the very cinematic it
        /// is waiting on -- the KO's 0.4 s beat would run ~2.7 s at the match-point
        /// shot's 0.15x, and the banner would arrive long after the moment had passed.
        public static IEnumerator WaitForRelease(float minSeconds)
        {
            const float cap = 4f;               // never hang a round on a stuck shot
            float t0 = 0f;
            while (t0 < cap && (t0 < minSeconds || Active))
            {
                t0 += Time.unscaledDeltaTime;
                yield return null;
            }
        }
    }
}
