using System.Collections.Generic;
using System.Threading.Tasks;
using GLTFast;
using UnityEngine;

namespace KaijuRuin
{
    // Loads base character GLBs and per-move animation clip GLBs from
    // StreamingAssets/Models via glTFast (D-011). The clip GLBs are full
    // animated exports of the same rig; only their AnimationClips are used.
    public static class GltfCharacterLoader
    {
        static string Uri(string file)
        {
            var p = System.IO.Path.Combine(Application.streamingAssetsPath, "Models", file);
#if UNITY_ANDROID && !UNITY_EDITOR
            return p;                       // already a jar: URI on Android
#else
            return "file://" + p;
#endif
        }

        public static async Task<GameObject> LoadCharacter(string baseGlb, Dictionary<string, string> clipFiles,
            Vector3 position, bool faceRight, Transform parent)
        {
            var root = new GameObject(baseGlb);
            root.transform.SetParent(parent, false);
            root.transform.position = position;

            var gltf = new GltfImport();
            bool ok = await gltf.Load(Uri(baseGlb));
            if (!ok)
            {
                Debug.LogError("Failed to load GLB " + baseGlb + " - using capsule stand-in");
                var capsule = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                capsule.transform.SetParent(root.transform, false);
                capsule.transform.localPosition = new Vector3(0f, 1f, 0f);
            }
            else
            {
                var visual = new GameObject("Visual");
                visual.transform.SetParent(root.transform, false);
                await gltf.InstantiateMainSceneAsync(visual.transform);
                visual.transform.localRotation = Quaternion.Euler(0f, 90f, 0f);   // face +X
            }

            var clips = new Dictionary<string, AnimationClip>();
            foreach (var kv in clipFiles)
            {
                var clip = await LoadClip(kv.Value);
                if (clip != null) clips[kv.Key] = clip;
            }

            var animator = root.AddComponent<FighterAnimator>();
            animator.Bind(root, clips);

            if (!faceRight)
            {
                var s = root.transform.localScale;
                s.x = -Mathf.Abs(s.x);
                root.transform.localScale = s;
            }
            return root;
        }

        // Extracts the first AnimationClip of an animated GLB export.
        static async Task<AnimationClip> LoadClip(string file)
        {
            var gltf = new GltfImport();
            bool ok = await gltf.Load(Uri(file));
            if (!ok) { Debug.LogWarning("Missing animation GLB: " + file); return null; }
            var clips = gltf.GetAnimationClips();
            if (clips == null || clips.Length == 0) { Debug.LogWarning("No clips in " + file); return null; }
            return clips[0];
        }
    }
}
