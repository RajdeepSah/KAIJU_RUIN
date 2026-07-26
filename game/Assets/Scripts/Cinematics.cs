using System.Collections.Generic;
using UnityEngine;

namespace KaijuRuin
{
    // Which hits earn a slow-motion shot (D-026). TimeDirector owns how a shot
    // LOOKS; this owns how rare it is, which is the harder half of the brief:
    // "only during certain attacks" is a budget problem, not an effect problem.
    //
    // THE FAILURE MODE THIS EXISTS TO PREVENT. Round length after the D-025
    // rebalance is 42-60 s with 32-45 clean hits in it, and cards now fire ~2.7x
    // more often than they used to. A shot on every heavy, or on every card, would
    // put the fight in slow motion for a fifth of its length -- at which point it is
    // not an accent, it is the frame rate. So: at most THREE non-KO shots per round,
    // at least SIX seconds apart. That is one shot per ~15-20 s of fighting, worst
    // case, and most rounds will see one or two.
    //
    // WHY THESE TRIGGERS. A "critical hit" here is earned, not rolled. There is no
    // RNG anywhere in this: a random crit would make the same input look different
    // on two identical reads, which is the one thing a fighting game may not do, and
    // it would also be the first non-deterministic thing in a sim that the live-PvP
    // plan wants to keep lockstep-able (D-017). Every trigger below is a state the
    // player can see coming and can cause on purpose:
    //
    //   COUNTER  a committed strike landing on someone who is still mid-swing --
    //            the whiff punish, the single most earned hit in any fighter.
    //   BREAKER  the hit that takes an opponent from above a quarter health to at or
    //            below it. Self-limiting: health only falls within a round, so this
    //            can fire at most once per fighter per round by construction.
    //   COMEBACK a heavy landed from under a fifth health -- the round's turn, once.
    //   SUPER    a tier-3 card connecting. Only tier 3: it costs the whole meter bar,
    //            so it is rare by economy. Tiers 1 and 2 deliberately do NOT qualify
    //            (they are the ones D-025 made frequent).
    //   K.O.     the finishing blow, always, and deeper still when it takes the match.
    //
    // Damage is untouched by all of this. "Critical" is a classification of the
    // MOMENT, not a multiplier: D-025's damage table is one session old and was
    // tuned move-by-move to a 37.5% factor, so quietly adding crit damage on top
    // would undo it. If crit damage is ever wanted it is a separate decision with
    // its own re-tune.
    public static class Cinematics
    {
        // ---- Budget ---------------------------------------------------------
        // Deliberately set at the RARE end for the first playtest. Too rare is a
        // one-constant fix; too frequent is the exact failure the brief names, and by
        // the time it is felt the effect has already stopped meaning anything. At
        // 2 / 7 s a 42-60 s round holds at most two of these plus its K.O. — roughly
        // 5% of the 32-45 clean hits a round now contains (D-025).
        public const int MaxShotsPerRound = 2;    // K.O. shots are exempt (there is one)
        public const float ShotCooldown = 7f;     // unscaled seconds between shots

        // Thresholds the two health-state triggers read, as fractions of a full bar.
        public const float BreakerAt = 0.25f;
        public const float ComebackAt = 0.20f;

        /// Set by RoundManager when a live remote peer is driving the opponent.
        /// A cinematic dilates local time, and dilating one peer's clock in a
        /// lockstep match is a desync, not an effect (D-017). Loopback/AI is fine —
        /// there is no second clock.
        public static bool OnlineLockout;

        static int shotsThisRound;
        static float lastShotAt = -999f;
        static readonly HashSet<Fighter> comebackUsed = new HashSet<Fighter>();

        /// Called from RoundManager.StartRound, next to CombatFx.Reset(), so no
        /// budget or cooldown ever leaks across a round boundary.
        public static void ResetRound()
        {
            shotsThisRound = 0;
            lastShotAt = -999f;
            comebackUsed.Clear();
        }

        // Non-KO shots only. The round-frozen check keeps cinema out of banners and
        // the between-round Khulandra beat; unscaled time is used for the cooldown
        // because a budget measured in slowed seconds would refill faster the more
        // it was spent.
        static bool Ready =>
            TimeDirector.Enabled && TimeDirector.I != null && !OnlineLockout
            && !GameManager.Paused && !RoundManager.RoundFrozen
            && shotsThisRound < MaxShotsPerRound
            && Time.unscaledTime - lastShotAt >= ShotCooldown;

        static void Fire(in TimeDirector.Shot s)
        {
            shotsThisRound++;
            lastShotAt = Time.unscaledTime;
            TimeDirector.Play(s);
        }

        /// Every CLEAN (unblocked, unparried, non-fatal) hit reports here.
        ///
        /// `wasCounter` and `hpBefore` must be captured by the caller BEFORE the hit
        /// mutates the target: Resolve writes StunUntil and Hp part-way through, and
        /// reading them afterwards would make every hit look like a counter on a
        /// stunned opponent whose health never changed.
        public static void OnCleanHit(Fighter attacker, Fighter target, in CombatSystem.Attack atk,
                                      bool wasCounter, float hpBefore)
        {
            if (attacker == null || target == null || !Ready) return;

            // A tier-3 card connecting is its own tier of moment, above any crit.
            if (atk.CardSlot == 3) { Fire(TimeDirector.Super); return; }

            // Only a committed strike can be critical. A jab that happens to catch
            // someone mid-swing is a good poke, not a cinematic.
            bool committed = atk.Fx == CombatSystem.FxWeight.Heavy
                          || atk.Fx == CombatSystem.FxWeight.Launch
                          || atk.Fx == CombatSystem.FxWeight.Special
                          || atk.Grab;

            if (wasCounter && committed) { Fire(TimeDirector.Critical); return; }

            float breaker = Fighter.MaxHp * BreakerAt;
            if (hpBefore > breaker && target.Hp <= breaker) { Fire(TimeDirector.Critical); return; }

            if (committed && attacker.Hp <= Fighter.MaxHp * ComebackAt && comebackUsed.Add(attacker))
                Fire(TimeDirector.Critical);
        }

        /// The finishing blow. Exempt from the budget and the cooldown — there is
        /// exactly one per round, and it is the moment the whole system exists for.
        /// Not gated on RoundFrozen either: the KO is what SETS it.
        public static void OnKo(bool winsMatch)
        {
            if (!TimeDirector.Enabled || OnlineLockout) return;
            TimeDirector.Play(winsMatch ? TimeDirector.MatchKo : TimeDirector.Ko);
        }
    }
}
