using System.Collections.Generic;
using UnityEngine;

namespace KaijuRuin
{
    // Deterministic hit resolution on the X axis. Base numbers from DESIGN_BRIEF.md;
    // Session 5 (D-015) tightens recoveries for a faster fight and adds parry,
    // dash i-frames, air-juggle decay, forward step-in, and per-hit impact FX.
    public static class CombatSystem
    {
        public enum FxWeight { None, Light, Medium, Heavy, Launch, Special, Parry }

        public struct Attack
        {
            public string Name;
            public float Damage;
            public float Reach;
            public float Recovery;
            public float Knockback;
            public float StepIn;    // forward drive on use (aggressive flow)
            public bool Low;        // hits low: beats STANDING guard, stopped by crouch guard
            public bool Overhead;   // comes down from above: beats CROUCH guard, stopped by standing guard
            public bool Grab;       // ignores guard entirely; cannot catch an airborne body
            public bool Knockdown;  // floors the target (long stun + wake-up i-frames)
            public bool Launch;     // pops / keeps the target airborne
            public FxWeight Fx;     // hit-stop + shake profile
            public string Vfx;      // sprite under Resources/Art/vfx, optional (marks a special)
            public string Sfx;
        }

        const float ParryWindow = 0.16f;   // block within this of its start = perfect guard
        const int MaxJuggle = 4;           // air hits before a forced knockdown

        // ---- Distance model (D-023) -----------------------------------------
        // Every Attack.Reach below is a CENTRE-TO-CENTRE distance quoted for the
        // baseline body — Kest, measured off his rigged GLB: 0.78 m of knuckle
        // reach and a 0.32 m hurt half-depth (their sum, 1.10 m, is exactly the
        // brief's jab reach, which is why Kest is the baseline). A fighter with a
        // longer arm strikes from further out, and a deeper body gets struck from
        // further out, so the effective range is the listed reach adjusted by both
        // bodies. Without this, one Reach number served a 1.8 m werefox and a
        // 2.4 m culler alike: Tengi's fist visibly buried itself in Kest for
        // 0.36 m before anything registered.
        public const float BaselineArm = 0.78f;
        public const float BaselineHurt = 0.32f;

        // Centre-to-centre distance at which `atk` connects, for this pairing.
        public static float EffectiveReach(Fighter attacker, Fighter target, Attack atk)
            => atk.Reach + (attacker.ArmReach - BaselineArm) + (target.HurtDepth - BaselineHurt);

        // How far past its own centre a strike can touch a SURFACE. Independent of
        // the target (the target's depth cancels out), so this is the honest number
        // to draw on the ground as "where my fist tops out" — for Kest's jab it
        // comes out at 0.78 m, his knuckles exactly.
        public static float StrikeExtent(Fighter attacker, Attack atk)
            => atk.Reach + attacker.ArmReach - BaselineArm - BaselineHurt;

        // The longest normal `attacker` can threaten `target` with. The AI's spacing
        // reads this to know when it is standing in danger, and the ground cues draw
        // it, so it must stay the actual maximum over the normals below (the tail
        // roundhouse outranges the haymaker since D-024).
        public static float ThreatReach(Fighter attacker, Fighter target)
            => EffectiveReach(attacker, target, LongestNormal);

        public static bool InRange(Fighter attacker, Fighter target, Attack atk)
            => target != null && !target.Dead && InFront(attacker, target)
               && attacker.DistanceTo(target) <= EffectiveReach(attacker, target, atk);

        // A strike only reaches what it is turned toward. Both controllers re-face
        // every frame so this is normally a formality, but it closes the phantom
        // hit where a fighter connects with something behind their own back.
        static bool InFront(Fighter attacker, Fighter target)
            => (target.transform.position.x - attacker.transform.position.x)
               * (attacker.FacingRight ? 1f : -1f) > -0.01f;

        // Closest the two bodies may stand. Push boxes are narrower than hurt
        // boxes (a cloak reads as silhouette but is not solid), same as any fighter.
        public static float MinSeparation(Fighter a, Fighter b) => a.PushDepth + b.PushDepth;

        // Bodies are solid on the sim axis: neither fighter may walk through the
        // other. RoundManager runs this once per frame, after all movement, as the
        // single authority — so the gap the player SEES and the gap the hit check
        // reads can never disagree, and point-blank stops being a state where every
        // move connects because the two roots share an X.
        public static void Separate(Fighter a, Fighter b)
        {
            if (a == null || b == null || a.Dead || b.Dead) return;
            float ax = a.transform.position.x, bx = b.transform.position.x;
            float gap = bx - ax;
            float sign = gap >= 0f ? 1f : -1f;                  // +1 when b is to the right
            float overlap = MinSeparation(a, b) - Mathf.Abs(gap);
            if (overlap <= 0.0001f) return;

            // Split the correction, except that a fighter pinned against a soft
            // wall pushes the other the whole way instead of sinking through it.
            float aPush = -sign * overlap * 0.5f, bPush = sign * overlap * 0.5f;
            if (Mathf.Abs(ax + aPush) >= Fighter.Arena) { aPush = 0f; bPush = sign * overlap; }
            else if (Mathf.Abs(bx + bPush) >= Fighter.Arena) { bPush = 0f; aPush = -sign * overlap; }

            SetX(a, ax + aPush);
            SetX(b, bx + bPush);
        }

        static void SetX(Fighter f, float x)
        {
            var p = f.transform.position;
            p.x = Mathf.Clamp(x, -Fighter.Arena, Fighter.Arena);
            f.transform.position = p;
        }

        // ---- Universal normals ----------------------------------------------
        // Shared by the whole roster (no per-character variants): only the feel
        // knobs on CharacterDef — cadence, walk, body metrics, procedural amplitude
        // — separate one champion's version of a move from another's.
        //
        // Recoveries were trimmed from v1 for a faster fight (D-015). Session 10
        // (D-024) widened the set to kaiju-scaled claw / tail / haunch strikes plus a
        // guard-breaking grab, and every move now declares how it interacts with
        // guard: nothing (a mid), Low, Overhead, or Grab. Reaches remain
        // centre-to-centre distances quoted for Kest's measured body (D-023).
        //
        // The guard triangle these numbers exist to serve:
        //   standing guard  <- beaten by Low (tail sweep, leg sweep) and by Grab
        //   crouch guard    <- beaten by Overhead (claw slam) and by Grab
        //   no guard        <- beaten by everything, hardest by the haymaker
        public static readonly Attack Jab      = new Attack { Name = "Claw Jab",      Damage = 40,  Reach = 1.10f, Recovery = 0.20f, StepIn = 0.06f, Fx = FxWeight.Light,  Sfx = "hit_light" };
        public static readonly Attack Cross    = new Attack { Name = "Claw Cross",    Damage = 55,  Reach = 1.20f, Recovery = 0.24f, StepIn = 0.10f, Fx = FxWeight.Light,  Sfx = "hit_light" };
        public static readonly Attack Hook     = new Attack { Name = "Claw Hook",     Damage = 75,  Reach = 1.15f, Recovery = 0.34f, Knockback = 0.5f, StepIn = 0.12f, Fx = FxWeight.Medium, Sfx = "hit_heavy" };
        public static readonly Attack Launcher = new Attack { Name = "Rising Claw",   Damage = 90,  Reach = 1.35f, Recovery = 0.50f, StepIn = 0.12f, Launch = true, Fx = FxWeight.Launch, Sfx = "hit_heavy" };
        public static readonly Attack Slam     = new Attack { Name = "Claw Slam",     Damage = 110, Reach = 1.30f, Recovery = 0.56f, Knockback = 0.8f, StepIn = 0.14f, Overhead = true, Fx = FxWeight.Heavy, Sfx = "hit_heavy" };
        public static readonly Attack Heavy    = new Attack { Name = "Haymaker",      Damage = 120, Reach = 1.60f, Recovery = 0.62f, Knockback = 1.5f, StepIn = 0.22f, Fx = FxWeight.Heavy,  Sfx = "hit_heavy" };
        public static readonly Attack TailRound= new Attack { Name = "Tail Roundhouse",Damage = 100,Reach = 1.75f, Recovery = 0.54f, Knockback = 1.2f, StepIn = 0.16f, Fx = FxWeight.Heavy, Sfx = "hit_heavy" };
        public static readonly Attack Sweep    = new Attack { Name = "Tail Sweep",    Damage = 80,  Reach = 1.45f, Recovery = 0.50f, StepIn = 0.20f, Low = true, Knockdown = true, Fx = FxWeight.Medium, Sfx = "hit_light" };
        public static readonly Attack LegSweep = new Attack { Name = "Leg Sweep",     Damage = 55,  Reach = 1.15f, Recovery = 0.32f, StepIn = 0.14f, Low = true,    Fx = FxWeight.Light,  Sfx = "hit_light" };
        public static readonly Attack Bash     = new Attack { Name = "Haunch Bash",   Damage = 70,  Reach = 0.95f, Recovery = 0.36f, Knockback = 0.9f, StepIn = 0.08f, Fx = FxWeight.Medium, Sfx = "hit_heavy" };

        // Command grab: the answer to a fighter who simply holds guard. Ignores both
        // stances, so its cost is range and commitment — it reaches barely past the
        // push boxes and its whiff is the longest recovery of any normal.
        public static readonly Attack Grab     = new Attack { Name = "Command Grab",  Damage = 140, Reach = 0.85f, Recovery = 0.70f, Knockback = 1.0f, Grab = true, Knockdown = true, Fx = FxWeight.Heavy, Sfx = "hit_heavy" };

        // The longest normal in the set — the AI's threat radius and the outer ground
        // band both read this rather than naming a move.
        public static readonly Attack LongestNormal = TailRound;

        // Does `target`'s held stance stop this attack? The mirror the mix-up rests
        // on: a low goes under a standing guard, an overhead comes over a crouching
        // one, a grab ignores both, and an airborne fighter guards nothing.
        public static bool GuardStops(Fighter target, Attack atk)
        {
            if (!target.Blocking || target.Airborne || atk.Grab) return false;
            return target.Guard == Fighter.GuardKind.Crouch ? !atk.Overhead : !atk.Low;
        }

        // Air-juggle follow-ups (used only while the target is airborne, D-015).
        public static readonly Attack AirRake  = new Attack { Name = "Air Rake",  Damage = 55, Reach = 1.4f, Recovery = 0.30f, StepIn = 0.10f, Launch = true, Fx = FxWeight.Medium, Sfx = "hit_light" };
        public static readonly Attack AirSlam  = new Attack { Name = "Air Slam",  Damage = 95, Reach = 1.5f, Recovery = 0.50f, Knockback = 1.4f, StepIn = 0.10f, Fx = FxWeight.Heavy, Sfx = "hit_heavy" };

        public static Attack KestS1  => new Attack { Name = "Fox-fire Dash",  Damage = 100, Reach = 3.2f, Recovery = 0.5f, Knockback = 1.0f, Fx = FxWeight.Special, Vfx = "kest_foxfire",     Sfx = "kest_special" };
        public static Attack KestS2  => new Attack { Name = "Phantom Rake",   Damage = 160, Reach = 1.4f, Recovery = 0.7f, Knockback = 0.8f, Fx = FxWeight.Special, Vfx = "kest_foxfire",     Sfx = "kest_special" };
        public static Attack KestS3  => new Attack { Name = "Hunt of Shadows",Damage = 280, Reach = 2.2f, Recovery = 1.0f, Knockback = 2.0f, Fx = FxWeight.Special, Vfx = "kest_foxfire",     Sfx = "kest_special" };
        public static Attack TengiS1 => new Attack { Name = "Crow Wall",      Damage = 130, Reach = 1.4f, Recovery = 0.6f, Knockback = 1.0f, Fx = FxWeight.Special, Vfx = "tengi_bladewave",  Sfx = "tengi_special" };
        public static Attack TengiS2 => new Attack { Name = "Culling Arc",    Damage = 180, Reach = 3.0f, Recovery = 0.8f, Knockback = 1.5f, Fx = FxWeight.Special, Vfx = "tengi_bladewave",  Sfx = "tengi_special" };
        public static Attack TengiS3 => new Attack { Name = "Black Sun",      Damage = 300, Reach = 1.8f, Recovery = 1.4f, Knockback = 2.2f, Fx = FxWeight.Special, Vfx = "tengi_bladewave",  Sfx = "tengi_special" };

        // Card slot (1..3) -> Attack for a character's special set. Adding a set for a
        // new champion is one more branch here + its cards on the fighter (D-017).
        public static Attack Special(string set, int slot)
        {
            if (set == "tengi") return slot == 1 ? TengiS1 : slot == 2 ? TengiS2 : TengiS3;
            return slot == 1 ? KestS1 : slot == 2 ? KestS2 : KestS3;
        }

        // True when this special set's slot-1 is a gap-closing dash (Kest's Fox-fire
        // Dash). Kept as data so the controllers don't hardcode a character name.
        public static bool SpecialIsDash(string set, int slot) => set == "kest" && slot == 1;

        // Where a gap-closing special plants the attacker: touching, but just outside
        // the push boxes, so the strike that follows is inside every move's range and
        // the two bodies never interpenetrate on arrival.
        public static float DashInX(Fighter attacker, Fighter target)
        {
            float dir = attacker.FacingRight ? 1f : -1f;
            float x = target.transform.position.x - dir * (MinSeparation(attacker, target) + 0.10f);
            return Mathf.Clamp(x, -Fighter.Arena, Fighter.Arena);
        }

        // Returns true when the attack connected (clean, unblocked).
        public static bool Resolve(Fighter attacker, Attack atk)
        {
            // Recovery scaled by the attacker's cadence (per-character + global speed-up).
            float recovery = atk.Recovery / Mathf.Max(0.5f, attacker.AttackSpeed);
            attacker.AttackLockUntil = Time.time + recovery;
            // Swinging opens you up: the guard drops for the whole recovery, so a
            // stance can never be held through the attacks thrown out of it (D-024).
            attacker.OpenUpUntil(attacker.AttackLockUntil);
            attacker.LastAttackLow = atk.Low;
            attacker.LastAttackOverhead = atk.Overhead;

            var target = attacker.Opponent;
            if (target == null || target.Dead) return false;

            // Reach is checked against the PRE-step-in distance so listed ranges hold
            // and whiff-punish still works; step-in is a follow-through on connect only.
            // A low sweep also passes harmlessly under a juggled opponent, who is now
            // visibly off the ground (ProcAnim lift) — a low that hit an airborne body
            // was the most obvious phantom hit in the slice.
            // A grab closes on a body that is standing there to be seized; a juggled
            // one is not, so it whiffs like a low passing under an airborne fighter.
            if (!InRange(attacker, target, atk) || ((atk.Low || atk.Grab) && target.Airborne))
            {
                // Whiff: readable answer so "I missed and I'm exposed" is felt.
                AudioManager.I?.Sfx("whiff", 0.4f);   // soft-fails until the clip exists
                SpawnWhiffVfx(attacker);
                return false;
            }

            // Dash i-frames: a well-timed evade lets the strike pass through.
            if (target.Invulnerable)
            {
                AudioManager.I?.Sfx("whiff", 0.45f);
                SpawnWhiffVfx(attacker);
                return false;
            }

            // Forward step-in: a confirmed hit drives the attacker toward the foe
            // (aggressive flow, D-015) without ever crossing past a minimum gap. It
            // never extends reach (applied after the whiff check).
            if (atk.StepIn > 0f) StepIn(attacker, target, atk.StepIn);

            // A hit resolves on the input frame, but hit-stop freezes the pose a
            // moment later — so without snapping the attacker's gesture to its peak
            // the freeze frame shows a fighter who has not extended yet, which is
            // exactly what reads as "that shouldn't have reached me". Same for the
            // victim's recoil below.
            attacker.Proc?.Contact();

            var impact = ImpactPoint(attacker, target, atk);
            bool blocked = GuardStops(target, atk);
            bool parried = blocked && target.ParryArmed && (Time.time - target.BlockStartedAt) <= ParryWindow;

            if (parried)
            {
                // Perfect guard: zero damage, attacker opened up for a punish (a launch
                // is possible but now bounded by wake-up i-frames, not a TOD), modest
                // meter reward so parry is strong-but-not-dominant (D-015 review fix).
                attacker.StunUntil = Time.time + 0.30f;
                attacker.AttackLockUntil = Mathf.Max(attacker.AttackLockUntil, attacker.StunUntil);
                attacker.Anim?.Play("hit", 0.05f);
                attacker.Proc?.Play(ProcAnim.Move.Hit);
                attacker.Proc?.Contact();
                target.GainMeter(40f);
                target.ParryArmed = false;                    // one parry per block press
                AudioManager.I?.Sfx("block", 0.9f);
                SpawnFx("parry_spark", "meter_flare", impact, 1.3f, AssetLib.GoryoFlame);
                Spawn("hit_spark", impact, 0.7f, AssetLib.BonePaper);
                ApplyImpactFx(FxWeight.Parry);
                TouchUI.I?.RefreshBars();
                TouchUI.I?.OnParry(target);
                return false;
            }

            float damage = atk.Damage;
            if (blocked)
            {
                damage *= IsSpecial(atk) ? 0.10f : 0.25f;     // 75% reduction, 10% chip on specials
                AudioManager.I?.Sfx("block");
                target.Anim?.Play("block", 0.05f);
                SpawnBlockVfx(impact);                         // deflect spark, no blood
            }
            else
            {
                // Air-juggle bookkeeping: each successive air hit decays, and the
                // juggle ends (knockdown) after MaxJuggle so it can't loop forever.
                bool wasAirborne = target.Airborne;
                if (wasAirborne) { target.JuggleCount++; damage *= Mathf.Max(0.5f, 1f - 0.1f * target.JuggleCount); }
                bool keepAir = atk.Launch && (!wasAirborne || target.JuggleCount < MaxJuggle);

                AudioManager.I?.Sfx(atk.Sfx);
                // A knockdown (grab throw, tail sweep) floors the target for longer
                // than a normal hit — that long stun is what makes the mix-up worth
                // landing, and the wake-up i-frames below are what stop it looping.
                bool floored = atk.Knockdown && !keepAir;
                target.StunUntil = Time.time + (keepAir ? 0.7f : floored ? 0.80f : (atk.Launch ? 0.5f : 0.35f));
                target.Airborne = keepAir;
                // Being hit cleanly breaks the stance: without this a stunned fighter
                // still counts as guarding, and the next hit of the string reads as
                // blocked by someone who is being knocked across the harbor.
                if (!keepAir) target.OpenUpUntil(target.StunUntil);
                if (floored)
                {
                    target.JuggleCount = 0;
                    target.InvulnUntil = target.StunUntil;   // wake-up i-frames, as on a juggle end
                }
                if (!keepAir && wasAirborne)
                {
                    // Juggle ended (knockdown): grant wake-up i-frames lasting the
                    // whole knockdown so an immediate relaunch whiffs — bounds the
                    // juggle and prevents a deterministic touch-of-death loop.
                    target.JuggleCount = 0;
                    target.InvulnUntil = target.StunUntil;
                }
                target.Anim?.Play("hit", 0.05f);
                target.Proc?.Play(ProcAnim.Move.Hit);
                target.Proc?.Contact();          // recoil lands on the frozen frame, not after it
                if (atk.Knockback > 0f)
                {
                    float dir = attacker.FacingRight ? 1f : -1f;
                    SetX(target, target.transform.position.x + dir * atk.Knockback);
                }
                SpawnHitVfx(impact, atk.Vfx);                  // spark + blood + special
                if (atk.Fx == FxWeight.Heavy || atk.Fx == FxWeight.Special)
                    SpawnFx("impact_ring", "hit_spark", impact, 1.6f, Color.white);
            }

            target.Hp = Mathf.Max(0f, target.Hp - damage);
            // Specials grant reduced meter (0.25x) so a normal-xx-special confirm is
            // meter-negative — specials stay a resource earned from neutral, not
            // self-funded pressure (D-015 review fix).
            attacker.GainMeter(IsSpecial(atk) ? damage * 0.25f : damage);   // ~1 segment per 150 damage dealt (normals)
            target.GainMeter(damage * 150f / 80f * 0.5f);  // taken damage charges faster (halved: ~1 seg / 160 taken)
            TouchUI.I?.RefreshBars();

            if (target.Hp <= 0f && !target.Dead)
            {
                target.Dead = true;
                target.Airborne = false;
                target.Anim?.Play("death", 0.1f);
                CombatFx.Shake(0.28f, 0.5f);               // KO owns its longer freeze in RoundManager
                CombatFx.Punch(0.6f, 0.3f);
                RoundManager.I?.OnKo(attacker, target);
                return !blocked;
            }

            if (!blocked)
            {
                ApplyImpactFx(atk.Fx);
                TouchUI.I?.OnHitLanded(attacker);
            }
            return !blocked;
        }

        static bool IsSpecial(Attack atk) => atk.Vfx != null;

        // Attacks drive the attacker forward, never crossing a minimum gap. The gap
        // scales with both bodies (D-023) so a step-in can't shove a fighter inside
        // an opponent the push-out pass would only have to eject again.
        static void StepIn(Fighter attacker, Fighter target, float amount)
        {
            float dir = attacker.FacingRight ? 1f : -1f;
            float minGap = MinSeparation(attacker, target) + 0.10f;
            float desired = attacker.transform.position.x + dir * amount;
            if (dir > 0f) desired = Mathf.Min(desired, target.transform.position.x - minGap);
            else desired = Mathf.Max(desired, target.transform.position.x + minGap);
            SetX(attacker, desired);
        }

        // Where a connecting strike reads as touching: the near surface of the
        // target's body, at a height the move implies, following a juggled body up.
        // (It used to be the target's centre line at a flat 1.1 m — belly height on
        // Tengi, and behind the surface the fist actually reached.)
        static Vector3 ImpactPoint(Fighter attacker, Fighter target, Attack atk)
        {
            float dir = attacker.FacingRight ? 1f : -1f;
            float x = target.transform.position.x - dir * target.HurtDepth * 0.85f;
            float y = atk.Low ? target.ChestY * 0.42f
                    : atk.Overhead ? target.ChestY * 1.28f      // lands on the head/shoulders
                    : atk.Launch ? target.ChestY * 1.08f
                    : atk.Grab ? target.ChestY * 0.88f          // seized around the body
                    : target.ChestY;
            return new Vector3(x, y + target.Lift, 0f);
        }

        // Impact "juice" per hit weight (D-015): hit-stop bite + camera shake/punch.
        static void ApplyImpactFx(FxWeight w)
        {
            switch (w)
            {
                case FxWeight.Light:   CombatFx.HitStop(CombatFx.StopLight); break;
                case FxWeight.Medium:  CombatFx.HitStop(CombatFx.StopMedium);  CombatFx.Shake(0.06f, 0.18f); break;
                case FxWeight.Heavy:   CombatFx.HitStop(CombatFx.StopHeavy);   CombatFx.Shake(0.14f, 0.28f); CombatFx.Punch(0.40f, 0.22f); break;
                case FxWeight.Launch:  CombatFx.HitStop(CombatFx.StopLaunch);  CombatFx.Shake(0.08f, 0.20f); CombatFx.Punch(0.22f, 0.20f); break;
                case FxWeight.Special: CombatFx.HitStop(CombatFx.StopSpecial); CombatFx.Shake(0.18f, 0.34f); CombatFx.Punch(0.50f, 0.28f); break;
                case FxWeight.Parry:   CombatFx.HitStop(CombatFx.StopParry);   CombatFx.Shake(0.10f, 0.22f); break;
            }
        }

        // Clean hit: white spark, ink blood, and any special overlay.
        public static void SpawnHitVfx(Vector3 pos, string extraVfx)
        {
            Spawn("hit_spark", pos, 0.9f);
            Spawn("ink_blood", pos + new Vector3(0.15f, 0.1f, 0f), 0.7f);
            if (extraVfx != null) Spawn(extraVfx, pos, 1.4f);
        }

        // Block: a small Ash Steel deflect spark and NO blood, so a guarded hit
        // never reads as a clean hit (Pillar 4: violence must communicate).
        public static void SpawnBlockVfx(Vector3 pos)
        {
            Spawn("hit_spark", pos, 0.55f, AssetLib.AshSteel);
        }

        // Whiff: a faint smear where this fighter's knuckles actually stopped, so a
        // miss shows the player the gap they misjudged rather than a generic puff.
        public static void SpawnWhiffVfx(Fighter attacker)
        {
            float dir = attacker.FacingRight ? 1f : -1f;
            var pos = attacker.transform.position + new Vector3(dir * attacker.ArmReach, attacker.ChestY, 0f);
            Spawn("hit_spark", pos, 0.4f, new Color(0.9f, 0.9f, 0.9f, 0.35f));
        }

        // Dash / afterimage streak in the fighter's accent tint.
        public static void SpawnDash(Fighter f)
        {
            float dir = f.FacingRight ? 1f : -1f;
            var pos = f.transform.position + new Vector3(-dir * 0.4f, f.ChestY * 0.78f, 0f);
            SpawnFx("dash_streak", "hit_spark", pos, 1.1f, f.Theme);
        }

        // Spawn a preferred sprite if it exists, else a guaranteed fallback (so new
        // move VFX degrade gracefully until the generated sprite is synced in).
        public static void SpawnFx(string preferred, string fallback, Vector3 pos, float scale, Color tint)
        {
            string use = AssetLib.Has("vfx/" + preferred) ? preferred : fallback;
            Spawn(use, pos, scale, tint);
        }

        // Resolved VFX sprites cached by name so the hit frame never rebuilds the
        // "vfx/"+name key or re-creates the Sprite (mobile budget).
        static readonly Dictionary<string, Sprite> vfxSprites = new Dictionary<string, Sprite>();

        static Sprite VfxSprite(string name)
        {
            if (!vfxSprites.TryGetValue(name, out var s)) { s = AssetLib.Sprite("vfx/" + name, 256f); vfxSprites[name] = s; }
            return s;
        }

        public static void Spawn(string sprite, Vector3 pos, float scale) => Spawn(sprite, pos, scale, Color.white);

        public static void Spawn(string sprite, Vector3 pos, float scale, Color tint)
        {
            var s = VfxSprite(sprite);
            if (s == null) return;
            VfxFade.Get().Init(s, new Vector3(pos.x, pos.y, -0.5f), scale, tint);
        }
    }

    // Pooled one-shot sprite: quick scale-up and fade-out, then back to the pool —
    // no per-hit GameObject alloc/Destroy (mobile GC budget). Holds during hit-stop
    // so impact bursts freeze with the fighters.
    public class VfxFade : MonoBehaviour
    {
        static readonly Stack<VfxFade> pool = new Stack<VfxFade>();

        SpriteRenderer sr;
        float t;
        Color baseColor = Color.white;
        bool live;

        public static VfxFade Get()
        {
            while (pool.Count > 0)
            {
                var f = pool.Pop();
                if (f != null) return f;          // skip any (Unity-)destroyed entries
            }
            var go = new GameObject("vfx", typeof(SpriteRenderer));
            var v = go.AddComponent<VfxFade>();
            v.sr = go.GetComponent<SpriteRenderer>();
            v.sr.sortingOrder = 40;
            return v;
        }

        public void Init(Sprite sprite, Vector3 pos, float scale, Color tint)
        {
            sr.sprite = sprite;
            sr.color = tint;
            baseColor = tint;
            transform.position = pos;
            transform.localScale = Vector3.one * scale;
            t = 0f;
            live = true;
            gameObject.SetActive(true);
        }

        void Update()
        {
            if (!live || CombatFx.Frozen) return;   // freeze the burst with the fighters
            t += Time.deltaTime;
            float life = 0.35f;
            transform.localScale *= 1f + Time.deltaTime * 1.6f;
            if (sr != null) sr.color = new Color(baseColor.r, baseColor.g, baseColor.b, baseColor.a * Mathf.Clamp01(1f - t / life));
            if (t >= life)
            {
                live = false;
                gameObject.SetActive(false);
                pool.Push(this);
            }
        }
    }
}
