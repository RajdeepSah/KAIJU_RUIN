using UnityEngine;

namespace KaijuRuin
{
    // Kest. Every method here is a named binding from DESIGN_BRIEF.md controls.
    //
    // Session 4: a one-slot input buffer implements the brief's "next buffered
    // input" rule. Session 5 (D-015) layers the expanded moveset onto the SAME
    // one-thumb scheme: a swipe away is now an evasive back-dash, a light chain
    // can cancel into a heavier normal (target combos), a special cancels your own
    // recovery (normal xx special), and tap / swipe-up become air follow-ups while
    // the opponent is juggled. No new gestures are introduced.
    public class PlayerController : MonoBehaviour
    {
        public Fighter Self;
        // False when a remote peer drives this controller (RemoteController). Gates
        // the local-player HUD so the opponent's inputs never touch our pips/cards.
        public bool Local = true;

        int chainStep;              // 0 none, 1 jab, 2 cross, 3 finisher
        float chainWindowUntil;
        float normalCancelUntil;    // a connected light may cancel into a heavier normal

        enum Cmd { None, Tap, Heavy, Launcher, Sweep, S1, S2, S3 }
        Cmd buffered = Cmd.None;
        float bufferedAt;
        const float BufferWindow = 0.18f;   // ~11 frames at 60 fps

        bool FoeAirborne => Self.Opponent != null && Self.Opponent.Airborne;

        void Awake() { Self = GetComponent<Fighter>(); }

        void Update()
        {
            // Hit-stop / pause hold the buffer so it neither expires nor fires early.
            if (CombatFx.Frozen || GameManager.Paused) return;

            if (Time.time > chainWindowUntil) chainStep = 0;
            Self.Face(Self.Opponent);

            if (buffered != Cmd.None)
            {
                if (Time.time - bufferedAt > BufferWindow) buffered = Cmd.None;
                else if (Self.CanAct && !Self.Blocking) { var m = buffered; buffered = Cmd.None; Exec(m); }
            }
        }

        public void Move(float axis) => Self.MoveAxis(axis);

        // HUD chain pips reflect the LOCAL player only.
        void ChainHud(int step) { if (Local) TouchUI.I?.SetChainStep(step); }

        public void SetBlock(bool held)
        {
            if (Self.Dead) return;
            if (held)
            {
                Self.BeginBlock(true);              // a player hold is always a parry attempt
                Self.Anim?.Play("block", 0.08f);
                Self.Proc?.Play(ProcAnim.Move.Parry);
                buffered = Cmd.None;
            }
            else
            {
                Self.EndBlock();
                Self.Anim?.Play("idle", 0.15f);
            }
            if (Local) TouchUI.I?.SetBlockIndicator(held);
        }

        // Gate an attack request: execute now, cancel a light's recovery into a
        // heavier normal, buffer for recovery, or drop it.
        bool Ready(Cmd m)
        {
            if (Self.Dead || Self.Blocking) return false;
            if (Self.CanAct) return true;
            if (IsHeavyNormal(m) && Time.time < normalCancelUntil && InCancelableRecovery())
            {
                Self.AttackLockUntil = 0f;          // skip the light's remaining recovery
                normalCancelUntil = 0f;             // one cancel per connected light (no heavy-into-heavy)
                return true;
            }
            buffered = m; bufferedAt = Time.time;
            return false;
        }

        bool InCancelableRecovery()
        {
            return !Self.Dead && Time.time >= Self.StunUntil && Time.time < Self.AttackLockUntil
                   && !RoundManager.RoundFrozen && !CombatFx.Frozen && !GameManager.Paused && !Self.Dashing;
        }

        static bool IsHeavyNormal(Cmd m) => m == Cmd.Heavy || m == Cmd.Launcher || m == Cmd.Sweep;

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

        // Swipe away from the opponent: an evasive back-dash with i-frames. Only
        // from neutral (never a cancel) so it can't be abused to escape recovery.
        public void BackDash()
        {
            if (!Self.CanAct) return;
            chainStep = 0; ChainHud(0);
            float dir = Self.FacingRight ? -1f : 1f;             // away from the opponent
            Self.Dash(dir * 1.2f, 0.18f, 0.24f);                 // slide + i-frames
            Self.AttackLockUntil = Time.time + 0.30f;            // recovery
            Self.Proc?.Play(ProcAnim.Move.BackHop);
            CombatSystem.SpawnDash(Self);
            AudioManager.I?.Sfx("whiff", 0.5f);
            PerfMonitor.MarkImpact();       // caller (TouchInput.FireSwipe) already marked input
        }

        void DoTap()
        {
            if (FoeAirborne) { DoAir(CombatSystem.AirRake, ProcAnim.Move.AirRake); return; }
            chainStep = (Time.time <= chainWindowUntil) ? Mathf.Min(chainStep + 1, 3) : 1;
            chainWindowUntil = Time.time + 0.6f;
            var atk = chainStep == 1 ? CombatSystem.Jab : chainStep == 2 ? CombatSystem.Cross : CombatSystem.Finisher;
            float speed = chainStep == 1 ? 1.1f : chainStep == 2 ? 1.15f : 1.2f;   // per-step escalation
            Self.Anim?.Play("punch", 0.05f, speed);
            Self.Proc?.Play(chainStep == 1 ? ProcAnim.Move.Jab : chainStep == 2 ? ProcAnim.Move.Cross : ProcAnim.Move.Finisher);
            bool hit = CombatSystem.Resolve(Self, atk);
            PerfMonitor.MarkImpact();
            // Pips show LANDED hits only; a whiff/blocked tap drops the chain.
            ChainHud(hit ? chainStep : 0);
            if (hit) normalCancelUntil = Time.time + 0.35f;      // open the target-combo cancel window
            if (!hit || chainStep == 3) chainStep = 0;
        }

        void DoHeavy() { chainStep = 0; ChainHud(0); Self.Anim?.Play("punch", 0.05f, 0.8f); Self.Proc?.Play(ProcAnim.Move.Heavy); CombatSystem.Resolve(Self, CombatSystem.Heavy); PerfMonitor.MarkImpact(); }

        void DoLauncher()
        {
            if (FoeAirborne) { DoAir(CombatSystem.AirSlam, ProcAnim.Move.AirSlam); return; }
            chainStep = 0; ChainHud(0);
            Self.Anim?.Play("punch", 0.05f, 0.9f);
            Self.Proc?.Play(ProcAnim.Move.Launcher);
            CombatSystem.Resolve(Self, CombatSystem.Launcher);
            PerfMonitor.MarkImpact();
        }

        void DoSweep() { chainStep = 0; ChainHud(0); Self.Anim?.Play("punch", 0.05f, 0.9f); Self.Proc?.Play(ProcAnim.Move.Sweep); CombatSystem.Resolve(Self, CombatSystem.Sweep); PerfMonitor.MarkImpact(); }

        // Air follow-up on a juggled opponent (tap -> Air Rake, swipe-up -> Air Slam).
        void DoAir(CombatSystem.Attack atk, ProcAnim.Move g)
        {
            chainStep = 0; ChainHud(0);
            Self.Anim?.Play("punch", 0.05f, 1.15f);
            Self.Proc?.Play(g);
            CombatSystem.Resolve(Self, atk);
            PerfMonitor.MarkImpact();
        }

        public void CastSpecial(int slot)
        {
            var m = slot == 1 ? Cmd.S1 : slot == 2 ? Cmd.S2 : Cmd.S3;
            if (Self.Dead || Self.Blocking) { if (Local) TouchUI.I?.CardResult(slot, false); return; }
            // A special CANCELS your own attack recovery (spend meter to link) but
            // never fires through stun / hit-stop / pause / a dash.
            bool hardLocked = Time.time < Self.StunUntil || RoundManager.RoundFrozen
                              || CombatFx.Frozen || GameManager.Paused || Self.Dashing;
            if (hardLocked) { buffered = m; bufferedAt = Time.time; return; }
            DoSpecial(slot);
        }

        void DoSpecial(int slot)
        {
            int cost = slot;                      // card 1/2/3 costs 1/2/3 segments
            if (!Self.SpendSegments(cost)) { if (Local) TouchUI.I?.CardResult(slot, false); return; }
            Self.AttackLockUntil = 0f;            // cancel any lingering normal recovery (combo link)
            var atk = CombatSystem.Special(Self.SpecialSet, slot);   // cards follow the chosen character (D-017)

            if (CombatSystem.SpecialIsDash(Self.SpecialSet, slot) && Self.Opponent != null)
            {
                // Fox-fire Dash closes the gap before the hit lands, landing just
                // outside both push boxes (D-023) rather than at a fixed 0.9 m.
                var p = transform.position;
                p.x = CombatSystem.DashInX(Self, Self.Opponent);
                transform.position = p;
                CombatSystem.SpawnDash(Self);
            }
            chainStep = 0;
            Self.Anim?.Play("special", 0.05f);
            Self.Proc?.Play(Self.SpecialSet == "tengi" ? ProcAnim.Move.SpecialTengi : ProcAnim.Move.SpecialKest);
            CombatSystem.Resolve(Self, atk);
            if (Local) TouchUI.I?.CardResult(slot, true);
            TouchUI.I?.RefreshBars();
        }
    }
}
