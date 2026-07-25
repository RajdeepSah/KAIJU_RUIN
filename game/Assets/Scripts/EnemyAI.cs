using UnityEngine;

namespace KaijuRuin
{
    // Tengi. State machine per DESIGN_BRIEF.md: APPROACH / POKE / PUNISH /
    // DEFEND / SPEND, with per-round reaction delay and block-rate ramps.
    //
    // Session 5 (D-015) gives Tengi the expanded toolkit so the fight stays
    // dynamic: air-juggle follow-ups after his own launcher, a ramped read-parry
    // (not just chip-block), and an evasive back-dash out of pressure.
    public class EnemyAI : MonoBehaviour
    {
        public Fighter Self;

        float nextThinkAt;
        float blockUntil;

        // Ramped by RoundManager at round start.
        public int Round = 1;
        public float ReactionDelay = 0.32f;
        public float BlockRate = 0.45f;
        public float ParryChance = 0f;      // chance a defend becomes an armed read-parry
        public float DashChance = 0.10f;    // chance to back-dash out of pressure

        void Awake() { Self = GetComponent<Fighter>(); }

        void Update()
        {
            if (Self.Dead || RoundManager.RoundFrozen || GameManager.Paused || CombatFx.Frozen) { Self.MoveAxis(0f); return; }
            Self.Face(Self.Opponent);

            if (Self.Blocking && Time.time >= blockUntil) SetBlock(false, false);
            if (Time.time < nextThinkAt) return;
            nextThinkAt = Time.time + ReactionDelay;

            var foe = Self.Opponent;
            if (foe == null) return;
            float dist = Self.DistanceTo(foe);

            // AIR FOLLOW-UP: keep the juggle going when Tengi has popped the player up.
            if (foe.Airborne && Self.CanAct && dist < 1.9f)
            {
                if (Random.value < 0.6f) Cast(CombatSystem.AirRake, "punch", 1.1f, ProcAnim.Move.AirRake);
                else Cast(CombatSystem.AirSlam, "punch", 0.9f, ProcAnim.Move.AirSlam);
                return;
            }

            bool foeAttacking = Time.time < foe.AttackLockUntil;

            // DEFEND: block, or at higher rounds gamble on an armed read-parry.
            if (foeAttacking && dist < 1.8f && Random.value < BlockRate)
            {
                bool parry = Round >= 2 && Random.value < ParryChance;
                SetBlock(true, parry);
                blockUntil = Time.time + (parry ? 0.22f : 0.5f);
                return;
            }

            // EVADE: sometimes back-dash out of pressure (i-frames) instead of eating it.
            if (foeAttacking && dist < 1.3f && Self.CanAct && Random.value < DashChance)
            {
                BackDash();
                return;
            }

            if (!Self.CanAct) return;

            // SPEND: meter priorities (cards follow this fighter's special set, D-017)
            int seg = Self.MeterSegments;
            if (seg >= 3 && foe.StunUntil > Time.time) { CastSpecial(3); return; }
            if (seg >= 2 && dist > 2.0f && dist < 3.0f) { CastSpecial(2); return; }
            if (seg >= 1 && dist < 1.6f && foeAttacking) { CastSpecial(1); return; }

            // PUNISH: opponent stuck in recovery at range
            if (foeAttacking && dist < 1.6f) { Cast(CombatSystem.Heavy, "punch", 0.8f, ProcAnim.Move.Heavy); return; }

            // POKE at range, APPROACH otherwise. A mix of pokes, launchers (to open
            // juggles), and heavies keeps the offense varied.
            if (dist <= 1.4f)
            {
                float r = Random.value;
                if (r < 0.5f) Cast(CombatSystem.Jab, "punch", 1.1f, ProcAnim.Move.Jab);
                else if (r < 0.72f) Cast(CombatSystem.Launcher, "punch", 0.9f, ProcAnim.Move.Launcher);
                else if (r < 0.86f) Cast(CombatSystem.Sweep, "punch", 0.9f, ProcAnim.Move.Sweep);
                else Cast(CombatSystem.Heavy, "punch", 0.8f, ProcAnim.Move.Heavy);
            }
            else
            {
                float dir = foe.transform.position.x > transform.position.x ? 1f : -1f;
                Self.MoveAxis(dir * 0.85f);
            }
        }

        void BackDash()
        {
            float dir = Self.FacingRight ? -1f : 1f;
            Self.Dash(dir * 1.0f, 0.18f, 0.22f);
            Self.AttackLockUntil = Time.time + 0.32f;
            Self.Proc?.Play(ProcAnim.Move.BackHop);
            CombatSystem.SpawnDash(Self);
        }

        // Normals only (specials go through CastSpecial so cost/selection stay data-driven).
        void Cast(CombatSystem.Attack atk, string animState, float speed, ProcAnim.Move g)
        {
            Self.Anim?.Play(animState, 0.05f, speed);
            Self.Proc?.Play(g);
            CombatSystem.Resolve(Self, atk);
            TouchUI.I?.RefreshBars();
        }

        // Spend a card (1..3) from this fighter's special set — mirrors PlayerController.DoSpecial.
        void CastSpecial(int slot)
        {
            if (!Self.SpendSegments(slot)) return;
            Self.AttackLockUntil = 0f;
            var atk = CombatSystem.Special(Self.SpecialSet, slot);
            if (CombatSystem.SpecialIsDash(Self.SpecialSet, slot) && Self.Opponent != null)
            {
                float dir = Self.FacingRight ? 1f : -1f;
                float targetX = Self.Opponent.transform.position.x - dir * 0.9f;
                var p = transform.position; p.x = Mathf.Clamp(targetX, -6f, 6f); transform.position = p;
                CombatSystem.SpawnDash(Self);
            }
            Self.Anim?.Play("special", 0.05f);
            Self.Proc?.Play(Self.SpecialSet == "tengi" ? ProcAnim.Move.SpecialTengi : ProcAnim.Move.SpecialKest);
            CombatSystem.Resolve(Self, atk);
            TouchUI.I?.RefreshBars();
        }

        void SetBlock(bool held, bool armed)
        {
            if (held) { Self.BeginBlock(armed); Self.Anim?.Play("block", 0.08f); Self.Proc?.Play(ProcAnim.Move.Parry); }
            else { Self.EndBlock(); Self.Anim?.Play("idle", 0.15f); }
        }
    }
}
