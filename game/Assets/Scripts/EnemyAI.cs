using UnityEngine;

namespace KaijuRuin
{
    // Tengi. State machine per DESIGN_BRIEF.md: APPROACH / POKE / PUNISH /
    // DEFEND / SPEND, with per-round reaction delay and block-rate ramps.
    //
    // Session 5 (D-015) gave him the expanded toolkit. Session 9 (D-023) fixed how
    // he reads DISTANCE:
    //
    //  * Decisions are still rate-limited by ReactionDelay, but LOCOMOTION now runs
    //    every frame. Approach used to live inside the think tick, so he walked for
    //    one frame in every ~19 — an effective approach speed near a twentieth of
    //    his walk, which is why he drifted at range instead of committing.
    //  * Every attack is chosen from the moves whose EffectiveReach actually covers
    //    the current gap for THIS pairing, instead of from distance literals that
    //    predate per-character bodies. No more jabbing from 1.35 m with a 1.10 m
    //    jab, and no more burning two segments on a 1.4 m card from 2.8 m out.
    //  * He refuses to loiter in the one gap that only loses — where the opponent's
    //    longest normal covers him and none of his cover them. He either closes to
    //    his own poke range or steps out of the threat.
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
        public float IdleChance = 0.22f;    // chance to space instead of poking on a tick
        public float MixupRate = 0.55f;     // chance to answer a held guard with the tool that beats it (D-024)
        public float CrouchGuardRate = 0.35f;  // share of its guards taken in the crouch stance

        // Walking intent: chosen on a think tick, driven every frame.
        enum Intent { Hold, Advance, Retreat }
        Intent intent;
        float intentUntil;

        // Spacing targets, refreshed each think from the live pair of bodies.
        float holdAt = 1.0f;        // stop advancing here — just inside our own poke
        float backOffAt = 2.0f;     // stop retreating here — just outside their heavy

        // The poke mix, as weights over moves that must first pass a reach check.
        // As the gap opens the short tools simply drop out of the draw. Widened to
        // the full D-024 normal set: the AI has to use the same moveset the player
        // does, or the guard rules only bind one side of the fight.
        struct Poke
        {
            public CombatSystem.Attack Atk;
            public float Weight;
            public float Speed;
            public string Clip;
            public ProcAnim.Move Gesture;
        }

        static readonly Poke[] Pokes =
        {
            new Poke { Atk = CombatSystem.Jab,       Weight = 0.26f, Speed = 1.1f,  Clip = "clawjab",    Gesture = ProcAnim.Move.Jab },
            new Poke { Atk = CombatSystem.Cross,     Weight = 0.12f, Speed = 1.05f, Clip = "clawcross",  Gesture = ProcAnim.Move.Cross },
            new Poke { Atk = CombatSystem.Hook,      Weight = 0.10f, Speed = 1f,    Clip = "clawhook",   Gesture = ProcAnim.Move.Hook },
            new Poke { Atk = CombatSystem.Launcher,  Weight = 0.13f, Speed = 0.9f,  Clip = "clawupper",  Gesture = ProcAnim.Move.Launcher },
            new Poke { Atk = CombatSystem.Slam,      Weight = 0.07f, Speed = 0.85f, Clip = "clawslam",   Gesture = ProcAnim.Move.Slam },
            new Poke { Atk = CombatSystem.Heavy,     Weight = 0.10f, Speed = 0.8f,  Clip = "haymaker",   Gesture = ProcAnim.Move.Heavy },
            new Poke { Atk = CombatSystem.TailRound, Weight = 0.09f, Speed = 0.95f, Clip = "tailround",  Gesture = ProcAnim.Move.TailRound },
            new Poke { Atk = CombatSystem.Sweep,     Weight = 0.08f, Speed = 0.9f,  Clip = "tailsweep",  Gesture = ProcAnim.Move.Sweep },
            new Poke { Atk = CombatSystem.LegSweep,  Weight = 0.07f, Speed = 1.1f,  Clip = "legsweep",   Gesture = ProcAnim.Move.LegSweep },
            new Poke { Atk = CombatSystem.Bash,      Weight = 0.08f, Speed = 1.05f, Clip = "haunchbash", Gesture = ProcAnim.Move.Bash },
        };

        readonly float[] pokeWeights = new float[Pokes.Length];

        void Awake() { Self = GetComponent<Fighter>(); }

        void Update()
        {
            if (Self.Dead || RoundManager.RoundFrozen || GameManager.Paused || CombatFx.Frozen) { Self.MoveAxis(0f); return; }

            var foe = Self.Opponent;
            if (foe == null) { Self.MoveAxis(0f); return; }
            Self.Face(foe);

            if (Self.Blocking && Time.time >= blockUntil) SetGuard(Fighter.GuardKind.None, false);

            if (Time.time >= nextThinkAt)
            {
                nextThinkAt = Time.time + ReactionDelay;
                Think(foe);
            }

            Drive(foe);      // every frame, so an approach is an approach
        }

        void Think(Fighter foe)
        {
            float dist = Self.DistanceTo(foe);
            float poke = CombatSystem.EffectiveReach(Self, foe, CombatSystem.Jab);
            float swing = CombatSystem.EffectiveReach(Self, foe, CombatSystem.Heavy);
            float threat = CombatSystem.ThreatReach(foe, Self);   // what THEY can reach US with

            holdAt = Mathf.Max(CombatSystem.MinSeparation(Self, foe), poke - 0.12f);
            backOffAt = Mathf.Max(threat + 0.25f, holdAt);

            // AIR FOLLOW-UP: keep a juggle alive — but chase it rather than swinging
            // at air, since the air moves' longer reach is not infinite.
            if (foe.Airborne && Self.CanAct)
            {
                if (Random.value < 0.4f && CombatSystem.InRange(Self, foe, CombatSystem.AirSlam))
                { Cast(CombatSystem.AirSlam, "punch", 0.9f, ProcAnim.Move.AirSlam); return; }
                if (CombatSystem.InRange(Self, foe, CombatSystem.AirRake))
                { Cast(CombatSystem.AirRake, "punch", 1.1f, ProcAnim.Move.AirRake); return; }

                SetIntent(Intent.Advance);
                return;
            }

            bool foeAttacking = Time.time < foe.AttackLockUntil;

            // DEFEND: block only from a distance where something can actually land on
            // us — guarding at 2 m used to be a real outcome, and read as cowardice.
            if (foeAttacking && dist <= threat + 0.15f && Random.value < BlockRate)
            {
                bool parry = Round >= 2 && Random.value < ParryChance;
                // Which stance to take is a READ, not a coin flip: what hit him last
                // is what he braces against, so a player who leans on lows gets
                // crouch-guarded (and their overhead starts working, and vice versa).
                var kind = foe.LastAttackLow ? Fighter.GuardKind.Crouch
                         : foe.LastAttackOverhead ? Fighter.GuardKind.Standing
                         : Random.value < CrouchGuardRate ? Fighter.GuardKind.Crouch
                         : Fighter.GuardKind.Standing;
                SetGuard(kind, parry);
                blockUntil = Time.time + (parry ? 0.22f : 0.5f);
                return;
            }

            // EVADE: back-dash out of close pressure (i-frames) instead of eating it.
            if (foeAttacking && dist <= threat * 0.8f && Self.CanAct && Random.value < DashChance)
            {
                BackDash();
                return;
            }

            if (!Self.CanAct) return;

            // SPEND: meter priorities, each gated on the card reaching (a whiffed
            // card burns 1-3 segments, so this is the most expensive misread there is).
            if (TrySpecial(foe, dist, poke)) return;

            // MIX-UP: a held guard is a stance, not a wall — answer it with the tool
            // that beats the one they are actually in (D-024). Without this the low /
            // overhead pair and the grab would exist only on the player's side, and
            // holding guard against Tengi would be free.
            if (foe.Blocking && Random.value < MixupRate && TryMixup(foe)) return;

            // PUNISH: opponent stuck in recovery, inside our heaviest answer.
            if (foeAttacking && CombatSystem.InRange(Self, foe, CombatSystem.Heavy))
            { Cast(CombatSystem.Heavy, "haymaker", 0.8f, ProcAnim.Move.Heavy); return; }

            // Space out instead of poking on every single tick: an unbroken wall of
            // normals is neither readable nor punishable. Only rolled when something
            // WOULD have reached (the longest normal), so it can never stall an
            // approach — standing still out of range is the one thing to avoid.
            bool anythingReaches = CombatSystem.InRange(Self, foe, CombatSystem.LongestNormal);
            if (anythingReaches && Random.value < IdleChance) { SetIntent(Intent.Hold); return; }

            // POKE with whatever reaches from here.
            if (TryPoke(foe)) return;

            // SPACING: nothing we own covers this gap (so it is wider than our heavy).
            // If their longest normal still covers us, this is the losing pocket —
            // commit forward or leave it, never sit in it.
            if (dist <= threat + 0.10f) SetIntent(Random.value < 0.65f ? Intent.Advance : Intent.Retreat);
            else SetIntent(Intent.Advance);
        }

        // Beat the stance in front of us: grab ignores both guards, an overhead comes
        // over a crouch, a low goes under a stand. Returns false when nothing that
        // beats it is in range — the caller then falls through to normal spacing, so
        // this never freezes him in front of a turtle he cannot reach.
        bool TryMixup(Fighter foe)
        {
            if (CombatSystem.InRange(Self, foe, CombatSystem.Grab) && Random.value < 0.5f)
            { CastGrab(); return true; }

            if (foe.Guard == Fighter.GuardKind.Crouch)
            {
                if (CombatSystem.InRange(Self, foe, CombatSystem.Slam))
                { Cast(CombatSystem.Slam, "clawslam", 0.85f, ProcAnim.Move.Slam); return true; }
                return false;
            }

            bool longFirst = Random.value < 0.55f;
            if (longFirst && CombatSystem.InRange(Self, foe, CombatSystem.Sweep))
            { Cast(CombatSystem.Sweep, "tailsweep", 0.9f, ProcAnim.Move.Sweep); return true; }
            if (CombatSystem.InRange(Self, foe, CombatSystem.LegSweep))
            { Cast(CombatSystem.LegSweep, "legsweep", 1.1f, ProcAnim.Move.LegSweep); return true; }
            if (CombatSystem.InRange(Self, foe, CombatSystem.Sweep))
            { Cast(CombatSystem.Sweep, "tailsweep", 0.9f, ProcAnim.Move.Sweep); return true; }
            return false;
        }

        // Weighted draw over the normals whose effective reach covers the gap.
        bool TryPoke(Fighter foe)
        {
            float total = 0f;
            for (int i = 0; i < Pokes.Length; i++)
            {
                pokeWeights[i] = CombatSystem.InRange(Self, foe, Pokes[i].Atk) ? Pokes[i].Weight : 0f;
                total += pokeWeights[i];
            }
            if (total <= 0f) return false;

            float r = Random.value * total;
            for (int i = 0; i < Pokes.Length; i++)
            {
                if (pokeWeights[i] <= 0f) continue;
                r -= pokeWeights[i];
                if (r <= 0f) { Cast(Pokes[i].Atk, Pokes[i].Clip, Pokes[i].Speed, Pokes[i].Gesture); return true; }
            }
            // Float drift only: fall back to the longest tool that reached.
            for (int i = Pokes.Length - 1; i >= 0; i--)
                if (pokeWeights[i] > 0f) { Cast(Pokes[i].Atk, Pokes[i].Clip, Pokes[i].Speed, Pokes[i].Gesture); return true; }
            return false;
        }

        bool TrySpecial(Fighter foe, float dist, float poke)
        {
            int seg = Self.MeterSegments;
            if (seg >= 3 && Time.time < foe.StunUntil && CardReaches(foe, 3)) { CastSpecial(3); return true; }
            if (seg >= 2 && dist > poke && CardReaches(foe, 2)) { CastSpecial(2); return true; }
            if (seg >= 1 && Time.time < foe.AttackLockUntil && CardReaches(foe, 1)) { CastSpecial(1); return true; }
            return false;
        }

        // A gap-closing card (Kest's slot 1) makes its own range; everything else
        // has to already be in range.
        bool CardReaches(Fighter foe, int slot)
            => CombatSystem.SpecialIsDash(Self.SpecialSet, slot)
               || CombatSystem.InRange(Self, foe, CombatSystem.Special(Self.SpecialSet, slot));

        void SetIntent(Intent i)
        {
            intent = i;
            intentUntil = Time.time + ReactionDelay * 4f;   // re-decided long before this
        }

        // Locomotion: runs every frame off the standing intent, and stops itself at
        // the spacing target so he never walks through his own poke range.
        void Drive(Fighter foe)
        {
            if (Self.Blocking || !Self.CanAct) { Self.MoveAxis(0f); return; }
            if (Time.time > intentUntil) intent = Intent.Hold;

            float dist = Self.DistanceTo(foe);
            float toFoe = foe.transform.position.x > Self.transform.position.x ? 1f : -1f;

            switch (intent)
            {
                case Intent.Advance:
                    if (dist <= holdAt) { intent = Intent.Hold; Self.MoveAxis(0f); }
                    else Self.MoveAxis(toFoe * 0.9f);
                    break;
                case Intent.Retreat:
                    if (dist >= backOffAt) { intent = Intent.Hold; Self.MoveAxis(0f); }
                    else Self.MoveAxis(-toFoe * 0.8f);
                    break;
                default:
                    Self.MoveAxis(0f);
                    break;
            }
        }

        void BackDash()
        {
            float dir = Self.FacingRight ? -1f : 1f;
            Self.Dash(dir * 1.0f, 0.18f, 0.22f);
            Self.AttackLockUntil = Time.time + 0.32f;
            Self.Proc?.Play(ProcAnim.Move.BackHop);
            CombatSystem.SpawnDash(Self);
            SetIntent(Intent.Hold);
        }

        // Normals only (specials go through CastSpecial so cost/selection stay data-driven).
        // The clip is fitted to the move's recovery, same rule as the player's, so the
        // two sides of the fight telegraph at the same rate — an AI whose wind-ups are
        // slower than its hits is unreadable, which reads to a player as unfair.
        void Cast(CombatSystem.Attack atk, string animState, float speed, ProcAnim.Move g)
        {
            SetIntent(Intent.Hold);            // committed: hold the ground we struck from
            Self.Anim?.PlayFor(animState, atk.Recovery / Mathf.Max(0.5f, Self.AttackSpeed) * 1.5f / Mathf.Max(0.5f, speed));
            Self.Proc?.Play(g);
            CombatSystem.Resolve(Self, atk);
            TouchUI.I?.RefreshBars();
        }

        // Spend a card (1..3) from this fighter's special set — mirrors PlayerController.DoSpecial.
        void CastSpecial(int slot)
        {
            if (!Self.SpendSegments(slot)) return;
            SetIntent(Intent.Hold);
            Self.AttackLockUntil = 0f;
            var atk = CombatSystem.Special(Self.SpecialSet, slot);
            if (CombatSystem.SpecialIsDash(Self.SpecialSet, slot) && Self.Opponent != null)
            {
                var p = transform.position;
                p.x = CombatSystem.DashInX(Self, Self.Opponent);
                transform.position = p;
                CombatSystem.SpawnDash(Self);
            }
            Self.Anim?.Play("special", 0.05f);
            Self.Proc?.Play(Self.SpecialSet == "tengi" ? ProcAnim.Move.SpecialTengi : ProcAnim.Move.SpecialKest);
            CombatSystem.Resolve(Self, atk);
            TouchUI.I?.RefreshBars();
        }

        // Enter a guard stance (or leave it with GuardKind.None). The crouch stance
        // gets the crouch-guard clip and holds its procedural sink; the standing one
        // keeps the block clip.
        void SetGuard(Fighter.GuardKind kind, bool armed)
        {
            if (kind == Fighter.GuardKind.None)
            {
                Self.EndGuard();
                Self.Proc?.Release();
                Self.Anim?.Play("idle", 0.15f);
                return;
            }
            Self.BeginGuard(kind, armed, false);
            if (kind == Fighter.GuardKind.Crouch)
            {
                Self.Anim?.Play("crouchguard", 0.10f);
                Self.Proc?.Hold(ProcAnim.Move.Crouch);
            }
            else
            {
                Self.Anim?.Play("block", 0.08f);
                Self.Proc?.Play(ProcAnim.Move.Parry);
            }
        }

        // Grab: seize, then slam on a catch (mirrors PlayerController.DoGrab).
        void CastGrab()
        {
            SetIntent(Intent.Hold);
            float window = CombatSystem.Grab.Recovery / Mathf.Max(0.5f, Self.AttackSpeed) * 1.5f;
            Self.Anim?.PlayFor("grab", window * 0.5f);
            Self.Proc?.Play(ProcAnim.Move.Grab);
            bool hit = CombatSystem.Resolve(Self, CombatSystem.Grab);
            if (hit)
            {
                Self.Anim?.PlayFor("throw", window * 0.75f);
                Self.Proc?.Play(ProcAnim.Move.Throw);
            }
            TouchUI.I?.RefreshBars();
        }
    }
}
