using System.Collections;
using UnityEngine;

namespace KaijuRuin
{
    // Harbor Ruins: layered sprites with parallax, plus the scripted Khulandra
    // living-stage event (DESIGN_BRIEF.md: fires between rounds 1 and 2).
    public class StageManager : MonoBehaviour
    {
        public static StageManager I { get; private set; }

        public bool Flooded { get; private set; }

        SpriteRenderer sky;
        SpriteRenderer mid;
        SpriteRenderer ground;
        Camera cam;
        float lastSplashAt;

        public void Build(Camera camera)
        {
            I = this;
            cam = camera;

            sky = Layer("Sky", "stages/harbor_sky", z: 14f, scale: 3.4f, order: 0);
            mid = Layer("Mid", "stages/harbor_mid", z: 8f, scale: 2.6f, order: 5);
            ground = Layer("Ground", "stages/harbor_ground", z: 0.6f, scale: 1f, order: 10);
            if (ground != null)
            {
                ground.drawMode = SpriteDrawMode.Tiled;
                ground.size = new Vector2(30f, 2.6f);
                ground.transform.position = new Vector3(0f, -1.3f, 0.6f);
            }
        }

        SpriteRenderer Layer(string name, string sprite, float z, float scale, int order)
        {
            var s = AssetLib.Sprite(sprite, 100f);
            if (s == null) return null;
            var go = new GameObject(name, typeof(SpriteRenderer));
            go.transform.SetParent(transform, false);
            var sr = go.GetComponent<SpriteRenderer>();
            sr.sprite = s;
            sr.sortingOrder = order;
            go.transform.position = new Vector3(0f, name == "Sky" ? 3.5f : name == "Mid" ? 1.8f : 0f, z);
            go.transform.localScale = Vector3.one * scale;
            return sr;
        }

        void LateUpdate()
        {
            if (cam == null) return;
            float cx = cam.transform.position.x;
            if (sky != null) sky.transform.position = new Vector3(cx * 0.9f, sky.transform.position.y, sky.transform.position.z);
            if (mid != null) mid.transform.position = new Vector3(cx * 0.6f, mid.transform.position.y, mid.transform.position.z);
        }

        public void WadeSplash(Vector3 fighterPos)
        {
            if (!Flooded || Time.time - lastSplashAt < 0.4f) return;
            lastSplashAt = Time.time;
            var s = AssetLib.Sprite("vfx/meter_flare", 512f);
            if (s == null) return;
            var go = new GameObject("splash", typeof(SpriteRenderer));
            var sr = go.GetComponent<SpriteRenderer>();
            sr.sprite = s;
            sr.color = new Color(0.4f, 0.9f, 0.8f, 0.45f);
            sr.sortingOrder = 12;
            go.transform.position = new Vector3(fighterPos.x, 0.05f, 0.4f);
            go.transform.localScale = Vector3.one * 0.5f;
            go.AddComponent<VfxFade>();
        }

        // Scripted breach: roar, sprite rises behind the midground, shockwave,
        // then the fight plane swaps to the flooded strip for the rest of the match.
        public IEnumerator KhulandraEvent()
        {
            AudioManager.I?.Sfx("khulandra_roar");
            AudioManager.I?.Announce("announcer_khulandra_rises");

            var s = AssetLib.Sprite("stages/khulandra_breach", 140f);
            SpriteRenderer breach = null;
            if (s != null)
            {
                var go = new GameObject("KhulandraBreach", typeof(SpriteRenderer));
                go.transform.SetParent(transform, false);
                breach = go.GetComponent<SpriteRenderer>();
                breach.sprite = s;
                breach.sortingOrder = 4;   // behind mid, in front of sky
                go.transform.position = new Vector3(2.5f, -9f, 9f);
                go.transform.localScale = Vector3.one * 1.8f;
            }

            float t = 0f;
            while (t < 2.2f)
            {
                t += Time.deltaTime;
                if (breach != null)
                {
                    float y = Mathf.Lerp(-9f, -1.2f, Mathf.SmoothStep(0f, 1f, t / 2.2f));
                    breach.transform.position = new Vector3(2.5f, y, 9f);
                }
                if (cam != null)
                {
                    var p = cam.transform.position;
                    cam.transform.position = p + (Vector3)Random.insideUnitCircle * 0.03f;
                }
                yield return null;
            }

            CombatSystem.Spawn("kaiju_shockwave", new Vector3(0f, 0.2f, 0f), 3.0f);
            Flooded = true;
            var flooded = AssetLib.Sprite("stages/harbor_ground_flooded", 100f);
            if (ground != null && flooded != null) ground.sprite = flooded;

            yield return new WaitForSeconds(1.2f);

            if (breach != null)
            {
                t = 0f;
                var start = breach.transform.position;
                while (t < 1.5f)
                {
                    t += Time.deltaTime;
                    breach.transform.position = Vector3.Lerp(start, new Vector3(2.5f, -9f, 9f), t / 1.5f);
                    yield return null;
                }
                Destroy(breach.gameObject);
            }
        }
    }
}
