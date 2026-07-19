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

        void Awake()
        {
            I = this;
            musicSource = gameObject.AddComponent<AudioSource>();
            musicSource.loop = true;
            musicSource.volume = 0.55f;
            sfxSource = gameObject.AddComponent<AudioSource>();
            voSource = gameObject.AddComponent<AudioSource>();
            voSource.volume = 0.9f;
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
