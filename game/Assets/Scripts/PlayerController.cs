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
    // the opponent is juggled.
    //
    // Session 10 (D-024) adds the kaiju-scaled mix-up set and the guard it plays
    // against, still on two thumbs' worth of gestures:
    //   * The LEFT thumb, which only walked before, now also carries the stance —
    //     hold away = walk back with a standing guard up (as in any 2D fighter, back
    //     is both retreat and block), hold down-and-away = crouch guard, hold down =
    //     crouch. The RIGHT-thumb hold is still the committed, parry-armed guard.
    //   * The RIGHT thumb's four swipe directions became eight: the diagonals carry
    //     the overhead claw slam, the haunch bash, the command grab and the leg
    //     sweep. Crouching turns a forward swipe into the tail roundhouse.
    // Attacking always drops the guard for its recovery (Fighter.OpenUpUntil), so a
    // stance is never held through the strikes thrown out of it.
    public class PlayerController : MonoBehaviour
    {
        public Fighter Self;
        // False when a remote peer drives this controller (RemoteController). Gates
        // the local-player HUD so the opponent's inputs never touch our pips/cards.
        public bool Local = true;

        int chainStep;              // 0 none, 1 claw jab, 2 claw cross, 3 claw hook
        float chainWindowUntil;
        float normalCancelUntil;    // a connected light may cancel into a heavier normal

        // Held stance from the left thumb, re-evaluated every frame by TouchInput.
        bool wantCrouch, wantBackGuard;
        bool holdGuard;             // the right-thumb hold: committed, parry-armed guard

        enum Stance { None, Crouch, StandGuard, CrouchGuard }
        Stance shownStance = Stance.None;    // what the rig/HUD is currently showing

        enum Cmd { None, Tap, Heavy, Launcher, Sweep, Slam, Bash, Grab, LegSweep, S1, S2, S3 }
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

            ApplyStance();

            if (buffered != Cmd.None)
            {
                if (Time.time - bufferedAt > BufferWindow) buffered = Cmd.None;
                else if (Self.CanAct) { var m = buffered; buffered = Cmd.None; Exec(m); }
            }
        }

        public void Move(float axis) => Self.MoveAxis(axis);

        // Left-thumb stance, driven every frame while a finger is down (both false
        // when it lifts). Kept separate from the attack channel so guarding never
        // costs the player their attacking thumb.
        public void SetStance(bool crouch, bool away)
        {
            wantCrouch = crouch;
            wantBackGuard = away;
        }

        // Resolve the held stance into a guard, every frame. Two reasons it runs
        // continuously rather than on a gesture edge: a stance held through an
        // attack's recovery has to come back the instant the recovery ends, and the
        // same finger's position can change stance without lifting. The right-thumb
        // hold (committed, parry-armed) outranks it while held.
        //
        // Animation and HUD are touched only when the resolved stance CHANGES —
        // FighterAnimator.Play restarts a clip that is already current, so calling it
        // per frame would pin the guard clip to frame zero.
        void ApplyStance()
        {
            if (Self.Dead) { shownStance = Stance.None; return; }
            if (holdGuard)
            {
                // Still holding after an attack dropped the guard: re-engage once the
                // recovery ends, but UNARMED — a parry must be a fresh press, not a
                // free by-product of having kept the thumb down.
                if (!Self.Blocking) Self.BeginGuard(Fighter.GuardKind.Standing, false, false);
                else if (Self.CanAct) ShowStance(Stance.StandGuard);
                return;
            }

            var want = wantCrouch && wantBackGuard ? Fighter.GuardKind.Crouch
                     : wantBackGuard ? Fighter.GuardKind.Standing
                     : Fighter.GuardKind.None;

            if (want == Fighter.GuardKind.None)
            {
                if (Self.Blocking) Self.EndGuard();
                // Crouching without guarding is still a stance: it is what turns a
                // tap into a leg sweep and a forward swipe into a tail roundhouse.
                Self.Crouching = wantCrouch && Self.CanGuard;
            }
            else Self.BeginGuard(want, false, want == Fighter.GuardKind.Standing);

            var now = Self.Guard == Fighter.GuardKind.Crouch ? Stance.CrouchGuard
                    : Self.Guard == Fighter.GuardKind.Standing ? Stance.StandGuard
                    : Self.Crouching ? Stance.Crouch : Stance.None;

            bool changed = now != shownStance;
            shownStance = now;
            // Re-assert a held stance once an attack thrown out of it has finished:
            // the attack's Play() left its own clip on the rig and cleared the
            // sustained crouch, so a fighter who is still holding down-back would
            // otherwise stand back up while the guard is still legally up.
            //
            // Deliberately NOT re-asserted for the walking guard: that one is allowed
            // to move, so its locomotion blend owns the rig, and forcing the block
            // clip back every frame would fight `SetLocomotion` for it.
            bool holdsPose = now == Stance.Crouch || now == Stance.CrouchGuard
                             || (now == Stance.StandGuard && !Self.GuardWalking);
            if (changed || (holdsPose && Self.CanAct)) ShowStance(now);
            if (changed && Local) TouchUI.I?.SetGuardIndicator(Self.Guard);
        }

        // Idempotent: safe to call every frame for a held stance.
        void ShowStance(Stance s)
        {
            switch (s)
            {
                case Stance.CrouchGuard:
                case Stance.Crouch:
                    if (Self.Anim != null && Self.Anim.Current != "crouchguard") Self.Anim.Play("crouchguard", 0.10f);
                    Self.Proc?.Hold(ProcAnim.Move.Crouch);
                    break;
                case Stance.StandGuard:
                    if (Self.Anim != null && Self.Anim.Current != "block") Self.Anim.Play("block", 0.10f);
                    Self.Proc?.Release();
                    break;
                default:
                    Self.Proc?.Release();
                    if (Self.CanAct && Self.Anim != null && Self.Anim.Current != "idle") Self.Anim.Play("idle", 0.15f);
                    break;
            }
        }

        // HUD chain pips reflect the LOCAL player only.
        void ChainHud(int step) { if (Local) TouchUI.I?.SetChainStep(step); }

        // The right-thumb hold: a committed standing guard that roots the fighter and
        // is the only guard that can parry. The left thumb's held-back guard walks
        // and cannot (holding away is cheap; standing your ground is the read).
        public void SetBlock(bool held)
        {
            if (Self.Dead) return;
            holdGuard = held;
            if (held)
            {
                // End any walking guard first: BeginGuard deliberately refuses to
                // re-arm the stance it is already in (the per-frame stance input would
                // otherwise hold the parry window open forever), and this press is a
                // genuine new one that has earned its 160 ms.
                Self.EndGuard();
                Self.BeginGuard(Fighter.GuardKind.Standing, true, false);
                Self.Anim?.Play("block", 0.08f);
                Self.Proc?.Play(ProcAnim.Move.Parry);
                buffered = Cmd.None;
                shownStance = Stance.StandGuard;
            }
            else
            {
                Self.EndGuard();
                Self.Anim?.Play("idle", 0.15f);
                shownStance = Stance.None;
            }
            if (Local) TouchUI.I?.SetGuardIndicator(Self.Guard);
        }

        // Gate an attack request: execute now, cancel a light's recovery into a
        // heavier normal, buffer for recovery, or drop it. Guarding is NOT a refusal
        // any more — attacking out of a held stance is the point of the stance, and
        // CombatSystem.Resolve drops the guard for the recovery (D-024).
        bool Ready(Cmd m)
        {
            if (Self.Dead) return false;
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

        // Which commands may cancel a connected light's recovery (target combos).
        static bool IsHeavyNormal(Cmd m) => m == Cmd.Heavy || m == Cmd.Launcher || m == Cmd.Sweep
                                            || m == Cmd.Slam || m == Cmd.Bash || m == Cmd.LegSweep;

        void Exec(Cmd m)
        {
            switch (m)
            {
                case Cmd.Tap: DoTap(); break;
                case Cmd.Heavy: DoHeavy(); break;
                case Cmd.Launcher: DoLauncher(); break;
                case Cmd.Sweep: DoSweep(); break;
                case Cmd.Slam: DoSlam(); break;
                case Cmd.Bash: DoBash(); break;
                case Cmd.Grab: DoGrab(); break;
                case Cmd.LegSweep: DoLegSweep(); break;
                case Cmd.S1: DoSpecial(1); break;
                case Cmd.S2: DoSpecial(2); break;
                case Cmd.S3: DoSpecial(3); break;
            }
        }

        public void TapAttack() { if (Ready(Cmd.Tap)) DoTap(); }
        public void HeavyAttack() { if (Ready(Cmd.Heavy)) DoHeavy(); }
        public void Launcher() { if (Ready(Cmd.Launcher)) DoLauncher(); }
        public void Sweep() { if (Ready(Cmd.Sweep)) DoSweep(); }
        public void ClawSlam() { if (Ready(Cmd.Slam)) DoSlam(); }
        public void HaunchBash() { if (Ready(Cmd.Bash)) DoBash(); }
        public void CommandGrab() { if (Ready(Cmd.Grab)) DoGrab(); }
        public void LegSweep() { if (Ready(Cmd.LegSweep)) DoLegSweep(); }

        // Swipe away from the opponent: an evasive back-dash with i-frames. Only
        // from neutral (never a cancel) so it can't be abused to escape recovery.
        public void BackDash()
        {
            if (!Self.CanAct) return;
            chainStep = 0; ChainHud(0);
            float dir = Self.FacingRight ? -1f : 1f;             // away from the opponent
            Self.Dash(dir * 1.2f, 0.18f, 0.24f);                 // slide + i-frames
            Self.AttackLockUntil = Time.time + 0.30f;            // recovery
            Self.OpenUpUntil(Self.AttackLockUntil);              // evading is not guarding
            shownStance = Stance.None;
            Self.Proc?.Play(ProcAnim.Move.BackHop);
            CombatSystem.SpawnDash(Self);
            AudioManager.I?.Sfx("whiff", 0.5f);
            PerfMonitor.MarkImpact();       // caller (TouchInput.FireSwipe) already marked input
        }

        void DoTap()
        {
            if (FoeAirborne) { DoAir(CombatSystem.AirRake, ProcAnim.Move.AirRake); return; }
            // From a crouch a tap is the quick low instead of the chain — the fast
            // way to open a standing guard.
            if (Self.Crouching) { DoLegSweep(); return; }
            chainStep = (Time.time <= chainWindowUntil) ? Mathf.Min(chainStep + 1, 3) : 1;
            chainWindowUntil = Time.time + 0.6f;
            var atk = chainStep == 1 ? CombatSystem.Jab : chainStep == 2 ? CombatSystem.Cross : CombatSystem.Hook;
            string clip = chainStep == 1 ? "clawjab" : chainStep == 2 ? "clawcross" : "clawhook";
            // Per-step escalation now rides the clip fit: each step of the chain gets
            // a slightly tighter window than the last.
            Self.Anim?.PlayFor(clip, ClipWindow(atk) * (chainStep == 1 ? 1f : chainStep == 2 ? 0.94f : 0.88f));
            Self.Proc?.Play(chainStep == 1 ? ProcAnim.Move.Jab : chainStep == 2 ? ProcAnim.Move.Cross : ProcAnim.Move.Hook);
            bool hit = CombatSystem.Resolve(Self, atk);
            PerfMonitor.MarkImpact();
            // Pips show LANDED hits only; a whiff/blocked tap drops the chain.
            ChainHud(hit ? chainStep : 0);
            if (hit) normalCancelUntil = Time.time + 0.35f;      // open the target-combo cancel window
            if (!hit || chainStep == 3) chainStep = 0;
        }

        // Swipe toward the foe: the haymaker, or the tail roundhouse out of a crouch
        // (the sunk stance is what a tail lash comes out of).
        void DoHeavy()
        {
            if (Self.Crouching) { Strike(CombatSystem.TailRound, "tailround", ProcAnim.Move.TailRound); return; }
            Strike(CombatSystem.Heavy, "haymaker", ProcAnim.Move.Heavy);
        }

        void DoLauncher()
        {
            if (FoeAirborne) { DoAir(CombatSystem.AirSlam, ProcAnim.Move.AirSlam); return; }
            Strike(CombatSystem.Launcher, "clawupper", ProcAnim.Move.Launcher);
        }

        void DoSweep() => Strike(CombatSystem.Sweep, "tailsweep", ProcAnim.Move.Sweep);
        void DoLegSweep() => Strike(CombatSystem.LegSweep, "legsweep", ProcAnim.Move.LegSweep);
        void DoSlam() => Strike(CombatSystem.Slam, "clawslam", ProcAnim.Move.Slam);
        void DoBash() => Strike(CombatSystem.Bash, "haunchbash", ProcAnim.Move.Bash);

        // Command grab: guard cannot answer it, so the honesty is all in the range and
        // the recovery. A connected grab switches to the body-slam clip mid-move, so
        // the throw is what the player sees on a catch and the empty reach is what
        // they see on a whiff.
        void DoGrab()
        {
            chainStep = 0; ChainHud(0);
            float window = ClipWindow(CombatSystem.Grab);
            Self.Anim?.PlayFor("grab", window * 0.5f);      // seize on the front half
            Self.Proc?.Play(ProcAnim.Move.Grab);
            bool hit = CombatSystem.Resolve(Self, CombatSystem.Grab);
            if (hit)
            {
                Self.Anim?.PlayFor("throw", window * 0.75f);
                Self.Proc?.Play(ProcAnim.Move.Throw);
            }
            PerfMonitor.MarkImpact();
        }

        // One normal: clear the chain, play its own clip + procedural gesture, resolve.
        void Strike(CombatSystem.Attack atk, string clip, ProcAnim.Move g)
        {
            chainStep = 0; ChainHud(0);
            Self.Anim?.PlayFor(clip, ClipWindow(atk));
            Self.Proc?.Play(g);
            CombatSystem.Resolve(Self, atk);
            PerfMonitor.MarkImpact();
        }

        // How long a move's clip is allowed to take: its own recovery at this
        // character's cadence, plus half again so the follow-through carries past the
        // moment the fighter can act (a clip cut exactly at recovery looks amputated).
        float ClipWindow(CombatSystem.Attack atk)
            => atk.Recovery / Mathf.Max(0.5f, Self.AttackSpeed) * 1.5f;

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
            if (Self.Dead) { if (Local) TouchUI.I?.CardResult(slot, false); return; }
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
