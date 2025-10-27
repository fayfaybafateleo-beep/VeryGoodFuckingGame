using UnityEngine;

public class WeaponWaging : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [Header("Targets (optional)")]
    public Transform positionTarget; // 为空则用自身
    public Transform rotationTarget; // 为空则用自身

    [Header("Walk Sway / Bob")]
    [Tooltip("行走摆动频率（Hz）")]
    public float walkFrequency = 9.5f;
    [Tooltip("位置摆动幅度（局部米）：X左右、Y上下、Z前后")]
    public Vector3 walkPosAmplitude = new Vector3(0.006f, 0.012f, 0.004f);
    [Tooltip("旋转摆动幅度（度）：X俯仰、Y摇头、Z侧倾")]
    public Vector3 walkRotAmplitude = new Vector3(1.4f, 2.0f, 0.8f);

    [Header("Blending")]
    [Tooltip("从静止到满摆动的平滑时间（秒）")]
    public float blendSmoothTime = 0.12f;
    [Tooltip("把速度归一化所用的参考速度（m/s），达到这速度基本视为满摆动")]
    public float referenceMoveSpeed = 4.0f;

    [Header("ADS Scaling (optional)")]
    [Tooltip("开镜(瞄准)时的摆动权重（1=不衰减，0=完全关闭摆动）")]
    [Range(0f, 1f)] public float adsWeight = 1f;

    [Header("Idle Bob (optional)")]
    [Tooltip("站立时的极轻微呼吸/轻摆（米/度），当速度≈0时生效")]
    public Vector3 idlePosAmplitude = new Vector3(0.0015f, 0.003f, 0.001f);
    public Vector3 idleRotAmplitude = new Vector3(0.35f, 0.45f, 0.2f);
    public float idleFrequency = 2.2f;

    [Header("Fire Suppress (stop sway on fire)")]
    [Tooltip("开火后是否暂时停止摆动")]
    public bool stopSwayOnFire = true;
    [Tooltip("压制时长（秒）：这段时间内摆动为0")]
    public float fireSuppressTime = 0.12f;
    [Tooltip("压制结束后，从0恢复到正常所需时间（秒）")]
    public float fireRecoverTime = 0.10f;

    [Header("Time")]
    [Tooltip("使用不受 timeScale 影响的时间（慢动作时保持节奏）")]
    public bool useUnscaledTime = true;

    public CharacterController characterController;
    [Tooltip("没有 CC 时：外部手动设置移动速度（m/s）")]
    public float externalMoveSpeed;
    [Tooltip("没有 CC 时：外部手动设置是否在地面")]
    public bool externalGrounded = true;

    // —— 内部状态 ——
    Transform P => positionTarget ? positionTarget : transform;
    Transform R => rotationTarget ? rotationTarget : transform;

    Vector3 _baseLocalPos, _baseLocalEuler;
    float _walkPhase;
    float _walkBlend;         // 0..1
    float _walkBlendVel;      // SmoothDamp 缓存
    float _idlePhase;

    // Fire suppress
    float _fireSuppressTimer;     // >0 = 压制阶段（摆动为0）
    float _fireRecoverProgress;   // 0→1 恢复插值

    // ===== 新增：装备/换弹 动画层（最终输出叠加） =====
    [Header("Equip / Reload Hand Animation  // ===== 新增 =====")]
    [Tooltip("下降到低位的距离（局部 -Y 米）")]
    public float lowerDistance = 0.12f;                      // ===== 新增 =====
    [Tooltip("下降/抬起时的附加俯仰（度，负值为枪口下压）")]
    public float animPitchDeg = -8f;                          // ===== 新增 =====
    [Tooltip("下降动画时长（秒）")]
    public float lowerDuration = 0.18f;                       // ===== 新增 =====
    [Tooltip("抬起动画时长（秒）")]
    public float raiseDuration = 0.20f;                       // ===== 新增 =====

    // 动画层内部状态
    enum HandAnimState { None, Lowering, Raising }            // ===== 新增 =====
    HandAnimState _animState = HandAnimState.None;            // ===== 新增 =====
    Vector3 _animPosOffset;                                   // ===== 新增 =====
    Vector3 _animRotOffsetEuler;                              // ===== 新增 =====
    Vector3 _animStartPos, _animTargetPos;                    // ===== 新增 =====
    Vector3 _animStartRotEuler, _animTargetRotEuler;          // ===== 新增 =====
    float _animTimer, _animDuration;                          // ===== 新增 =====


    void Awake()
    {
        _baseLocalPos = P.localPosition;
        _baseLocalEuler = R.localEulerAngles;
        characterController =GameObject.FindGameObjectWithTag("Player").GetComponentInParent<CharacterController>();
    }

    void LateUpdate()
    {
        float dt = useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
        float time = useUnscaledTime ? Time.unscaledTime : Time.time;
        if (dt <= 0f) return;

        // —— 读取移动状态 —— //
        float speed = 0f;
        bool grounded = externalGrounded;

        if (characterController)
        {
            Vector3 v = characterController.velocity; v.y = 0f;
            speed = v.magnitude;
            grounded = characterController.isGrounded;
        }
        else
        {
            speed = Mathf.Max(0f, externalMoveSpeed);
        }

        // —— 目标摆动强度（地面才摆动） —— //
        float speed01 = Mathf.Clamp01(speed / Mathf.Max(0.0001f, referenceMoveSpeed));
        float targetBlend = grounded ? speed01 : 0f;

        // —— 开火压制 —— //
        if (stopSwayOnFire)
        {
            if (_fireSuppressTimer > 0f)
            {
                _fireSuppressTimer -= dt;
                _walkBlend = 0f;
                _fireRecoverProgress = 0f;
            }
            else
            {
                // 先计算“正常情况下”的 _walkBlend，再把它乘以恢复进度
                _walkBlend = Mathf.SmoothDamp(_walkBlend, targetBlend, ref _walkBlendVel, blendSmoothTime);
                if (fireRecoverTime > 0f && _walkBlend > 0f && _fireRecoverProgress < 1f)
                {
                    _fireRecoverProgress = Mathf.Clamp01(_fireRecoverProgress + dt / fireRecoverTime);
                    _walkBlend *= _fireRecoverProgress;
                }
                else
                {
                    _fireRecoverProgress = 1f;
                }
            }
        }
        else
        {
            _walkBlend = Mathf.SmoothDamp(_walkBlend, targetBlend, ref _walkBlendVel, blendSmoothTime);
            _fireRecoverProgress = 1f;
        }

        // ADS 衰减
        float weight = Mathf.Clamp01(adsWeight);

        // —— 推进相位 —— //
        float phaseSpeed = Mathf.Lerp(0.2f, 1f, speed01) * walkFrequency; // 低速不至于停摆
        _walkPhase += dt * phaseSpeed;

        // —— 计算行走摆动 —— //
        float s1 = Mathf.Sin(_walkPhase);
        float s2 = Mathf.Sin(_walkPhase * 2f);
        float absS1 = Mathf.Abs(s1);

        // 位置：X 左右 = sin(2t); Y 上下 = |sin(t)|; Z 前后 = cos(2t)
        Vector3 walkPos =
            new Vector3(
                s2 * walkPosAmplitude.x,
                absS1 * walkPosAmplitude.y,
                Mathf.Cos(_walkPhase * 2f) * walkPosAmplitude.z
            ) * (_walkBlend * weight);

        // 旋转：X 俯仰 = -|sin(t)|; Y 摇头 = sin(2t); Z 侧倾 = -sin(t)
        Vector3 walkRot =
            new Vector3(
                -absS1 * walkRotAmplitude.x,
                s2 * walkRotAmplitude.y,
                -s1 * walkRotAmplitude.z
            ) * (_walkBlend * weight);

        // —— Idle 轻摆（速度≈0时）—— //
        float idleBlend = 1f - Mathf.Clamp01(speed01 * 3f); // 速度稍有就快速衰减
        _idlePhase += dt * idleFrequency;
        Vector3 idlePos = new Vector3(
            Mathf.Sin(_idlePhase) * idlePosAmplitude.x,
            (Mathf.Sin(_idlePhase * 0.5f) * 0.5f + 0.5f) * idlePosAmplitude.y,
            Mathf.Cos(_idlePhase) * idlePosAmplitude.z
        ) * idleBlend * weight * 0.8f;

        Vector3 idleRot = new Vector3(
            -Mathf.Abs(Mathf.Sin(_idlePhase)) * idleRotAmplitude.x,
            Mathf.Sin(_idlePhase * 1.5f) * idleRotAmplitude.y,
            -Mathf.Sin(_idlePhase) * idleRotAmplitude.z
        ) * idleBlend * weight * 0.8f;
        if (_animState != HandAnimState.None)                 // ===== 新增 =====
        {
            _animTimer += dt;
            float t = (_animDuration > 1e-6f) ? Mathf.Clamp01(_animTimer / _animDuration) : 1f;
            float s = t * t * (3f - 2f * t); // SmoothStep

            _animPosOffset = Vector3.LerpUnclamped(_animStartPos, _animTargetPos, s);
            _animRotOffsetEuler = Vector3.LerpUnclamped(_animStartRotEuler, _animTargetRotEuler, s);

            if (t >= 1f) _animState = HandAnimState.None;
        }

        // —— 应用到局部变换（叠加在动画之后）—— //
        P.localPosition = _baseLocalPos + _animPosOffset + walkPos + idlePos;                        // ===== 修改 =====
        R.localRotation = Quaternion.Euler(_baseLocalEuler + _animRotOffsetEuler + walkRot + idleRot); // ===== 修改 =====
    }

    /// 在“开火”事件里调用：暂停摆动并计时恢复
    public void SuppressSwayOnFire()
    {
        if (!stopSwayOnFire) return;
        _fireSuppressTimer = Mathf.Max(_fireSuppressTimer, fireSuppressTime);
        _fireRecoverProgress = 0f;
    }

    /// 没有 CharacterController 时，可从外部喂速度与落地状态（m/s, grounded）
    public void SetLocomotionState(float moveSpeed, bool isGrounded, float ads01 = 1f)
    {
        externalMoveSpeed = Mathf.Max(0f, moveSpeed);
        externalGrounded = isGrounded;
        adsWeight = Mathf.Clamp01(ads01);
    }

    /// 输入为 0~1 的移动向量（如你的输入轴），内部按 referenceMoveSpeed 估算速度
    public void SetMoveInput01(Vector2 move01, bool isGrounded, float ads01 = 1f)
    {
        SetLocomotionState(move01.magnitude * referenceMoveSpeed, isGrounded, ads01);
    }

    /// 重置到初始局部姿态
    public void ResetPose()
    {
        _walkBlend = 0f; _walkBlendVel = 0f;
        _walkPhase = 0f; _idlePhase = 0f;
        _fireSuppressTimer = 0f; _fireRecoverProgress = 1f;

        // ===== 修改：重置时也清掉动画层 =====
        _animState = HandAnimState.None; _animTimer = 0f; _animDuration = 0f; // ===== 修改 =====
        _animPosOffset = Vector3.zero; _animRotOffsetEuler = Vector3.zero;    // ===== 修改 =====

        P.localPosition = _baseLocalPos;
        R.localRotation = Quaternion.Euler(_baseLocalEuler);
    }
    /// <summary>从默认位姿“下降”到低位（用于换弹时的收枪）。</summary>
    public void LowerForReload(float? duration = null, float? distance = null, float? pitchDeg = null) // ===== 新增 =====
    {
        float d = duration ?? lowerDuration;
        float dist = distance ?? lowerDistance;
        float pitch = pitchDeg ?? animPitchDeg;

        _animStartPos = _animPosOffset;
        _animStartRotEuler = _animRotOffsetEuler;

        _animTargetPos = new Vector3(0f, -Mathf.Abs(dist), 0f);
        _animTargetRotEuler = new Vector3(pitch, 0f, 0f);

        _animTimer = 0f;
        _animDuration = Mathf.Max(0.01f, d);
        _animState = HandAnimState.Lowering;
    }

    /// <summary>从低位“抬回”默认位姿（用于切枪/换弹结束）。</summary>
    public void RaiseFromLow(float? duration = null)                                             // ===== 新增 =====
    {
        float d = duration ?? raiseDuration;

        _animStartPos = _animPosOffset;
        _animStartRotEuler = _animRotOffsetEuler;

        _animTargetPos = Vector3.zero;
        _animTargetRotEuler = Vector3.zero;

        _animTimer = 0f;
        _animDuration = Mathf.Max(0.01f, d);
        _animState = HandAnimState.Raising;
    }

    /// <summary>瞬间切到低位（用于新武器出场先在低位，然后抬起）。</summary>
    public void SnapToLow()                                                                      // ===== 新增 =====
    {
        _animState = HandAnimState.None;
        _animTimer = 0f; _animDuration = 0f;
        _animPosOffset = new Vector3(0f, -Mathf.Abs(lowerDistance), 0f);
        _animRotOffsetEuler = new Vector3(animPitchDeg, 0f, 0f);
    }

    /// <summary>打断动画并恢复默认姿态。</summary>
    public void CancelHandAnimation()                                                            // ===== 新增 =====
    {
        _animState = HandAnimState.None;
        _animTimer = 0f; _animDuration = 0f;
        _animPosOffset = Vector3.zero;
        _animRotOffsetEuler = Vector3.zero;
    }

}
