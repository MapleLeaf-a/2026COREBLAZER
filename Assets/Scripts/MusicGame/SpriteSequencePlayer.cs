using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 多图层动画播放器: 读 Resources/StepAnimations/{stepId}.json 按时间表播放
/// 支持: 多图层叠加 / 每帧位置缩放旋转不透明度 / 关键帧插值 (含贝塞尔)
/// 坐标系: 比例坐标 — 位置是 "占画面百分比" (0=居中, 100=右移半个画面宽)
/// 由 CoreBlazer 步骤动画编排工具自动配套生成
/// </summary>
public class SpriteSequencePlayer : MonoBehaviour
{
    [Header("要播放的步骤 ID (对应 StepAnimations/{stepId}.json)")]
    public string stepId = "";

    [Header("画面参考分辨率 (X = 100% 对应多少像素)")]
    public Vector2 referenceResolution = new Vector2(1920, 1080);

    [Header("Pixels Per Unit (用于把像素坐标换成 Unity 单位)")]
    public float pixelsPerUnit = 100f;

    [Header("启动时自动播放")]
    public bool playOnStart = true;

    [Header("循环播放")]
    public bool loop = false;

    [Header("放置图层用的容器 (留空则在本对象下创建)")]
    public Transform layerContainer;

    [System.Serializable]
    public class TransformEntry
    {
        public float posX = 0;      // % of half-screen width (50 = right-edge)
        public float posY = 0;      // % of half-screen height (50 = top-edge)
        public float scale = 100;   // %
        public float rotation = 0;  // degrees
        public float opacity = 100; // %
    }

    [System.Serializable]
    public class KeyframeEntry
    {
        public float timeSec;
        public string prop;     // "posX" | "posY" | "scale" | "rotation" | "opacity"
        public float value;
        public string easing = "linear"; // "linear" | "easeIn" | "easeOut" | "easeInOut"
    }

    [System.Serializable]
    public class FrameEntry
    {
        public string imageName;
        public float startSec;
        public float durationSec;
        public TransformEntry transform = new TransformEntry();
        public List<KeyframeEntry> keyframes = new List<KeyframeEntry>();
    }

    [System.Serializable]
    public class LayerEntry
    {
        public string id;
        public string name;
        public bool visible = true;
        public List<FrameEntry> frames = new List<FrameEntry>();
    }

    [System.Serializable]
    public class AnimationData
    {
        public string stepId;
        public int formatVersion = 2;
        public Vector2 referenceResolution = new Vector2(1920, 1080);
        public List<LayerEntry> layers = new List<LayerEntry>();
        // Legacy support: if 'frames' is present (v1), it's a single layer
        public List<FrameEntry> frames;
    }

    private AnimationData data;
    private Dictionary<string, Sprite> spriteCache = new Dictionary<string, Sprite>();
    // Per-layer runtime renderer (created at Load time)
    private List<SpriteRenderer> layerRenderers = new List<SpriteRenderer>();
    private float startTime;
    private bool playing = false;

    void Awake()
    {
        if (layerContainer == null) layerContainer = transform;
    }

    void Start()
    {
        if (!string.IsNullOrEmpty(stepId)) Load(stepId);
        if (playOnStart) Play();
    }

    public void Load(string id)
    {
        stepId = id;
        spriteCache.Clear();
        // Tear down existing layer renderers
        foreach (var r in layerRenderers) if (r != null) Destroy(r.gameObject);
        layerRenderers.Clear();

        TextAsset json = Resources.Load<TextAsset>("StepAnimations/" + stepId);
        if (json == null) { Debug.LogWarning("找不到动画数据: StepAnimations/" + stepId); return; }
        data = JsonUtility.FromJson<AnimationData>(json.text);
        if (data == null) { Debug.LogWarning("动画 JSON 解析失败: " + stepId); return; }

        // Legacy v1 → wrap into single layer
        if ((data.layers == null || data.layers.Count == 0) && data.frames != null && data.frames.Count > 0)
        {
            data.layers = new List<LayerEntry>();
            data.layers.Add(new LayerEntry { id = "legacy", name = "Layer 1", visible = true, frames = data.frames });
        }
        if (data.referenceResolution.x <= 0) data.referenceResolution = referenceResolution;

        // Pre-load sprites and create one SpriteRenderer per layer
        for (int li = 0; li < data.layers.Count; li++)
        {
            var layer = data.layers[li];
            // Cache sprites
            foreach (var f in layer.frames)
            {
                if (string.IsNullOrEmpty(f.imageName)) continue;
                if (!spriteCache.ContainsKey(f.imageName))
                {
                    Sprite s = Resources.Load<Sprite>("StepAnimations/" + stepId + "_frames/" + f.imageName);
                    if (s == null) Debug.LogWarning("找不到帧贴图: " + stepId + "_frames/" + f.imageName);
                    spriteCache[f.imageName] = s;
                }
            }
            // Create renderer GameObject for this layer
            var go = new GameObject("Layer_" + (li + 1) + "_" + (layer.name ?? ""));
            go.transform.SetParent(layerContainer, false);
            go.transform.localPosition = Vector3.zero;
            var sr = go.AddComponent<SpriteRenderer>();
            // First layer (index 0) is front-most → highest sortingOrder
            sr.sortingOrder = data.layers.Count - li;
            layerRenderers.Add(sr);
        }
    }

    public void Play()
    {
        startTime = Time.time;
        playing = true;
        ApplyTime(0);
    }

    public void Stop() { playing = false; }
    public bool IsPlaying => playing;

    void Update()
    {
        if (!playing || data == null || data.layers == null) return;
        float t = Time.time - startTime;
        float totalDur = GetTotalDuration();
        if (totalDur > 0 && t > totalDur)
        {
            if (loop) { startTime = Time.time; t = 0; }
            else { playing = false; return; }
        }
        ApplyTime(t);
    }

    float GetTotalDuration()
    {
        float max = 0;
        foreach (var layer in data.layers)
            foreach (var f in layer.frames)
            {
                float end = f.startSec + f.durationSec;
                if (end > max) max = end;
            }
        return max;
    }

    void ApplyTime(float t)
    {
        for (int li = 0; li < data.layers.Count; li++)
        {
            var layer = data.layers[li];
            var sr = layerRenderers[li];
            if (!layer.visible) { sr.enabled = false; continue; }

            // Find active frame at time t
            FrameEntry active = null;
            foreach (var f in layer.frames)
            {
                if (t >= f.startSec && t < f.startSec + f.durationSec) { active = f; break; }
            }
            if (active == null) { sr.enabled = false; continue; }
            sr.enabled = true;

            // Apply sprite
            if (spriteCache.TryGetValue(active.imageName, out Sprite s)) sr.sprite = s;

            // Compute effective transform (with keyframe interpolation)
            float posX = ResolveProp(active, "posX", t, active.transform.posX);
            float posY = ResolveProp(active, "posY", t, active.transform.posY);
            float scale = ResolveProp(active, "scale", t, active.transform.scale);
            float rotation = ResolveProp(active, "rotation", t, active.transform.rotation);
            float opacity = ResolveProp(active, "opacity", t, active.transform.opacity);

            // Apply to transform — proportional coords:
            // posX/posY are percentages of half-screen.
            // Convert: pixel_offset = (pos / 100) * (refResolution / 2)
            // Then convert px → unity units by dividing by PPU.
            float pxX = (posX / 100f) * (data.referenceResolution.x / 2f);
            float pxY = (posY / 100f) * (data.referenceResolution.y / 2f);
            float ux = pxX / pixelsPerUnit;
            float uy = pxY / pixelsPerUnit;
            sr.transform.localPosition = new Vector3(ux, uy, 0);
            sr.transform.localScale = Vector3.one * (scale / 100f);
            sr.transform.localRotation = Quaternion.Euler(0, 0, rotation);
            var c = sr.color; c.a = opacity / 100f; sr.color = c;
        }
    }

    float ResolveProp(FrameEntry frame, string prop, float t, float baseValue)
    {
        if (frame.keyframes == null || frame.keyframes.Count == 0) return baseValue;
        // Filter and sort
        List<KeyframeEntry> kfs = new List<KeyframeEntry>();
        foreach (var k in frame.keyframes) if (k.prop == prop) kfs.Add(k);
        if (kfs.Count == 0) return baseValue;
        kfs.Sort((a, b) => a.timeSec.CompareTo(b.timeSec));
        if (kfs.Count == 1) return kfs[0].value;
        if (t <= kfs[0].timeSec) return kfs[0].value;
        if (t >= kfs[kfs.Count - 1].timeSec) return kfs[kfs.Count - 1].value;
        KeyframeEntry kf0 = kfs[0], kf1 = kfs[1];
        for (int i = 0; i < kfs.Count - 1; i++)
            if (t >= kfs[i].timeSec && t <= kfs[i + 1].timeSec) { kf0 = kfs[i]; kf1 = kfs[i + 1]; break; }
        float span = kf1.timeSec - kf0.timeSec;
        float u = span > 0 ? (t - kf0.timeSec) / span : 0;
        float eased = ApplyEasing(u, kf0.easing);
        return kf0.value + (kf1.value - kf0.value) * eased;
    }

    static float ApplyEasing(float u, string type)
    {
        u = Mathf.Clamp01(u);
        switch (type)
        {
            case "linear": return u;
            case "easeIn": return u * u;
            case "easeOut": return u * (2 - u);
            case "easeInOut": return u < 0.5f ? 2 * u * u : -1 + (4 - 2 * u) * u;
            default: return u;
        }
    }
}
