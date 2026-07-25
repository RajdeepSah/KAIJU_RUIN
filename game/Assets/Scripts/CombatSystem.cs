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
            public bool Low;        // beats standing block (sweep)
            public bool Launch;     // pops / keeps the target airborne
            public FxWeight Fx;     // hit-stop + shake profile
            public string Vfx;      // sprite under Resources/Art/vfx, optional (marks a special)
            public string Sfx;
        }

        const float ParryWindow = 0.16f;   // block within this of its start = perfect guard
        const int MaxJuggle = 4;           // air hits before a forced knockdown

        // Universal normals (recoveries trimmed from v1 for a faster fight, D-015).
        public static readonly Attack Jab      = new Attack { Name = "Jab",      Damage = 40,  Reach = 1.1f, Recovery = 0.20f, StepIn = 0.06f, Fx = FxWeight.Light,  Sfx = "hit_light" };
        public static readonly Attack Cross    = new Attack { Name = "Cross",    Damage = 50,  Reach = 1.1f, Recovery = 0.20f, StepIn = 0.08f, Fx = FxWeight.Light,  Sfx = "hit_light" };
        public static readonly Attack Finisher = new Attack { Name = "Finisher", Damage = 70,  Reach = 1.1f, Recovery = 0.38f, Knockback = 0.6f, StepIn = 0.12f, Fx = FxWeight.Medium, Sfx = "hit_heavy" };
        public static readonly Attack Heavy    = new Attack { Name = "Heavy",    Damage = 120, Reach = 1.6f, Recovery = 0.62f, Knockback = 1.5f, StepIn = 0.22f, Fx = FxWeight.Heavy,  Sfx = "hit_heavy" };
        public static readonly Attack Launcher = new Attack { Name = "Launcher", Damage = 90,  Reach = 1.35f,Recovery = 0.50f, StepIn = 0.12f, Launch = true, Fx = FxWeight.Launch, Sfx = "hit_heavy" };
        public static readonly Attack Sweep    = new Attack { Name = "Sweep",    Damage = 80,  Reach = 1.35f,Recovery = 0.50f, StepIn = 0.20f, Low = true,    Fx = FxWeight.Medium, Sfx = "hit_light" };

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

        // Returns true when the attack connected (clean, unblocked).
        public static bool Resolve(Fighter attacker, Attack atk)
        {
            // Recovery scaled by the attacker's cadence (per-character + global speed-up).
            float recovery = atk.Recovery / Mathf.Max(0.5f, attacker.AttackSpeed);
            attacker.AttackLockUntil = Time.time + recovery;

            var target = attacker.Opponent;
            if (target == null || target.Dead) return false;

            // Reach is checked against the PRE-step-in distance so listed ranges hold
            // and whiff-punish still works; step-in is a follow-through on connect only.
            if (attacker.DistanceTo(target) > atk.Reach)
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

            var impact = target.transform.position + Vector3.up * 1.1f;
            bool blocked = target.Blocking && !atk.Low && !target.Airborne;
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
                target.StunUntil = Time.time + (keepAir ? 0.7f : (atk.Launch ? 0.5f : 0.35f));
                target.Airborne = keepAir;
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
                if (atk.Knockback > 0f)
                {
                    float dir = attacker.FacingRight ? 1f : -1f;
                    var p = target.transform.position;
                    p.x = Mathf.Clamp(p.x + dir * atk.Knockback, -6f, 6f);
                    target.transform.position = p;
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

        // Attacks drive the attacker forward, never crossing a minimum gap.
        static void StepIn(Fighter attacker, Fighter target, float amount)
        {
            float dir = attacker.FacingRight ? 1f : -1f;
            const float minGap = 0.9f;
            var p = attacker.transform.position;
            float desired = p.x + dir * amount;
            if (dir > 0f) desired = Mathf.Min(desired, target.transform.position.x - minGap);
            else desired = Mathf.Max(desired, target.transform.position.x + minGap);
            p.x = Mathf.Clamp(desired, -6f, 6f);
            attacker.transform.position = p;
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

        // Whiff: a faint smear at the attacker so a miss is visible, not silent.
        public static void SpawnWhiffVfx(Fighter attacker)
        {
            float dir = attacker.FacingRight ? 1f : -1f;
            var pos = attacker.transform.position + new Vector3(dir * 0.8f, 1.1f, 0f);
            Spawn("hit_spark", pos, 0.4f, new Color(0.9f, 0.9f, 0.9f, 0.35f));
        }

        // Dash / afterimage streak in the fighter's accent tint.
        public static void SpawnDash(Fighter f)
        {
            float dir = f.FacingRight ? 1f : -1f;
            var pos = f.transform.position + new Vector3(-dir * 0.4f, 0.95f, 0f);
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
