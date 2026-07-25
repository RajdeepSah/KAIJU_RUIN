using UnityEngine;

namespace KaijuRuin
{
    // Combat "juice": hit-stop and camera shake/punch. This is the single lever
    // that makes the fight read as fast and impactful (owner directive 2026-07-19,
    // D-015). It is deliberately render/timing-only and NEVER touches the
    // deterministic X-axis sim.
    //
    // Hit-stop is a real-time freeze WINDOW (same pattern as RoundManager.RoundFrozen /
    // GameManager.Paused) rather than Time.timeScale, because pause already owns
    // timeScale=0 and the two must not fight. While Frozen, movement, buffered
    // input, AI thinking, and clip playback all hold for a few frames on impact.
    public static class CombatFx
    {
        // ---- Hit-stop -------------------------------------------------------
        public static float FrozenUntil;
        public static bool Frozen => Time.time < FrozenUntil;

        // Per-move freeze lengths (seconds). Heavier hit => longer bite.
        public const float StopLight   = 0.045f;
        public const float StopMedium  = 0.07f;
        public const float StopHeavy   = 0.10f;
        public const float StopLaunch  = 0.08f;
        public const float StopSpecial = 0.14f;
        public const float StopParry   = 0.12f;

        public static void HitStop(float dur)
        {
            float u = Time.time + dur;
            if (u > FrozenUntil) FrozenUntil = u;
        }

        // ---- Camera shake ---------------------------------------------------
        static float shakeStart, shakeDur, shakeMag;

        public static void Shake(float mag, float dur)
        {
            if (mag <= 0f || dur <= 0f) return;
            // Keep the stronger of any residual shake and the new one.
            float t = Time.time - shakeStart;
            float residual = (shakeDur > 0f && t < shakeDur) ? shakeMag * (1f - t / shakeDur) : 0f;
            shakeStart = Time.time;
            shakeDur = dur;
            shakeMag = Mathf.Max(residual, mag);
        }

        public static Vector3 ShakeOffset()
        {
            if (shakeDur <= 0f) return Vector3.zero;
            float t = Time.time - shakeStart;
            if (t >= shakeDur) return Vector3.zero;
            float decay = 1f - t / shakeDur;
            float amp = shakeMag * decay * decay;
            float x = (Mathf.PerlinNoise(Time.time * 38f, 0.37f) - 0.5f) * 2f;
            float y = (Mathf.PerlinNoise(0.91f, Time.time * 41f) - 0.5f) * 2f;
            return new Vector3(x, y, 0f) * amp;
        }

        // ---- Camera punch (quick dolly toward the action) -------------------
        static float punchStart, punchDur, punchMag;

        public static void Punch(float mag, float dur)
        {
            if (mag <= 0f || dur <= 0f) return;
            // Keep the stronger of any *decaying* residual punch and the new one
            // (compute residual against the OLD punchStart before overwriting it).
            float t = Time.time - punchStart;
            float residual = (punchDur > 0f && t < punchDur) ? punchMag * (1f - t / punchDur) : 0f;
            punchStart = Time.time;
            punchDur = dur;
            punchMag = Mathf.Max(residual, mag);
        }

        // Positive = camera moved closer (toward +z; the fight cam sits at -z).
        public static float PunchZ()
        {
            if (punchDur <= 0f) return 0f;
            float t = Time.time - punchStart;
            if (t >= punchDur) return 0f;
            return punchMag * Mathf.Sin(Mathf.PI * (t / punchDur));
        }

        // Cleared on every round reset so a freeze/shake never leaks across rounds.
        public static void Reset()
        {
            FrozenUntil = 0f;
            shakeDur = 0f; shakeMag = 0f;
            punchDur = 0f; punchMag = 0f;
        }
    }
}
