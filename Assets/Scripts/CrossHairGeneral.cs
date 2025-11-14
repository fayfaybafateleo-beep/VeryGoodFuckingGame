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
    public float KillShakeHoldTime = 0.2f;
    [Header("ShakeFadeOut")]
    public float FadeOut;

    [Header("IsUnScaleTime")]
    public bool UseUnscaledTime = true;   

   
    Vector2 _baseUIPos;
    float _baseUIRotZ;

    float _strength;     
    float _fadeVel;      
    float _holdTimer;
    float _seedX, _seedY, _seedR;

    [Header("HitMarkSound")]
    public AudioSource HitMarkHitSound;
    public AudioClip HitMarkHitClip;
    public AudioClip HitMarkKillClip;
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

        // KeppIdle
        if (_strength <= 0f)
        {
            ApplyOutput(Vector2.zero, 0f);
            return;
        }

        // Noice
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

    public void AddShake(float strength = 1f)
    {
        _holdTimer = ShakeHoldTime;
        _strength = Mathf.Clamp01(Mathf.Max(_strength, 0.8f * strength)); 
        _fadeVel = 0f; 
    }

  
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
        _holdTimer = KillShakeHoldTime;
        _strength = Mathf.Clamp01(Mathf.Max(_strength, 100f * strength)); 
        _fadeVel = 0f; 
    }
    public void HitMarkKillSoundPlay()
    {
        HitMarkHitSound.PlayOneShot(HitMarkKillClip);
    }
}
