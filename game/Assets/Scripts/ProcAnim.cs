using UnityEngine;

namespace KaijuRuin
{
    // Procedural per-move body motion, layered on top of the glTFast clip.
    //
    // The slice ships 6 shared clips (idle/walk/punch/block/hit/death) retargeted
    // to both champions, so every attack otherwise reads as the same punch. This
    // component gives each move a distinct silhouette WITHOUT new generated clips
    // (owner directive 2026-07-19, D-015): a short envelope drives a local offset
    // on the rig's "Visual" child — lunge, uppercut rise, low sweep crouch,
    // back-hop, air reach, character-flavoured special flourish.
    //
    // Session 9 (D-023) adds two distance-reading duties: an airborne lift, so a
    // juggled fighter visibly leaves the ground instead of being hit by air moves
    // while apparently standing, and Contact(), which snaps a gesture to its peak
    // on the frame a strike resolves so the hit-stop freeze frame shows the pose
    // the move was drawn to land in.
    //
    // Invariant (ARCHITECTURE deterministic-sim contract): this animates the
    // "Visual" CHILD transform only. The root's X position — the sole sim truth
    // for range/hit resolution — is never touched here.
    public class ProcAnim : MonoBehaviour
    {
        public enum Move
        {
            Jab, Cross, Finisher, Heavy, Launcher, Sweep,
            BackHop, AirRake, AirSlam, SpecialKest, SpecialTengi, Parry, Hit
        }

        struct Gesture
        {
            public float dur;
            public Vector3 pos;    // peak local offset (m); +x = forward, +y = up
            public Vector3 rot;    // peak local euler (deg) layered on the base facing
            public float squash;   // +down/widen (crouch), -tall/narrow (rise)
            public float peak;     // normalized time of the amplitude peak
        }

        Transform visual;
        bool captured;
        Vector3 basePos;
        Quaternion baseRot;
        Vector3 baseScale;
        Fighter owner;

        // Per-character feel (set by RoundManager): amplitude and duration scaling.
        public float AmpMul = 1f;
        public float DurMul = 1f;

        // Airborne lift (set by RoundManager from the model height). A juggled
        // fighter has to LOOK off the ground or the air follow-ups' longer reach
        // reads as a phantom hit on someone standing right there — and the shadow
        // needs something to separate from. Still visual-only: the sim's X is the
        // only truth for range, and lift never touches it.
        public float AirLift = 0.54f;
        public float Lift { get; private set; }

        bool active;
        float t;
        Gesture cur;

        void LateUpdate()
        {
            if (!Capture()) return;

            // Hit-stop holds everything, lift included, so impacts freeze whole.
            float dt = CombatFx.Frozen ? 0f : Time.deltaTime;

            float wantLift = (owner != null && owner.Airborne && !owner.Dead) ? AirLift : 0f;
            Lift = Mathf.MoveTowards(Lift, wantLift, dt * (wantLift > Lift ? 4.5f : 2.2f));

            if (active)
            {
                t += dt;
                float dur = Mathf.Max(0.01f, cur.dur * DurMul);
                float nt = t / dur;
                if (nt >= 1f) { active = false; Apply(Vector3.zero, Vector3.zero, 0f); }
                else
                {
                    float e = Envelope(nt, cur.peak) * AmpMul;
                    Apply(cur.pos * e, cur.rot * e, cur.squash * e);
                }
            }
            else if (Lift > 0.0001f) Apply(Vector3.zero, Vector3.zero, 0f);
        }

        bool Capture()
        {
            if (captured) return visual != null;
            visual = transform.Find("Visual");
            owner = GetComponent<Fighter>();
            captured = true;
            if (visual == null) return false;   // capsule fallback: no procedural layer
            basePos = visual.localPosition;
            baseRot = visual.localRotation;
            baseScale = visual.localScale;
            return true;
        }

        public void Play(Move m) => Play(m, 1f);

        public void Play(Move m, float extraAmp)
        {
            cur = Lookup(m);
            cur.pos *= extraAmp;
            cur.rot *= extraAmp;
            t = 0f;
            active = true;
        }

        // Jump the active gesture to its amplitude peak — the pose the move was
        // drawn to hit in. Called at the instant a strike resolves (CombatSystem),
        // because hit-stop then freezes the fighter for up to 140 ms: without this
        // the freeze frame catches a lunge that has not left the idle pose, and the
        // hit reads as landing from further away than the body ever reached.
        public void Contact()
        {
            if (!active) return;
            t = Mathf.Max(t, Mathf.Max(0.01f, cur.dur * DurMul) * cur.peak);
        }

        void Apply(Vector3 pos, Vector3 rotEuler, float squash)
        {
            if (visual == null) return;
            visual.localPosition = basePos + pos + Vector3.up * Lift;
            visual.localRotation = baseRot * Quaternion.Euler(rotEuler);
            float sy = Mathf.Max(0.5f, 1f - squash);
            float sx = 1f + squash * 0.5f;
            visual.localScale = new Vector3(baseScale.x * sx, baseScale.y * sy, baseScale.z * sx);
        }

        // Fast attack to the peak, eased release. Smoothstep on each leg.
        static float Envelope(float nt, float peak)
        {
            if (nt <= 0f || nt >= 1f) return 0f;
            if (nt < peak)
            {
                float u = nt / peak;
                return u * u * (3f - 2f * u);
            }
            float d = (nt - peak) / (1f - peak);
            return 1f - d * d * (3f - 2f * d);
        }

        static Gesture Lookup(Move m)
        {
            switch (m)
            {
                case Move.Jab:          return new Gesture { dur = 0.20f, pos = V(0.20f, 0f, 0f),    rot = V(0f, -8f, 0f),   squash = 0f,     peak = 0.28f };
                case Move.Cross:        return new Gesture { dur = 0.24f, pos = V(0.28f, 0f, 0f),    rot = V(0f, -14f, 0f),  squash = 0f,     peak = 0.30f };
                case Move.Finisher:     return new Gesture { dur = 0.30f, pos = V(0.34f, -0.05f, 0f),rot = V(0f, -10f, 0f),  squash = 0.05f,  peak = 0.32f };
                case Move.Heavy:        return new Gesture { dur = 0.40f, pos = V(0.42f, -0.06f, 0f),rot = V(0f, -18f, 0f),  squash = 0.04f,  peak = 0.45f };
                case Move.Launcher:     return new Gesture { dur = 0.34f, pos = V(0.14f, 0.30f, 0f), rot = V(0f, 0f, 8f),    squash = -0.10f, peak = 0.40f };
                case Move.Sweep:        return new Gesture { dur = 0.34f, pos = V(0.30f, -0.02f, 0f),rot = V(0f, -6f, 0f),   squash = 0.22f,  peak = 0.35f };
                case Move.BackHop:      return new Gesture { dur = 0.34f, pos = V(-0.55f, 0.18f, 0f),rot = V(0f, 14f, 0f),   squash = -0.04f, peak = 0.30f };
                case Move.AirRake:      return new Gesture { dur = 0.28f, pos = V(0.24f, 0.34f, 0f), rot = V(0f, -10f, 0f),  squash = -0.06f, peak = 0.30f };
                case Move.AirSlam:      return new Gesture { dur = 0.34f, pos = V(0.30f, 0.10f, 0f), rot = V(0f, -12f, 0f),  squash = 0.06f,  peak = 0.25f };
                case Move.SpecialKest:  return new Gesture { dur = 0.50f, pos = V(0.50f, 0.05f, 0f), rot = V(0f, -360f, 0f), squash = 0f,     peak = 0.50f };
                case Move.SpecialTengi: return new Gesture { dur = 0.50f, pos = V(0.36f, 0.22f, 0f), rot = V(0f, 0f, -24f),  squash = -0.06f, peak = 0.45f };
                case Move.Parry:        return new Gesture { dur = 0.22f, pos = V(-0.06f, 0f, 0f),   rot = V(0f, 6f, 0f),    squash = 0f,     peak = 0.20f };
                case Move.Hit:          return new Gesture { dur = 0.26f, pos = V(-0.28f, 0.04f, 0f),rot = V(0f, 16f, 0f),   squash = 0f,     peak = 0.18f };
                default:                return new Gesture { dur = 0.20f, pos = Vector3.zero,        rot = Vector3.zero,     squash = 0f,     peak = 0.30f };
            }
        }

        static Vector3 V(float x, float y, float z) => new Vector3(x, y, z);
    }
}
