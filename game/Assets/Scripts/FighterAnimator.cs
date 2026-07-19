using System.Collections.Generic;
using UnityEngine;

namespace KaijuRuin
{
    // Runtime animation driver over the glTFast-loaded rig.
    //
    // D-011 intent is Animator Controller + blend trees; AnimatorController
    // authoring is an editor-only API, so at RUNTIME this wraps the legacy
    // Animation component glTFast produces and emulates the locomotion blend
    // with idle/walk crossfades. When the GLBs are imported in-editor for
    // production, wire the same state names into a real AnimatorController.
    public class FighterAnimator : MonoBehaviour
    {
        Animation anim;
        readonly Dictionary<string, string> states = new Dictionary<string, string>();
        string current;
        bool locomotionWalking;

        public void Bind(GameObject rigRoot, Dictionary<string, AnimationClip> clips)
        {
            anim = rigRoot.GetComponentInChildren<Animation>();
            if (anim == null) anim = rigRoot.AddComponent<Animation>();
            foreach (var kv in clips)
            {
                if (kv.Value == null) continue;
                kv.Value.legacy = true;
                kv.Value.wrapMode = (kv.Key == "idle" || kv.Key == "walk") ? WrapMode.Loop : WrapMode.Once;
                anim.AddClip(kv.Value, kv.Key);
                states[kv.Key] = kv.Key;
            }
            Play("idle", 0f);
        }

        public bool Has(string state) => states.ContainsKey(state);

        // Missing clips degrade per the design brief: special falls back to
        // punch, anything else falls back to idle.
        string Resolve(string state)
        {
            if (states.ContainsKey(state)) return state;
            if (state == "special" && states.ContainsKey("punch")) return "punch";
            return states.ContainsKey("idle") ? "idle" : null;
        }

        public void Play(string state, float fade = 0.12f, float speed = 1f)
        {
            if (anim == null) return;
            var s = Resolve(state);
            if (s == null) return;
            anim[s].speed = speed;
            // Re-triggering the clip already playing (chain hits 2/3, rapid hit
            // reactions) must restart it: CrossFade to a full-weight clip is a
            // no-op, so the screen otherwise never answers the repeated input.
            if (current == s) anim[s].time = 0f;
            current = s;
            anim.CrossFade(s, fade);
        }

        public void SetLocomotion(float speed01)
        {
            if (anim == null) return;
            bool walk = speed01 > 0.05f;
            if (walk == locomotionWalking && (current == "idle" || current == "walk")) { }
            if (current != "idle" && current != "walk")
            {
                if (anim.isPlaying) return;   // let one-shots finish
            }
            locomotionWalking = walk;
            var target = walk ? "walk" : "idle";
            if (current != target) Play(target, 0.15f);
        }

        public float Length(string state)
        {
            var s = Resolve(state);
            return (anim != null && s != null && anim[s] != null) ? anim[s].length : 0.6f;
        }
    }
}
