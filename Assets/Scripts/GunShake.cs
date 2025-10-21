using UnityEngine;

public class GunShake : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [Header("Targets (optional)")]
    public Transform positionTarget; // 为空则用自身
    public Transform rotationTarget; // 为空则用自身

    [Header("Recoil Kick (one-shot impulse)")]
    [Tooltip("后座位移（-Z 方向，米）")]
    public float kickBackDistance = 0.03f;
    [Tooltip("x=Pitch上仰(度), y=Yaw左右(度)")]
    public Vector2 kickPitchYaw = new Vector2(4f, 1.5f);
    [Tooltip("Yaw 左右随机翻转概率")]
    [Range(0, 1)] public float kickRandomYawSignChance = 0.5f;

    [Header("Spring (critically/over-damped)")]
    [Tooltip("位置回位刚度（越大回位越快）")]
    public float posSpringStiffness = 120f;
    [Tooltip("旋转回位刚度（越大回位越快）")]
    public float rotSpringStiffness = 140f;
    [Tooltip("位置阻尼比 ≥1 不振荡，=1 临界，>1 过阻尼更稳")]
    [Range(0.8f, 2f)] public float dampingRatioPos = 1.1f;
    [Tooltip("旋转阻尼比 ≥1 不振荡，=1 临界，>1 过阻尼更稳")]
    [Range(0.8f, 2f)] public float dampingRatioRot = 1.15f;

    [Header("High-Frequency Shake (Perlin)")]
    [Tooltip("开火后短暂的高频位移抖动（米）")]
    public float noisePosAmp = 0.003f;
    [Tooltip("开火后短暂的高频旋转抖动（度）")]
    public float noiseRotAmp = 0.6f;
    [Tooltip("噪声频率（Hz）")]
    public float noiseFrequency = 18f;
    [Tooltip("位移噪声各轴权重")]
    public Vector3 noiseAxesPos = new Vector3(1f, 1f, 0.2f);
    [Tooltip("旋转噪声各轴权重（Pitch/Yaw/Roll）")]
    public Vector3 noiseAxesRot = new Vector3(0.4f, 0.6f, 1f);

    [Header("Shake Envelope (only after fire)")]
    [Tooltip("开火后保持满强度的时间")]
    public float shakeHoldTime = 0.06f;
    [Tooltip("从满强度淡出到0的时间")]
    public float shakeFadeTime = 0.18f;

    [Header("Idle (optional)")]
    [Tooltip("是否在待机也给极轻微抖动")]
    public bool enableIdleNoise = false;
    [Tooltip("待机位移抖动幅度（米）")]
    public float idleNoisePosAmp = 0.0006f;
    [Tooltip("待机旋转抖动幅度（度）")]
    public float idleNoiseRotAmp = 0.12f;
    [Tooltip("待机噪声频率（Hz）")]
    public float idleNoiseFrequency = 6f;

    [Header("Time")]
    [Tooltip("使用不受 Time.timeScale 影响的时间（慢动作时保持同手感）")]
    public bool useUnscaledTime = false;

    // 内部状态
    Vector3 _baseLocalPos, _posOffset, _posVel;
    Vector3 _baseLocalEuler, _rotOffset, _rotVel; // 小角度用欧拉即可
    float _seedX, _seedY, _seedZ;

    // 开火噪声强度（0-1），只影响噪声，不影响弹簧
    float _shakeT;         // 当前强度
    float _shakeVel;       // SmoothDamp 的速度缓存
    float _holdTimer;      // 满强保持计时

    Transform P => positionTarget ? positionTarget : transform;
    Transform R => rotationTarget ? rotationTarget : transform;



    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        _baseLocalPos = P.localPosition;
        _baseLocalEuler = R.localEulerAngles;
        _seedX = Random.value * 1000f;
        _seedY = Random.value * 1000f;
        _seedZ = Random.value * 1000f;
    }

    void LateUpdate()
    {
        float dt = useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
        if (dt <= 0f) return;

        float time = useUnscaledTime ? Time.unscaledTime : Time.time;

        // -----------------------
        // 1) 更新弹簧（只对后座偏移起作用，不吃噪声）
        CriticallyDamped(ref _posOffset, ref _posVel, Vector3.zero, posSpringStiffness, dampingRatioPos, dt);
        CriticallyDamped(ref _rotOffset, ref _rotVel, Vector3.zero, rotSpringStiffness, dampingRatioRot, dt);

        // -----------------------
        // 2) 计算噪声（开火噪声 + 可选待机噪声），只在最终输出时相加
        Vector3 noisePos = Vector3.zero;
        Vector3 noiseRot = Vector3.zero;

        // 待机轻噪（可选）
        if (enableIdleNoise && idleNoisePosAmp > 0f || enableIdleNoise && idleNoiseRotAmp > 0f)
        {
            float tIdle = time * idleNoiseFrequency;
            Vector3 nIdle = new Vector3(
                (Mathf.PerlinNoise(_seedX, tIdle) - 0.5f) * 2f,
                (Mathf.PerlinNoise(_seedY, tIdle) - 0.5f) * 2f,
                (Mathf.PerlinNoise(_seedZ, tIdle) - 0.5f) * 2f
            );
            noisePos += Vector3.Scale(nIdle, noiseAxesPos) * idleNoisePosAmp;
            noiseRot += Vector3.Scale(nIdle, noiseAxesRot) * idleNoiseRotAmp;
        }

        // 开火高频噪声（按 _shakeT 叠加）
        if (_shakeT > 0.0001f && (noisePosAmp > 0f || noiseRotAmp > 0f))
        {
            float tFire = time * noiseFrequency;
            Vector3 nFire = new Vector3(
                (Mathf.PerlinNoise(_seedX + 111f, tFire) - 0.5f) * 2f,
                (Mathf.PerlinNoise(_seedY + 222f, tFire) - 0.5f) * 2f,
                (Mathf.PerlinNoise(_seedZ + 333f, tFire) - 0.5f) * 2f
            );
            noisePos += Vector3.Scale(nFire, noiseAxesPos) * noisePosAmp * _shakeT;
            noiseRot += Vector3.Scale(nFire, noiseAxesRot) * noiseRotAmp * _shakeT;
        }

        // -----------------------
        // 3) 更新开火噪声包络（hold 后再淡出）
        if (_holdTimer > 0f)
        {
            _holdTimer -= dt;
            // 保持满强
            _shakeT = 1f;
        }
        else
        {
            // 从 1 平滑淡出到 0
            _shakeT = Mathf.SmoothDamp(_shakeT, 0f, ref _shakeVel, shakeFadeTime);
            if (_shakeT < 0.0001f) _shakeT = 0f;
        }

        // -----------------------
        // 4) 应用到最终局部变换（叠加在动画之后）
        P.localPosition = _baseLocalPos + _posOffset + noisePos;
        R.localRotation = Quaternion.Euler(_baseLocalEuler + _rotOffset + noiseRot);
    }
    public void AddRecoil(float strength = 1f)
    {
        // 位移后座：往 -Z 推一小段（转为速度脉冲）
        _posVel += new Vector3(0f, 0f, -kickBackDistance * strength * 60f);

        // 旋转后座：上仰 + 随机左右
        float yawSign = (Random.value < kickRandomYawSignChance) ? -1f : 1f;
        _rotVel += new Vector3(kickPitchYaw.x * strength * 30f,
                               kickPitchYaw.y * yawSign * strength * 30f,
                               0f);

        // 开火噪声：立刻满强，并保持一小段时间后淡出
        _shakeT = 1f;
        _holdTimer = shakeHoldTime;
        // 重置淡出速度缓存，避免上一次 SmoothDamp 的残留影响
        _shakeVel = 0f;
    }

    // 二阶系统临界/过阻尼积分：x'' + c x' + k x = 0，c = 2 * zeta * sqrt(k)
    static void CriticallyDamped(ref Vector3 x, ref Vector3 v, Vector3 target, float k, float zeta, float dt)
    {
        float w = Mathf.Sqrt(Mathf.Max(k, 1e-4f));
        float c = 2f * Mathf.Max(zeta, 0f) * w;
        Vector3 a = -k * (x - target) - c * v;
        v += a * dt;
        x += v * dt;
    }
}
