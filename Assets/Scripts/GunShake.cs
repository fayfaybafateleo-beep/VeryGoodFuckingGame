using UnityEngine;
using UnityEngine.Serialization;

public class GunShake : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [Header("Target Transforms")]
    [FormerlySerializedAs("positionTarget")]
    public Transform PositionTarget;

    [FormerlySerializedAs("rotationTarget")]
    public Transform RotationTarget;

    [Header("Recoil Settings")]
    [FormerlySerializedAs("kickBackDistance")]
    public float RecoilBackDistance = 0.03f;

    [FormerlySerializedAs("kickPitchYaw")]
    public Vector2 RecoilPitchYaw = new Vector2(4f, 1.5f);

    [FormerlySerializedAs("kickRandomYawSignChance")]
    [Range(0, 1)] public float RandomYawChance = 0.5f;

    [Header("Return Spring")]
    [FormerlySerializedAs("posSpringStiffness")]
    public float PositionReturnSpeed = 120f;

    [FormerlySerializedAs("rotSpringStiffness")]
    public float RotationReturnSpeed = 140f;

    [FormerlySerializedAs("dampingRatioPos")]
    [Range(0.8f, 2f)] public float PositionDamping = 1.1f;

    [FormerlySerializedAs("dampingRatioRot")]
    [Range(0.8f, 2f)] public float RotationDamping = 1.15f;

    [Header("Fire Shake")]
    [FormerlySerializedAs("noisePosAmp")]
    public float FirePositionShake = 0.003f;

    [FormerlySerializedAs("noiseRotAmp")]
    public float FireRotationShake = 0.6f;

    [FormerlySerializedAs("noiseFrequency")]
    public float FireShakeFrequency = 18f;

    [FormerlySerializedAs("noiseAxesPos")]
    public Vector3 PositionShakeAxis = new Vector3(1f, 1f, 0.2f);

    [FormerlySerializedAs("noiseAxesRot")]
    public Vector3 RotationShakeAxis = new Vector3(0.4f, 0.6f, 1f);

    [Header("Fire Shake Timing")]
    [FormerlySerializedAs("shakeHoldTime")]
    public float ShakeHoldTime = 0.06f;

    [FormerlySerializedAs("shakeFadeTime")]
    public float ShakeFadeTime = 0.18f;

    [Header("Idle Shake")]
    [FormerlySerializedAs("enableIdleNoise")]
    public bool EnableIdleShake = false;

    [FormerlySerializedAs("idleNoisePosAmp")]
    public float IdlePositionShake = 0.0006f;

    [FormerlySerializedAs("idleNoiseRotAmp")]
    public float IdleRotationShake = 0.12f;

    [FormerlySerializedAs("idleNoiseFrequency")]
    public float IdleShakeFrequency = 6f;

    [Header("Time Control")]
    [FormerlySerializedAs("useUnscaledTime")]
    public bool IgnoreTimeScale = false;


    Vector3 BaseLocalPos;
    Vector3 BaseLocalEuler;

    Vector3 PositionOffset;
    Vector3 PositionVelocity;

    Vector3 RotationOffset;
    Vector3 RotationVelocity;

    float NoiseSeedX;
    float NoiseSeedY;
    float NoiseSeedZ;

    float ShakeStrength;
    float ShakeVelocity;
    float ShakeHoldTimer;

    Transform PosTarget => PositionTarget ? PositionTarget : transform;
    Transform RotTarget => RotationTarget ? RotationTarget : transform;



    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        BaseLocalPos = PosTarget.localPosition;
        BaseLocalEuler = RotTarget.localEulerAngles;

        NoiseSeedX = Random.value * 1000f;
        NoiseSeedY = Random.value * 1000f;
        NoiseSeedZ = Random.value * 1000f;
    }

    void LateUpdate()
    {
        float dt = IgnoreTimeScale ? Time.unscaledDeltaTime : Time.deltaTime;
        float time = IgnoreTimeScale ? Time.unscaledTime : Time.time;
        if (dt <= 0f) return;

        //Spring Return
        ApplySpring(ref PositionOffset, ref PositionVelocity, Vector3.zero,
            PositionReturnSpeed, PositionDamping, dt);

        ApplySpring(ref RotationOffset, ref RotationVelocity, Vector3.zero,
            RotationReturnSpeed, RotationDamping, dt);

        //Noise
        Vector3 finalPosNoise = Vector3.zero;
        Vector3 finalRotNoise = Vector3.zero;

        //IdleNoise
        if (EnableIdleShake)
        {
            float tIdle = time * IdleShakeFrequency;
            Vector3 idleNoise = GetNoise(tIdle);

            finalPosNoise += Vector3.Scale(idleNoise, PositionShakeAxis) * IdlePositionShake;
            finalRotNoise += Vector3.Scale(idleNoise, RotationShakeAxis) * IdleRotationShake;
        }

        // Fire Noise
        if (ShakeStrength > 0f)
        {
            float tFire = time * FireShakeFrequency;
            Vector3 fireNoise = GetNoise(tFire + 100f);

            finalPosNoise += Vector3.Scale(fireNoise, PositionShakeAxis) *
                             FirePositionShake * ShakeStrength;

            finalRotNoise += Vector3.Scale(fireNoise, RotationShakeAxis) *
                             FireRotationShake * ShakeStrength;
        }

        // ShakeFadeout/In
        if (ShakeHoldTimer > 0f)
        {
            ShakeHoldTimer -= dt;
            ShakeStrength = 1f;
        }
        else
        {
            ShakeStrength = Mathf.SmoothDamp(
                ShakeStrength,
                0f,
                ref ShakeVelocity,
                ShakeFadeTime
            );
        }

        //ApplyOnGun
        PosTarget.localPosition = BaseLocalPos + PositionOffset + finalPosNoise;
        RotTarget.localRotation = Quaternion.Euler(
            BaseLocalEuler + RotationOffset + finalRotNoise
        );
    }

   

    public void AddRecoil(float Strength = 1f)
    {
        PositionVelocity += new Vector3(
            0f,
            0f,
            -RecoilBackDistance * Strength * 60f
        );

        float yawDir = Random.value < RandomYawChance ? -1f : 1f;

        RotationVelocity += new Vector3(
            RecoilPitchYaw.x * Strength * 30f,
            RecoilPitchYaw.y * yawDir * Strength * 30f,
            0f
        );

        ShakeStrength = 1f;
        ShakeHoldTimer = ShakeHoldTime;
        ShakeVelocity = 0f;
    }



    Vector3 GetNoise(float t)
    {
        return new Vector3(
            (Mathf.PerlinNoise(NoiseSeedX, t) - 0.5f) * 2f,
            (Mathf.PerlinNoise(NoiseSeedY, t) - 0.5f) * 2f,
            (Mathf.PerlinNoise(NoiseSeedZ, t) - 0.5f) * 2f
        );
    }

    static void ApplySpring(
        ref Vector3 value,
        ref Vector3 velocity,
        Vector3 target,
        float stiffness,
        float damping,
        float dt
    )
    {
        float w = Mathf.Sqrt(Mathf.Max(stiffness, 0.0001f));
        float c = 2f * damping * w;
        //second order system
        Vector3 accel = -stiffness * (value - target) - c * velocity;

        velocity += accel * dt;
        value += velocity * dt;
    }
}
  

