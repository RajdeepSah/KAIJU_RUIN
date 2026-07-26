using System.Collections.Generic;
using UnityEngine;

namespace KaijuRuin
{
    // Loads clips from Resources/Audio/{music,sfx,vo}. Missing clips fail soft
    // (rows still `planned` in the asset manifest simply stay silent).
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager I { get; private set; }

        AudioSource musicSource;
        AudioSource sfxSource;
        AudioSource voSource;
        readonly Dictionary<string, AudioClip> cache = new Dictionary<string, AudioClip>();
        string currentMusic;

        const float MusicVolume = 0.55f;

        // How far each bus follows a slow-motion time scale (D-026). Silent slow
        // motion reads as a dropped frame, not a camera move — the audio drag is
        // most of what sells it — but the three buses want very different amounts:
        //
        //   SFX almost fully, so an impact that starts at the hit visibly (audibly)
        //       stretches with it. AudioSource.pitch applies to PlayOneShot voices
        //       already in flight, so this drags the hit that CAUSED the shot, not
        //       just the next one.
        //   MUSIC barely. A loop pitched to 0.2 is an octave-plus down and reads as
        //       comedy or as a broken build; a slight dip plus a duck reads as the
        //       room falling away. The mix gets out of the way of the impact instead.
        //   VO not at all — the announcer's "K.O." fires into the deepest shot in the
        //       game and has to stay intelligible.
        const float SfxDrag = 0.9f;
        const float MusicDrag = 0.3f;
        const float MusicDuck = 0.72f;   // volume multiplier at full depth

        void Awake()
        {
            I = this;
            musicSource = gameObject.AddComponent<AudioSource>();
            musicSource.loop = true;
            musicSource.volume = MusicVolume;
            sfxSource = gameObject.AddComponent<AudioSource>();
            voSource = gameObject.AddComponent<AudioSource>();
            voSource.volume = 0.9f;
        }

        // Driven every frame by TimeDirector with the live slow-motion scale
        // (1 = real time). Idempotent and cheap: at scale 1 it writes the resting
        // values back, so nothing has to remember to undo it.
        public void SetTimeStretch(float scale)
        {
            scale = Mathf.Clamp(scale, 0.05f, 1f);
            float depth = 1f - scale;
            if (sfxSource != null) sfxSource.pitch = Mathf.Lerp(1f, scale, SfxDrag);
            if (musicSource != null)
            {
                musicSource.pitch = Mathf.Lerp(1f, scale, MusicDrag);
                musicSource.volume = MusicVolume * Mathf.Lerp(1f, MusicDuck, depth);
            }
        }

        AudioClip Load(string path)
        {
            if (cache.TryGetValue(path, out var c)) return c;
            c = Resources.Load<AudioClip>("Audio/" + path);
            if (c == null) Debug.LogWarning("Missing audio: Audio/" + path);
            cache[path] = c;
            return c;
        }

        public void Music(string name)
        {
            if (currentMusic == name) return;
            currentMusic = name;
            var clip = Load("music/" + name);
            musicSource.clip = clip;
            if (clip != null) musicSource.Play(); else musicSource.Stop();
        }

        public void Sfx(string name, float volume = 1f)
        {
            var clip = Load("sfx/" + name);
            if (clip != null) sfxSource.PlayOneShot(clip, volume);
        }

        public void Announce(string name)
        {
            var clip = Load("vo/" + name);
            if (clip != null) { voSource.clip = clip; voSource.Play(); }
        }
    }
}
