using UnityEngine;

namespace KaijuRuin
{
    // Tengi. State machine per DESIGN_BRIEF.md: APPROACH / POKE / PUNISH /
    // DEFEND / SPEND, with per-round reaction delay and block-rate ramps.
    public class EnemyAI : MonoBehaviour
    {
        public Fighter Self;

        float nextThinkAt;
        float blockUntil;

        // Ramped by RoundManager at round start: round 1 -> 0.32s, 2 -> 0.26s, 3 -> 0.20s
        public float ReactionDelay = 0.32f;
        public float BlockRate = 0.45f;

        void Awake() { Self = GetComponent<Fighter>(); }

        void Update()
        {
            if (Self.Dead || RoundManager.RoundFrozen || GameManager.Paused) { Self.MoveAxis(0f); return; }
            Self.Face(Self.Opponent);

            if (Self.Blocking && Time.time >= blockUntil) SetBlock(false);
            if (Time.time < nextThinkAt) return;
            nextThinkAt = Time.time + ReactionDelay;

            var foe = Self.Opponent;
            if (foe == null) return;
            float dist = Self.DistanceTo(foe);

            // DEFEND: react to an incoming attack window
            bool foeAttacking = Time.time < foe.AttackLockUntil;
            if (foeAttacking && dist < 1.8f && Random.value < BlockRate)
            {
                SetBlock(true);
                blockUntil = Time.time + 0.5f;
                return;
            }

            if (!Self.CanAct) return;

            // SPEND: meter priorities
            int seg = Self.MeterSegments;
            if (seg >= 3 && foe.StunUntil > Time.time) { Cast(CombatSystem.TengiS3, "special"); return; }
            if (seg >= 2 && dist > 2.0f && dist < 3.0f) { Cast(CombatSystem.TengiS2, "special"); return; }
            if (seg >= 1 && dist < 1.6f && foeAttacking) { Cast(CombatSystem.TengiS1, "special"); return; }

            // PUNISH: opponent stuck in recovery at range
            if (foeAttacking && dist < 1.6f) { Cast(CombatSystem.Heavy, "punch", 0.8f); return; }

            // POKE at range, APPROACH otherwise
            if (dist <= 1.4f)
            {
                if (Random.value < 0.6f) Cast(CombatSystem.Jab, "punch", 1.1f);
                else Cast(CombatSystem.Heavy, "punch", 0.8f);
            }
            else
            {
                float dir = foe.transform.position.x > transform.position.x ? 1f : -1f;
                Self.MoveAxis(dir * 0.8f);
            }
        }

        void Cast(CombatSystem.Attack atk, string animState, float speed = 1f)
        {
            if (atk.Vfx != null)
            {
                int cost = atk.Name == "Crow Wall" ? 1 : atk.Name == "Culling Arc" ? 2 : 3;
                if (!Self.SpendSegments(cost)) return;
            }
            Self.Anim?.Play(animState, 0.05f, speed);
            CombatSystem.Resolve(Self, atk);
            TouchUI.I?.RefreshBars();
        }

        void SetBlock(bool held)
        {
            Self.Blocking = held;
            if (held) Self.Anim?.Play("block", 0.08f);
            else Self.Anim?.Play("idle", 0.15f);
        }
    }
}
