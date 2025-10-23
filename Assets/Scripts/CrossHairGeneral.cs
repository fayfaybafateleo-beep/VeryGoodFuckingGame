using UnityEngine;

public class CrossHairGenral : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [Header("Crosshair")]
    public RectTransform UiTarget; 

    [Header("ShakeAmplitude")]
    public Vector2 PositionAmplitude = new Vector2(8f, 8f);
    [Header("AmplitudeAmplitude")]
    public float RotationAmplitude = 6f;
    [Header("ShakeFrequency")]
    public float Frequency = 18f;

    [Header("ShakeHoldTime")]
    public float ShakeHoldTime;
    [Header("ShakeFadeOut")]
    public float FadeOut;

    [Header("IsUnScaleTime")]
    public bool UseUnscaledTime = true;   // 命中反馈一般不受慢动作影响更顺手

   
    Vector2 _baseUIPos;
    float _baseUIRotZ;

    float _strength;     // 0..1
    float _fadeVel;      // SmoothDamp缓存
    float _holdTimer;
    float _seedX, _seedY, _seedR;

    [Header("HitMarkSound")]
    public AudioSource HitMarkHitSound;
    public AudioClip HitMarkHitClip;
    void Awake()
    {
        _baseUIPos = UiTarget.anchoredPosition;
        _baseUIRotZ = UiTarget.localEulerAngles.z;
        _seedX = Random.value * 1000f + 111f;
        _seedY = Random.value * 1000f + 222f;
        _seedR = Random.value * 1000f + 333f;
    }

    void LateUpdate()
    {
        float tNow = UseUnscaledTime ? Time.unscaledTime : Time.time;
        float dt = UseUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
        if (dt <= 0f) return;

        // 更新包络：先hold，再SmoothDamp到0
        if (_holdTimer > 0f)
        {
            _holdTimer -= dt;
            _strength = 1f;
        }
        else
        {
            _strength = Mathf.SmoothDamp(_strength, 0f, ref _fadeVel, Mathf.Max(0.0001f, FadeOut));
            if (_strength < 0.0001f) _strength = 0f;
        }

        // 无抖动需求时保持基准
        if (_strength <= 0f)
        {
            ApplyOutput(Vector2.zero, 0f);
            return;
        }

        // 生成连续的Perlin噪声（不会跳变）
        float ft = tNow * Frequency;
        float nx = (Mathf.PerlinNoise(_seedX, ft) - 0.5f) * 2f; // [-1,1]
        float ny = (Mathf.PerlinNoise(_seedY, ft) - 0.5f) * 2f;
        float nr = (Mathf.PerlinNoise(_seedR, ft) - 0.5f) * 2f;

        Vector2 jitter = new Vector2(nx * PositionAmplitude.x, ny * PositionAmplitude.y) * _strength;
        float roll = nr * RotationAmplitude * _strength;

        ApplyOutput(jitter, roll);
    }

    void ApplyOutput(Vector2 offset, float rollZ)
    {
            UiTarget.anchoredPosition = _baseUIPos + offset;
            var e = UiTarget.localEulerAngles;
            e.z = _baseUIRotZ + rollZ;
            UiTarget.localEulerAngles = e;
    }

    /// 在命中时调用；strength 可用于暴头/弱命中区分（1=标准）
    public void AddShake(float strength = 1f)
    {
        _holdTimer = ShakeHoldTime;
        _strength = Mathf.Clamp01(Mathf.Max(_strength, 0.8f * strength)); // 叠加时保持有力
        _fadeVel = 0f; // 重置淡出速度，避免上次残留
    }

    // 可选：命中后立刻复位（如果需要）
    public void ResetPose()
    {
        _strength = 0f;
        _fadeVel = 0f;
        _holdTimer = 0f;
        ApplyOutput(Vector2.zero, 0f);
    }

    public void HitMarkHitSoundPlay()
    {
        HitMarkHitSound.PlayOneShot(HitMarkHitClip);
    }
    public void AddKillShake(float strength = 1f)
    {
        _holdTimer = ShakeHoldTime;
        _strength = Mathf.Clamp01(Mathf.Max(_strength, 10f * strength)); // 叠加时保持有力
        _fadeVel = 0f; // 重置淡出速度，避免上次残留
    }
}
