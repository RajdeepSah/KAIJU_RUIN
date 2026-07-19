using UnityEngine;

namespace KaijuRuin
{
    // Shared fighter state and movement. Deterministic sim on the X axis only;
    // rendering (GLB visual, animation) hangs off this and never feeds back in.
    public class Fighter : MonoBehaviour
    {
        public const float MaxHp = 1000f;
        public const int MaxMeterSegments = 3;
        public const float MeterPerSegment = 150f;

        public string DisplayName;
        public float Hp = MaxHp;
        public float Meter;                 // 0..450, one segment per 150
        public float WalkSpeed = 2.6f;
        public bool Blocking;
        public bool FacingRight = true;
        public Fighter Opponent;
        public FighterAnimator Anim;

        public float StunUntil;             // hit/knockdown stun (Time.time)
        public float AttackLockUntil;       // recovery after own attack
        public bool Airborne;
        public bool Dead;

        public int MeterSegments => Mathf.Min(MaxMeterSegments, (int)(Meter / MeterPerSegment));

        public bool CanAct => !Dead && Time.time >= StunUntil && Time.time >= AttackLockUntil && !RoundManager.RoundFrozen;

        public float DistanceTo(Fighter other) => Mathf.Abs(other.transform.position.x - transform.position.x);

        public void MoveAxis(float axis)
        {
            if (Dead || Blocking || Time.time < StunUntil || RoundManager.RoundFrozen) return;
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

        public void ResetForRound(float x)
        {
            Hp = MaxHp;
            Blocking = false;
            Airborne = false;
            Dead = false;
            StunUntil = 0f;
            AttackLockUntil = 0f;
            transform.position = new Vector3(x, 0f, 0f);
            Anim?.Play("idle", 0.1f);
        }
    }
}
