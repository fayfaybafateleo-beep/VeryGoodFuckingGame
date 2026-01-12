using UnityEngine;
using UnityEngine.Serialization;

public class WeaponWaging : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [Header("Targets (optional)")]
    [FormerlySerializedAs("positionTarget")]
    public Transform PositionTarget;

    [FormerlySerializedAs("rotationTarget")]
    public Transform RotationTarget;

    [Header("Walk Sway / Bob")]
    [FormerlySerializedAs("walkFrequency")]
    public float WalkFrequency = 9.5f;

    [FormerlySerializedAs("walkPosAmplitude")]
    public Vector3 WalkPosAmplitude = new Vector3(0.006f, 0.012f, 0.004f);

    [FormerlySerializedAs("walkRotAmplitude")]
    public Vector3 WalkRotAmplitude = new Vector3(1.4f, 2.0f, 0.8f);

    [Header("Blending")]
    [FormerlySerializedAs("blendSmoothTime")]
    public float BlendSmoothTime = 0.12f;

    [FormerlySerializedAs("referenceMoveSpeed")]
    public float ReferenceMoveSpeed = 4.0f;

    [Header("ADS Scaling (optional)")]
    [FormerlySerializedAs("adsWeight")]
    [Range(0f, 1f)]
    public float AdsWeight = 1f;

    [Header("Idle Bob (optional)")]
    [FormerlySerializedAs("idlePosAmplitude")]
    public Vector3 IdlePosAmplitude = new Vector3(0.0015f, 0.003f, 0.001f);

    [FormerlySerializedAs("idleRotAmplitude")]
    public Vector3 IdleRotAmplitude = new Vector3(0.35f, 0.45f, 0.2f);

    [FormerlySerializedAs("idleFrequency")]
    public float IdleFrequency = 2.2f;

    [Header("Fire Suppress (stop sway on fire)")]
    [FormerlySerializedAs("stopSwayOnFire")]
    public bool StopSwayOnFire = true;

    [FormerlySerializedAs("fireSuppressTime")]
    public float FireSuppressTime = 0.12f;

    [FormerlySerializedAs("fireRecoverTime")]
    public float FireRecoverTime = 0.10f;

    [Header("Movement Source")]
    [FormerlySerializedAs("characterController")]
    public CharacterController CharacterController;

    [FormerlySerializedAs("externalMoveSpeed")]
    public float ExternalMoveSpeed;

    [FormerlySerializedAs("externalGrounded")]
    public bool ExternalGrounded = true;

    [Header("Equip / Reload Hand Animation")]
    [FormerlySerializedAs("lowerDistance")]
    public float LowerDistance = 0.12f;

    [FormerlySerializedAs("animPitchDeg")]
    public float AnimPitchDeg = -8f;

    [FormerlySerializedAs("lowerDuration")]
    public float LowerDuration = 0.18f;

    [FormerlySerializedAs("raiseDuration")]
    public float RaiseDuration = 0.20f;

    Transform P => PositionTarget ? PositionTarget : transform;
    Transform R => RotationTarget ? RotationTarget : transform;

    Vector3 BaseLocalPos;
    Vector3 BaseLocalEuler;

    float WalkPhase;
    float WalkBlend;
    float WalkBlendVel;
    float IdlePhase;

    float FireSuppressTimer;
    float FireRecoverProgress;

    enum HandAnimState { None, Lowering, Raising }
    HandAnimState AnimState = HandAnimState.None;

    Vector3 AnimPosOffset;
    Vector3 AnimRotOffsetEuler;

    Vector3 AnimStartPos;
    Vector3 AnimTargetPos;

    Vector3 AnimStartRotEuler;
    Vector3 AnimTargetRotEuler;

    float AnimTimer;
    float AnimDuration;

    void Awake()
    {

        //BaseTransform
        BaseLocalPos = P.localPosition;
        BaseLocalEuler = R.localEulerAngles;

        if (!CharacterController)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player)
                CharacterController = player.GetComponentInParent<CharacterController>();
        }
    }

    void LateUpdate()
    {
        float dt = Time.deltaTime;
        if (dt <= 0f) return;

        float speed = 0f;
        bool grounded = ExternalGrounded;

        //SpeedCheck
        if (CharacterController)
        {
            Vector3 v = CharacterController.velocity;
            v.y = 0f;
            speed = v.magnitude;
            grounded = CharacterController.isGrounded;
        }
        else
        {
            speed = Mathf.Max(0f, ExternalMoveSpeed);
        }

        float speed01 = Mathf.Clamp01(speed / Mathf.Max(0.0001f, ReferenceMoveSpeed));
        float targetBlend = grounded ? speed01 : 0f;

        // SupressWaggingWhenFire
        if (StopSwayOnFire)
        {
            if (FireSuppressTimer > 0f)
            {
                FireSuppressTimer -= dt;
                WalkBlend = 0f;
                FireRecoverProgress = 0f;
            }
            else
            {
                WalkBlend = Mathf.SmoothDamp(WalkBlend, targetBlend, ref WalkBlendVel, BlendSmoothTime);

                if (FireRecoverTime > 0f && WalkBlend > 0f && FireRecoverProgress < 1f)
                {
                    FireRecoverProgress = Mathf.Clamp01(FireRecoverProgress + dt / FireRecoverTime);
                    WalkBlend *= FireRecoverProgress;
                }
                else
                {
                    FireRecoverProgress = 1f;
                }
            }
        }
        else
        {
            WalkBlend = Mathf.SmoothDamp(WalkBlend, targetBlend, ref WalkBlendVel, BlendSmoothTime);
            FireRecoverProgress = 1f;
        }

        float weight = Mathf.Clamp01(AdsWeight);

        //WalkWagging

        float phaseSpeed = Mathf.Lerp(0.2f, 1f, speed01) * WalkFrequency;
        WalkPhase += dt * phaseSpeed;

        float s1 = Mathf.Sin(WalkPhase);
        float s2 = Mathf.Sin(WalkPhase * 2f);
        float absS1 = Mathf.Abs(s1);

        Vector3 walkPos = new Vector3(
            s2 * WalkPosAmplitude.x,
            absS1 * WalkPosAmplitude.y,
            Mathf.Cos(WalkPhase * 2f) * WalkPosAmplitude.z
        ) * (WalkBlend * weight);

        Vector3 walkRot = new Vector3(
            -absS1 * WalkRotAmplitude.x,
            s2 * WalkRotAmplitude.y,
            -s1 * WalkRotAmplitude.z
        ) * (WalkBlend * weight);

        // IdleWagging

        float idleBlend = 1f - Mathf.Clamp01(speed01 * 3f);
        IdlePhase += dt * IdleFrequency;

        Vector3 idlePos = new Vector3(
            Mathf.Sin(IdlePhase) * IdlePosAmplitude.x,
            (Mathf.Sin(IdlePhase * 0.5f) * 0.5f + 0.5f) * IdlePosAmplitude.y,
            Mathf.Cos(IdlePhase) * IdlePosAmplitude.z
        ) * idleBlend * weight * 0.8f;

        Vector3 idleRot = new Vector3(
            -Mathf.Abs(Mathf.Sin(IdlePhase)) * IdleRotAmplitude.x,
            Mathf.Sin(IdlePhase * 1.5f) * IdleRotAmplitude.y,
            -Mathf.Sin(IdlePhase) * IdleRotAmplitude.z
        ) * idleBlend * weight * 0.8f;

        if (AnimState != HandAnimState.None)
        {
            AnimTimer += dt;
            float t = AnimDuration > 1e-6f ? Mathf.Clamp01(AnimTimer / AnimDuration) : 1f;
            float s = t * t * (3f - 2f * t);

            AnimPosOffset = Vector3.LerpUnclamped(AnimStartPos, AnimTargetPos, s);
            AnimRotOffsetEuler = Vector3.LerpUnclamped(AnimStartRotEuler, AnimTargetRotEuler, s);

            if (t >= 1f) AnimState = HandAnimState.None;
        }
        //AI Formular//
        P.localPosition = BaseLocalPos + AnimPosOffset + walkPos + idlePos;
        R.localRotation = Quaternion.Euler(BaseLocalEuler + AnimRotOffsetEuler + walkRot + idleRot);
        //AI Formular//
    }


    //OpenFireAndSupress
    public void SuppressSwayOnFire()
    {
        if (!StopSwayOnFire) return;
        FireSuppressTimer = Mathf.Max(FireSuppressTimer, FireSuppressTime);
        FireRecoverProgress = 0f;
    }

    public void SetLocomotionState(float moveSpeed, bool isGrounded, float ads01 = 1f)
    {
        ExternalMoveSpeed = Mathf.Max(0f, moveSpeed);
        ExternalGrounded = isGrounded;
        AdsWeight = Mathf.Clamp01(ads01);
    }

    public void SetMoveInput01(Vector2 move01, bool isGrounded, float ads01 = 1f)
    {
        SetLocomotionState(move01.magnitude * ReferenceMoveSpeed, isGrounded, ads01);
    }

    public void ResetPose()
    {
        WalkBlend = 0f;
        WalkBlendVel = 0f;
        WalkPhase = 0f;
        IdlePhase = 0f;

        FireSuppressTimer = 0f;
        FireRecoverProgress = 1f;

        AnimState = HandAnimState.None;
        AnimTimer = 0f;
        AnimDuration = 0f;
        AnimPosOffset = Vector3.zero;
        AnimRotOffsetEuler = Vector3.zero;

        P.localPosition = BaseLocalPos;
        R.localRotation = Quaternion.Euler(BaseLocalEuler);
    }


    // LowerGun
    public void LowerForReload(float? duration = null, float? distance = null, float? pitchDeg = null)
    {
        float d = duration ?? LowerDuration;
        float dist = distance ?? LowerDistance;
        float pitch = pitchDeg ?? AnimPitchDeg;

        AnimStartPos = AnimPosOffset;
        AnimStartRotEuler = AnimRotOffsetEuler;

        AnimTargetPos = new Vector3(0f, -Mathf.Abs(dist), 0f);
        AnimTargetRotEuler = new Vector3(pitch, 0f, 0f);

        AnimTimer = 0f;
        AnimDuration = Mathf.Max(0.01f, d);
        AnimState = HandAnimState.Lowering;
    }

    public void RaiseFromLow(float? duration = null)
    {
        float d = duration ?? RaiseDuration;

        AnimStartPos = AnimPosOffset;
        AnimStartRotEuler = AnimRotOffsetEuler;

        AnimTargetPos = Vector3.zero;
        AnimTargetRotEuler = Vector3.zero;

        AnimTimer = 0f;
        AnimDuration = Mathf.Max(0.01f, d);
        AnimState = HandAnimState.Raising;
    }

    public void SnapToLow()
    {
        AnimState = HandAnimState.None;
        AnimTimer = 0f;
        AnimDuration = 0f;

        AnimPosOffset = new Vector3(0f, -Mathf.Abs(LowerDistance), 0f);
        AnimRotOffsetEuler = new Vector3(AnimPitchDeg, 0f, 0f);
    }

    public void CancelHandAnimation()
    {
        AnimState = HandAnimState.None;
        AnimTimer = 0f;
        AnimDuration = 0f;

        AnimPosOffset = Vector3.zero;
        AnimRotOffsetEuler = Vector3.zero;
    }
}

