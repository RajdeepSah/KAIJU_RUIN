using UnityEngine;

namespace KaijuRuin
{
    // Deterministic hit resolution on the X axis. All numbers from DESIGN_BRIEF.md.
    public static class CombatSystem
    {
        public struct Attack
        {
            public string Name;
            public float Damage;
            public float Reach;
            public float Recovery;
            public float Knockback;
            public bool Low;        // beats standing block (sweep)
            public bool Launch;
            public string Vfx;      // sprite under Resources/Art/vfx, optional
            public string Sfx;
        }

        public static readonly Attack Jab      = new Attack { Name = "Jab",      Damage = 40,  Reach = 1.1f, Recovery = 0.25f, Sfx = "hit_light" };
        public static readonly Attack Cross    = new Attack { Name = "Cross",    Damage = 50,  Reach = 1.1f, Recovery = 0.25f, Sfx = "hit_light" };
        public static readonly Attack Finisher = new Attack { Name = "Finisher", Damage = 70,  Reach = 1.1f, Recovery = 0.5f,  Knockback = 0.6f, Sfx = "hit_heavy" };
        public static readonly Attack Heavy    = new Attack { Name = "Heavy",    Damage = 120, Reach = 1.6f, Recovery = 0.8f,  Knockback = 1.5f, Sfx = "hit_heavy" };
        public static readonly Attack Launcher = new Attack { Name = "Launcher", Damage = 90,  Reach = 1.3f, Recovery = 0.6f,  Launch = true,    Sfx = "hit_heavy" };
        public static readonly Attack Sweep    = new Attack { Name = "Sweep",    Damage = 80,  Reach = 1.3f, Recovery = 0.6f,  Low = true,       Sfx = "hit_light" };

        public static Attack KestS1  => new Attack { Name = "Fox-fire Dash",  Damage = 100, Reach = 3.2f, Recovery = 0.5f, Knockback = 1.0f, Vfx = "kest_foxfire",     Sfx = "kest_special" };
        public static Attack KestS2  => new Attack { Name = "Phantom Rake",   Damage = 160, Reach = 1.4f, Recovery = 0.7f, Knockback = 0.8f, Vfx = "kest_foxfire",     Sfx = "kest_special" };
        public static Attack KestS3  => new Attack { Name = "Hunt of Shadows",Damage = 280, Reach = 2.2f, Recovery = 1.0f, Knockback = 2.0f, Vfx = "kest_foxfire",     Sfx = "kest_special" };
        public static Attack TengiS1 => new Attack { Name = "Crow Wall",      Damage = 130, Reach = 1.4f, Recovery = 0.6f, Knockback = 1.0f, Vfx = "tengi_bladewave",  Sfx = "tengi_special" };
        public static Attack TengiS2 => new Attack { Name = "Culling Arc",    Damage = 180, Reach = 3.0f, Recovery = 0.8f, Knockback = 1.5f, Vfx = "tengi_bladewave",  Sfx = "tengi_special" };
        public static Attack TengiS3 => new Attack { Name = "Black Sun",      Damage = 300, Reach = 1.8f, Recovery = 1.4f, Knockback = 2.2f, Vfx = "tengi_bladewave",  Sfx = "tengi_special" };

        // Returns true when the attack connected.
        public static bool Resolve(Fighter attacker, Attack atk)
        {
            attacker.AttackLockUntil = Time.time + atk.Recovery;
            var target = attacker.Opponent;
            if (target == null || target.Dead) return false;
            if (attacker.DistanceTo(target) > atk.Reach)
            {
                // Whiff: readable answer so "I missed and I'm exposed" (Tengi's
                // PUNISH keys off recovery) is felt, not inferred from silence.
                AudioManager.I?.Sfx("whiff", 0.4f);   // soft-fails until the clip exists
                SpawnWhiffVfx(attacker);
                return false;
            }

            bool blocked = target.Blocking && !atk.Low && !target.Airborne;
            float damage = atk.Damage;
            var impact = target.transform.position + Vector3.up * 1.1f;
            if (blocked)
            {
                damage *= IsSpecial(atk) ? 0.10f : 0.25f;   // 75% reduction, 10% chip on specials
                AudioManager.I?.Sfx("block");
                target.Anim?.Play("block", 0.05f);
                SpawnBlockVfx(impact);                       // deflect spark, no blood
            }
            else
            {
                AudioManager.I?.Sfx(atk.Sfx);
                target.StunUntil = Time.time + (atk.Launch ? 0.7f : 0.35f);
                target.Airborne = atk.Launch;
                target.Anim?.Play("hit", 0.05f);
                if (atk.Knockback > 0f)
                {
                    float dir = attacker.FacingRight ? 1f : -1f;
                    var p = target.transform.position;
                    p.x = Mathf.Clamp(p.x + dir * atk.Knockback, -6f, 6f);
                    target.transform.position = p;
                }
                SpawnHitVfx(impact, atk.Vfx);                // spark + blood + special
            }

            target.Hp = Mathf.Max(0f, target.Hp - damage);
            attacker.GainMeter(damage);                    // one segment per 150 damage dealt
            target.GainMeter(damage * 150f / 80f * 0.5f);  // taken damage charges faster (segment per ~80... halved: net segment per 160 taken)
            TouchUI.I?.RefreshBars();

            if (target.Hp <= 0f && !target.Dead)
            {
                target.Dead = true;
                target.Anim?.Play("death", 0.1f);
                RoundManager.I?.OnKo(attacker, target);
            }
            return !blocked;
        }

        static bool IsSpecial(Attack atk) => atk.Vfx != null;

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

        public static void Spawn(string sprite, Vector3 pos, float scale) => Spawn(sprite, pos, scale, Color.white);

        public static void Spawn(string sprite, Vector3 pos, float scale, Color tint)
        {
            var s = AssetLib.Sprite("vfx/" + sprite, 256f);
            if (s == null) return;
            var go = new GameObject("vfx_" + sprite, typeof(SpriteRenderer));
            var sr = go.GetComponent<SpriteRenderer>();
            sr.sprite = s;
            sr.color = tint;
            sr.sortingOrder = 40;
            go.transform.position = new Vector3(pos.x, pos.y, -0.5f);
            go.transform.localScale = Vector3.one * scale;
            go.AddComponent<VfxFade>();
        }
    }

    // One-shot sprite: quick scale-up and fade-out, then destroy.
    public class VfxFade : MonoBehaviour
    {
        float t;
        SpriteRenderer sr;
        Color baseColor = Color.white;
        void Awake() { sr = GetComponent<SpriteRenderer>(); if (sr != null) baseColor = sr.color; }
        void Update()
        {
            t += Time.deltaTime;
            float life = 0.35f;
            transform.localScale *= 1f + Time.deltaTime * 1.6f;
            if (sr != null) sr.color = new Color(baseColor.r, baseColor.g, baseColor.b, baseColor.a * Mathf.Clamp01(1f - t / life));
            if (t >= life) Destroy(gameObject);
        }
    }
}
