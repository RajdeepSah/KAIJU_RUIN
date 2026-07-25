using UnityEngine;

namespace KaijuRuin
{
    // Shared fighter state and movement. Deterministic sim on the X axis only;
    // rendering (GLB visual, clip + procedural animation) hangs off this and never
    // feeds back in. root.transform.position.x is the SOLE sim truth for range/hit.
    //
    // Session 5 (D-015) adds the state the expanded moveset needs: per-character
    // cadence (AttackSpeed), dash i-frames, parry timing, and an air-juggle counter.
    public class Fighter : MonoBehaviour
    {
        public const float MaxHp = 1000f;
        public const int MaxMeterSegments = 3;
        public const float MeterPerSegment = 150f;

        public string DisplayName;
        public float Hp = MaxHp;
        public float Meter;                 // 0..450, one segment per 150
        public float WalkSpeed = 3.0f;      // was 2.6 — quicker neutral for a faster fight (D-015)
        public bool Blocking;
        public bool FacingRight = true;
        public Fighter Opponent;
        public FighterAnimator Anim;
        public ProcAnim Proc;               // procedural per-move body motion (D-015)

        public float StunUntil;             // hit/knockdown stun (Time.time)
        public float AttackLockUntil;       // recovery after own attack
        public bool Airborne;
        public bool Dead;

        // Expanded-combat state (D-015)
        public float AttackSpeed = 1f;      // scales recovery (Kest faster, Tengi heavier)
        public Color Theme = Color.white;   // per-character accent tint for move VFX
        public string SpecialSet = "kest";  // which CombatSystem special set drives this fighter's cards (D-017)
        public string IconKey = "icon_kest";// UI ability-icon slice prefix for this fighter (D-017)
        public float BlockStartedAt = -99f;  // origin of the parry timing window
        public bool ParryArmed;             // this block press is a parry attempt
        public float InvulnUntil;           // dash i-frames
        public int JuggleCount;             // hits taken in the current air juggle

        // Dash (evasive back-hop): a short eased slide of the sim-truth root X.
        float dashFromX, dashToX, dashElapsed, dashDur;

        public bool Dashing => dashDur > 0f;
        public bool Invulnerable => Time.time < InvulnUntil;

        public int MeterSegments => Mathf.Min(MaxMeterSegments, (int)(Meter / MeterPerSegment));

        public bool CanAct => !Dead && Time.time >= StunUntil && Time.time >= AttackLockUntil
                              && !RoundManager.RoundFrozen && !CombatFx.Frozen && !GameManager.Paused
                              && !Dashing;

        public float DistanceTo(Fighter other) => Mathf.Abs(other.transform.position.x - transform.position.x);

        void Update()
        {
            // Dash slide: advance only in live combat so hit-stop/pause freeze it.
            if (dashDur > 0f)
            {
                if (!CombatFx.Frozen && !RoundManager.RoundFrozen && !GameManager.Paused)
                    dashElapsed += Time.deltaTime;
                float k = Mathf.Clamp01(dashElapsed / dashDur);
                var p = transform.position;
                p.x = Mathf.Clamp(Mathf.Lerp(dashFromX, dashToX, k * (2f - k)), -6f, 6f);   // ease-out
                transform.position = p;
                if (k >= 1f) dashDur = 0f;
            }

            // The air-juggle window closes when the launch stun ends; then reset it.
            if (Airborne && !Dead && Time.time >= StunUntil)
            {
                Airborne = false;
                JuggleCount = 0;
            }
        }

        public void Dash(float dx, float dur, float invuln)
        {
            dashFromX = transform.position.x;
            dashToX = Mathf.Clamp(transform.position.x + dx, -6f, 6f);
            dashElapsed = 0f;
            dashDur = Mathf.Max(0.01f, dur);
            InvulnUntil = Time.time + invuln;
        }

        public void MoveAxis(float axis)
        {
            if (Dead || Blocking || Dashing || Time.time < StunUntil
                || RoundManager.RoundFrozen || CombatFx.Frozen || GameManager.Paused) return;
            float mult = StageManager.I != null && StageManager.I.Flooded ? 0.9f : 1f;
            var p = transform.position;
            p.x = Mathf.Clamp(p.x + axis * WalkSpeed * mult * Time.deltaTime, -6f, 6f);
            transform.position = p;
            Anim?.SetLocomotion(Mathf.Abs(axis));
            if (StageManager.I != null && Mathf.Abs(axis) > 0.1f) StageManager.I.WadeSplash(transform.position);
        }

        public void Face(Fighter other)
        {
            if (other == null) return;
            bool right = other.transform.position.x > transform.position.x;
            if (right == FacingRight) return;
            FacingRight = right;
            var s = transform.localScale;
            s.x = Mathf.Abs(s.x) * (right ? 1f : -1f);
            transform.localScale = s;
        }

        public void GainMeter(float amount)
        {
            Meter = Mathf.Min(Meter + amount, MaxMeterSegments * MeterPerSegment);
        }

        public bool SpendSegments(int segments)
        {
            if (MeterSegments < segments) return false;
            Meter -= segments * MeterPerSegment;
            return true;
        }

        // A block press. Armed presses can perfect-guard within the parry window;
        // the AI blocks unarmed (chip only) except when it deliberately reads a parry.
        public void BeginBlock(bool armed)
        {
            Blocking = true;
            BlockStartedAt = Time.time;
            ParryArmed = armed;
        }

        public void EndBlock()
        {
            Blocking = false;
            ParryArmed = false;
        }

        public void ResetForRound(float x)
        {
            Hp = MaxHp;
            Blocking = false;
            ParryArmed = false;
            Airborne = false;
            JuggleCount = 0;
            Dead = false;
            StunUntil = 0f;
            AttackLockUntil = 0f;
            InvulnUntil = 0f;
            BlockStartedAt = -99f;
            dashDur = 0f;
            transform.position = new Vector3(x, 0f, 0f);
            Anim?.Play("idle", 0.1f);
        }
    }
}
