using UnityEngine;

namespace KaijuRuin
{
    // Kest. Every method here is a named binding from DESIGN_BRIEF.md controls.
    //
    // Session 4: a one-slot input buffer implements the brief's "next buffered
    // input" rule. An attack that arrives during recovery is stored (not dropped)
    // and fires the frame the fighter can act again, so light-chain mashing is
    // recovery-limited (deterministic) instead of retry-limited (feels dropped).
    public class PlayerController : MonoBehaviour
    {
        public Fighter Self;

        int chainStep;              // 0 none, 1 jab, 2 cross, 3 finisher
        float chainWindowUntil;

        enum Cmd { None, Tap, Heavy, Launcher, Sweep, S1, S2, S3 }
        Cmd buffered = Cmd.None;
        float bufferedAt;
        const float BufferWindow = 0.18f;   // ~11 frames at 60 fps

        void Awake() { Self = GetComponent<Fighter>(); }

        void Update()
        {
            if (Time.time > chainWindowUntil) chainStep = 0;
            Self.Face(Self.Opponent);

            if (buffered != Cmd.None && !GameManager.Paused)
            {
                if (Time.time - bufferedAt > BufferWindow) buffered = Cmd.None;
                else if (Self.CanAct && !Self.Blocking) { var m = buffered; buffered = Cmd.None; Exec(m); }
            }
        }

        public void Move(float axis) => Self.MoveAxis(axis);

        public void SetBlock(bool held)
        {
            if (Self.Dead) return;
            Self.Blocking = held;
            if (held) { Self.Anim?.Play("block", 0.08f); buffered = Cmd.None; }
            else Self.Anim?.Play("idle", 0.15f);
            TouchUI.I?.SetBlockIndicator(held);
        }

        // Gate an attack request: execute now, buffer for recovery, or drop it.
        bool Ready(Cmd m)
        {
            if (Self.Dead || Self.Blocking) return false;
            if (!Self.CanAct) { buffered = m; bufferedAt = Time.time; return false; }
            return true;
        }

        void Exec(Cmd m)
        {
            switch (m)
            {
                case Cmd.Tap: DoTap(); break;
                case Cmd.Heavy: DoHeavy(); break;
                case Cmd.Launcher: DoLauncher(); break;
                case Cmd.Sweep: DoSweep(); break;
                case Cmd.S1: DoSpecial(1); break;
                case Cmd.S2: DoSpecial(2); break;
                case Cmd.S3: DoSpecial(3); break;
            }
        }

        public void TapAttack() { if (Ready(Cmd.Tap)) DoTap(); }
        public void HeavyAttack() { if (Ready(Cmd.Heavy)) DoHeavy(); }
        public void Launcher() { if (Ready(Cmd.Launcher)) DoLauncher(); }
        public void Sweep() { if (Ready(Cmd.Sweep)) DoSweep(); }

        void DoTap()
        {
            chainStep = (Time.time <= chainWindowUntil) ? Mathf.Min(chainStep + 1, 3) : 1;
            chainWindowUntil = Time.time + 0.6f;
            var atk = chainStep == 1 ? CombatSystem.Jab : chainStep == 2 ? CombatSystem.Cross : CombatSystem.Finisher;
            float speed = chainStep == 1 ? 1.1f : chainStep == 2 ? 1.15f : 1.2f;   // per-step escalation
            Self.Anim?.Play("punch", 0.05f, speed);
            bool hit = CombatSystem.Resolve(Self, atk);
            PerfMonitor.MarkImpact();
            // Pips show LANDED hits only: a whiff/blocked tap drops the chain so
            // the HUD never claims a combo that did not connect.
            TouchUI.I?.SetChainStep(hit ? chainStep : 0);
            if (!hit || chainStep == 3) chainStep = 0;
        }

        void DoHeavy() { chainStep = 0; TouchUI.I?.SetChainStep(0); Self.Anim?.Play("punch", 0.05f, 0.8f); CombatSystem.Resolve(Self, CombatSystem.Heavy); PerfMonitor.MarkImpact(); }
        void DoLauncher() { chainStep = 0; TouchUI.I?.SetChainStep(0); Self.Anim?.Play("punch", 0.05f, 0.9f); CombatSystem.Resolve(Self, CombatSystem.Launcher); PerfMonitor.MarkImpact(); }
        void DoSweep() { chainStep = 0; TouchUI.I?.SetChainStep(0); Self.Anim?.Play("punch", 0.05f, 0.9f); CombatSystem.Resolve(Self, CombatSystem.Sweep); PerfMonitor.MarkImpact(); }

        public void CastSpecial(int slot)
        {
            var m = slot == 1 ? Cmd.S1 : slot == 2 ? Cmd.S2 : Cmd.S3;
            if (Self.Dead || Self.Blocking) { TouchUI.I?.CardResult(slot, false); return; }
            if (!Self.CanAct) { buffered = m; bufferedAt = Time.time; return; }
            DoSpecial(slot);
        }

        void DoSpecial(int slot)
        {
            int cost = slot;                      // card 1/2/3 costs 1/2/3 segments
            if (!Self.SpendSegments(cost)) { TouchUI.I?.CardResult(slot, false); return; }
            var atk = slot == 1 ? CombatSystem.KestS1 : slot == 2 ? CombatSystem.KestS2 : CombatSystem.KestS3;

            if (slot == 1 && Self.Opponent != null)
            {
                // Fox-fire Dash closes the gap before the hit lands.
                float dir = Self.FacingRight ? 1f : -1f;
                float targetX = Self.Opponent.transform.position.x - dir * 0.9f;
                var p = transform.position; p.x = Mathf.Clamp(targetX, -6f, 6f); transform.position = p;
            }
            chainStep = 0;
            Self.Anim?.Play("special", 0.05f);
            CombatSystem.Resolve(Self, atk);
            TouchUI.I?.CardResult(slot, true);
            TouchUI.I?.RefreshBars();
        }
    }
}
