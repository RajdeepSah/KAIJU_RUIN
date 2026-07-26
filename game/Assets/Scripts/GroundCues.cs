using System.Collections.Generic;
using UnityEngine;

namespace KaijuRuin
{
    // Distance-reading aids drawn on the fight plane (D-023). Three jobs:
    //
    //  1. A soft contact shadow under each fighter. The stage is layered sprites and
    //     the ground strip cannot receive a real shadow, so a rigged 3D silhouette
    //     read as pasted in FRONT of the harbor rather than standing on it — and with
    //     nothing anchoring the feet, the gap between two fighters was guesswork.
    //  2. A reach guide for the local player: a two-tone ground band running from the
    //     front of their body out to their jab reach, then on to their heavy reach,
    //     each segment lighting when the opponent is inside it. Reading the gap
    //     becomes glancing at the floor instead of estimating between two silhouettes.
    //  3. A debug range overlay (F2) that marks every reach boundary for BOTH
    //     fighters, so hitbox-vs-visual can be checked on device at point-blank, mid
    //     and max-whiff range instead of inferred.
    //
    // Render-only: reads the sim, never writes it. RoundManager ticks this straight
    // after the body-separation pass so a mark can never disagree with the sim it
    // came from. All sprites are generated in code (no manifest assets).
    public class GroundCues : MonoBehaviour
    {
        public static bool ShowDebugRanges;

        // The reach guide is a readability aid, not a rule of the game — one flag so
        // "is this too much hand-holding?" is answerable without a code change.
        public static bool ShowReachGuide = true;

        // Guide bands sit just above the fight plane, over the ground strip
        // (sortingOrder 10) and under the wade splashes (12).
        const int Order = 11;
        const float PlaneY = 0.02f;
        const float BandDepth = 0.55f;      // how deep on Z a ground mark is drawn
        const float TickWidth = 0.035f;

        Fighter local, foe;

        Transform localShadow, foeShadow;
        SpriteRenderer localShadowSr, foeShadowSr;
        SpriteRenderer pokeBand, swingBand;
        readonly List<SpriteRenderer> debugTicks = new List<SpriteRenderer>();

        static Sprite blob, solid;

        public void Build(Fighter localFighter, Fighter opponent)
        {
            local = localFighter;
            foe = opponent;

            localShadowSr = Shadow("ShadowLocal", out localShadow);
            foeShadowSr = Shadow("ShadowFoe", out foeShadow);

            // Outer segment first so the inner (jab) one draws over it.
            swingBand = Band("SwingBand", AssetLib.AshSteel, Order);
            pokeBand = Band("PokeBand", AssetLib.AshSteel, Order + 1);
        }

        SpriteRenderer Shadow(string name, out Transform t)
        {
            var go = new GameObject(name, typeof(SpriteRenderer));
            go.transform.SetParent(transform, false);
            go.transform.rotation = Quaternion.Euler(90f, 0f, 0f);   // lie flat on the plane
            var sr = go.GetComponent<SpriteRenderer>();
            sr.sprite = Blob();
            sr.color = new Color(AssetLib.SumiInk.r, AssetLib.SumiInk.g, AssetLib.SumiInk.b, 0.5f);
            sr.sortingOrder = Order;
            t = go.transform;
            return sr;
        }

        SpriteRenderer Band(string name, Color tint, int order)
        {
            var go = new GameObject(name, typeof(SpriteRenderer));
            go.transform.SetParent(transform, false);
            go.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
            var sr = go.GetComponent<SpriteRenderer>();
            sr.sprite = Solid();
            sr.color = new Color(tint.r, tint.g, tint.b, 0f);
            sr.sortingOrder = order;
            return sr;
        }

        // Called by RoundManager every frame, after CombatSystem.Separate.
        public void Tick()
        {
            if (Input.GetKeyDown(KeyCode.F2)) ShowDebugRanges = !ShowDebugRanges;
            if (local == null || foe == null) return;

            PlaceShadow(localShadow, localShadowSr, local);
            PlaceShadow(foeShadow, foeShadowSr, foe);
            PlaceReachGuide();
            PlaceDebugTicks();
        }

        // The shadow tracks the sim X (not the animated visual), tightens and fades
        // as a fighter is lifted off the ground, and vanishes with a dead body.
        void PlaceShadow(Transform t, SpriteRenderer sr, Fighter f)
        {
            if (t == null || sr == null) return;
            bool show = !f.Dead && !GameManager.Paused;
            if (sr.enabled != show) sr.enabled = show;
            if (!show) return;

            float lift = f.Lift;
            float width = f.HurtDepth * 3.1f / (1f + lift * 0.7f);
            t.position = new Vector3(f.transform.position.x, PlaneY, 0f);
            // The camera looks along the plane from only ~13 degrees up, so the depth
            // axis is foreshortened to roughly a fifth; a near-round footprint is
            // what reads as an ellipse on screen.
            t.localScale = new Vector3(width, width * 0.85f, 1f);
            var c = sr.color;
            c.a = 0.5f / (1f + lift * 1.4f);
            sr.color = c;
        }

        // Two segments in front of the local player: body edge -> jab reach, then
        // jab reach -> heavy reach. Each lights when the opponent's body is inside
        // it, so "can I touch them from here" is answered on the floor.
        void PlaceReachGuide()
        {
            bool show = ShowReachGuide && !RoundManager.RoundFrozen && !GameManager.Paused
                        && !local.Dead && !foe.Dead;
            if (pokeBand.enabled != show) { pokeBand.enabled = show; swingBand.enabled = show; }
            if (!show) return;

            float dir = local.FacingRight ? 1f : -1f;
            float from = local.HurtDepth;
            float pokeTo = CombatSystem.StrikeExtent(local, CombatSystem.Jab);
            float swingTo = CombatSystem.StrikeExtent(local, CombatSystem.LongestNormal);
            float gap = local.DistanceTo(foe);

            bool inPoke = gap <= CombatSystem.EffectiveReach(local, foe, CombatSystem.Jab);
            bool inSwing = gap <= CombatSystem.EffectiveReach(local, foe, CombatSystem.LongestNormal);

            Segment(pokeBand, dir, from, pokeTo, inPoke ? AssetLib.GoryoFlame : AssetLib.AshSteel, inPoke ? 0.30f : 0.10f);
            Segment(swingBand, dir, pokeTo, swingTo,
                    inSwing && !inPoke ? AssetLib.SignalAmber : AssetLib.AshSteel,
                    inSwing && !inPoke ? 0.22f : 0.07f);
        }

        // Draw a flat band from `from` to `to` metres ahead of the local player.
        void Segment(SpriteRenderer sr, float dir, float from, float to, Color tint, float alpha)
        {
            float len = Mathf.Max(0.02f, to - from);
            float mid = local.transform.position.x + dir * (from + len * 0.5f);
            sr.transform.position = new Vector3(mid, PlaneY, 0f);
            sr.transform.localScale = new Vector3(len, BandDepth, 1f);
            sr.color = new Color(tint.r, tint.g, tint.b, alpha);
        }

        // ---- Debug overlay (F2) ---------------------------------------------
        // One tick per boundary that decides a hit, for both fighters, inner to
        // outer: push box (bone), hurt box (steel), GRAB (white, D-024), jab
        // (flame), tail sweep (amber), longest normal (blood). If the ticks and the
        // silhouettes disagree on device, the CharacterDef body metrics are what to
        // correct — not the per-move reaches, which are anatomy (D-023).
        void PlaceDebugTicks()
        {
            int used = 0;
            if (ShowDebugRanges && !GameManager.Paused)
            {
                used += FighterTicks(local, used);
                used += FighterTicks(foe, used);
            }
            for (int i = used; i < debugTicks.Count; i++)
                if (debugTicks[i].enabled) debugTicks[i].enabled = false;
        }

        int FighterTicks(Fighter f, int start)
        {
            int n = 0;
            float dir = f.FacingRight ? 1f : -1f;
            n += Tick(start + n, f, dir, f.PushDepth, AssetLib.BonePaper);
            n += Tick(start + n, f, dir, f.HurtDepth, AssetLib.AshSteel);
            // Grab range (D-024) sits between the push box and the jab: it is the
            // boundary a player most needs to feel, since guard cannot answer inside it.
            n += Tick(start + n, f, dir, CombatSystem.StrikeExtent(f, CombatSystem.Grab), Color.white);
            n += Tick(start + n, f, dir, CombatSystem.StrikeExtent(f, CombatSystem.Jab), AssetLib.GoryoFlame);
            n += Tick(start + n, f, dir, CombatSystem.StrikeExtent(f, CombatSystem.Sweep), AssetLib.SignalAmber);
            n += Tick(start + n, f, dir, CombatSystem.StrikeExtent(f, CombatSystem.LongestNormal), AssetLib.BloodSeal);
            return n;
        }

        int Tick(int index, Fighter f, float dir, float extent, Color tint)
        {
            while (debugTicks.Count <= index)
            {
                var sr = Band("DebugTick" + debugTicks.Count, Color.white, Order + 2);
                debugTicks.Add(sr);
            }
            var t = debugTicks[index];
            if (!t.enabled) t.enabled = true;
            t.transform.position = new Vector3(f.transform.position.x + dir * extent, PlaneY, 0f);
            t.transform.localScale = new Vector3(TickWidth, BandDepth * 1.6f, 1f);
            t.color = new Color(tint.r, tint.g, tint.b, 0.85f);
            return 1;
        }

        // ---- Procedural sprites ---------------------------------------------
        // pixelsPerUnit == texture size, so each sprite is exactly 1x1 world units
        // and localScale reads directly in metres.

        static Sprite Blob()
        {
            if (blob != null) return blob;
            const int n = 64;
            var t = new Texture2D(n, n, TextureFormat.RGBA32, false) { wrapMode = TextureWrapMode.Clamp };
            var px = new Color[n * n];
            float r = (n - 1) * 0.5f;
            for (int y = 0; y < n; y++)
                for (int x = 0; x < n; x++)
                {
                    float d = Mathf.Sqrt((x - r) * (x - r) + (y - r) * (y - r)) / r;
                    float a = Mathf.Clamp01(1f - d);
                    a = a * a * (3f - 2f * a);          // smooth falloff, no hard rim
                    px[y * n + x] = new Color(1f, 1f, 1f, a);
                }
            t.SetPixels(px);
            t.Apply();
            blob = Sprite.Create(t, new Rect(0, 0, n, n), new Vector2(0.5f, 0.5f), n);
            return blob;
        }

        static Sprite Solid()
        {
            if (solid != null) return solid;
            const int n = 8;
            var t = new Texture2D(n, n, TextureFormat.RGBA32, false) { wrapMode = TextureWrapMode.Clamp };
            var px = new Color[n * n];
            for (int i = 0; i < px.Length; i++) px[i] = Color.white;
            t.SetPixels(px);
            t.Apply();
            solid = Sprite.Create(t, new Rect(0, 0, n, n), new Vector2(0.5f, 0.5f), n);
            return solid;
        }
    }
}
