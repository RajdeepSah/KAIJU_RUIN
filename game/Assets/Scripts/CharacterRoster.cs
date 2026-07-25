using System.Collections.Generic;
using UnityEngine;

namespace KaijuRuin
{
    // Data-driven fighter definition. Adding a playable champion = add ONE
    // CharacterDef to CharacterRoster.All — no menu, spawn, HUD, or combat code
    // changes (the whole point of D-017's "scalable roster"). Every per-character
    // value the fight code used to hardcode lives here now.
    public class CharacterDef
    {
        public string Id;                 // stable key; matches the asset IDs (e.g. "kest")
        public string DisplayName;        // HUD + VS-screen name
        public string ModelGlb;           // rigged mesh in StreamingAssets/Models
        public string PortraitPath;       // Resources/Art path (no extension) for select + VS
        public string AnimClipPrefix;     // clip-GLB name prefix, e.g. "kest_anim_" (shared rig per D-011)
        public Color Theme;               // accent tint for this fighter's move VFX
        public string SpecialSet;         // which CombatSystem special set drives the cards ("kest"/"tengi")
        public string IconKey;            // UI ability-icon slice prefix ("icon_kest")

        // Feel knobs (were per-fighter literals in RoundManager before D-017).
        public float AttackSpeed = 1f;    // scales attack recovery
        public float WalkSpeed = 3.0f;
        public float ProcAmp = 1.0f;      // procedural body-motion amplitude
        public float ProcDur = 1.0f;      // procedural body-motion duration

        // ---- Body metrics, measured off the rigged GLB bind pose (D-023) -----
        // Metres on the sim (facing) axis. CombatSystem's distance model is stated
        // RELATIVE to Kest, so these are what make a move's listed reach mean the
        // same thing visually on a 1.8 m werefox and a 2.4 m culler. Defaults =
        // Kest, i.e. a champion that omits them inherits the baseline body.
        public float ArmReach = 0.78f;    // root centre -> knuckles, arm extended forward
        public float HurtDepth = 0.32f;   // half body depth: what an incoming strike must touch
        public float PushDepth = 0.28f;   // half push box: two bodies never close past a+b
        public float ChestY = 1.23f;      // impact height (hit sparks, blood)
        public float ModelHeight = 1.80f; // silhouette height (shadow size, airborne lift)

        public string Tagline = "";       // one-line blurb for the select screen
    }

    // The playable roster. New champions drop in here; their portrait/model/anim
    // assets are tracked as `planned` manifest rows until generated (ART_DIRECTION §4).
    public static class CharacterRoster
    {
        public static readonly List<CharacterDef> All = new List<CharacterDef>
        {
            new CharacterDef {
                Id = "kest", DisplayName = "KEST", ModelGlb = "kest_model.glb",
                PortraitPath = "characters/kest_portrait", AnimClipPrefix = "kest_anim_",
                Theme = AssetLib.GoryoFlame, SpecialSet = "kest", IconKey = "icon_kest",
                AttackSpeed = 1.12f, WalkSpeed = 3.1f, ProcAmp = 1.0f, ProcDur = 0.9f,
                // kest_model.glb: 1.80 m tall, shoulder 1.40, upper arm 0.280 +
                // forearm 0.271 + fist ~0.09 off a shoulder sitting 0.135 forward
                // => 0.78 m of knuckle reach; chest/torso half-depth 0.32.
                ArmReach = 0.78f, HurtDepth = 0.32f, PushDepth = 0.28f, ChestY = 1.23f, ModelHeight = 1.80f,
                Tagline = "Agile werefox — rushdown & fox-fire",
            },
            new CharacterDef {
                Id = "tengi", DisplayName = "TENGI", ModelGlb = "tengi_model.glb",
                // Tengi reuses Kest's rig clips (shared Meshy skeleton, D-011 slice rule).
                PortraitPath = "characters/tengi_portrait", AnimClipPrefix = "kest_anim_",
                Theme = AssetLib.BloodSeal, SpecialSet = "tengi", IconKey = "icon_tengi",
                AttackSpeed = 0.92f, WalkSpeed = 2.8f, ProcAmp = 1.12f, ProcDur = 1.05f,
                // tengi_model.glb: 2.40 m tall, shoulder 1.81, upper arm 0.439 +
                // forearm 0.461 + fist ~0.12 off a shoulder 0.116 forward => 1.14 m
                // of knuckle reach. Body depth runs to 0.56 at the cloak silhouette
                // but 99% of the mesh is inside 0.31, so the hurt box takes 0.46 (the
                // read silhouette) and the push box 0.36 (solid body). His "reach and
                // punish" tagline is now literally true instead of cosmetic.
                ArmReach = 1.14f, HurtDepth = 0.46f, PushDepth = 0.36f, ChestY = 1.57f, ModelHeight = 2.40f,
                Tagline = "Heavy culler — reach & punish",
            },
        };

        public static int Count => All.Count;

        public static CharacterDef Get(string id)
        {
            foreach (var c in All) if (c.Id == id) return c;
            return All[0];
        }

        public static int IndexOf(string id)
        {
            for (int i = 0; i < All.Count; i++) if (All[i].Id == id) return i;
            return 0;
        }

        // Default opponent = the next champion in the roster (wraps). Used to seed
        // the select screen and the local-matchmaking stand-in.
        public static string Other(string id)
        {
            int i = IndexOf(id);
            return All[(i + 1) % All.Count].Id;
        }

        // Deterministic "random" pick for the local quick-match sim (menu context,
        // so UnityEngine.Random is fine — this never touches the deterministic fight sim).
        public static string RandomId()
        {
            return All[Random.Range(0, All.Count)].Id;
        }
    }
}
