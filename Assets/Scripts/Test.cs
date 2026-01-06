using System.Collections.Generic;
using UnityEngine;

public class Test : MonoBehaviour
{


    [Header("Recoil")]
    public float KickSpeed = 30f;
    public float ReturnSpeed = 10f;
    public float MaxKick = 20f;

    float TargetKick;
    float CurrentKick;
    Quaternion baseLocalRot;

    void Awake()
    {
        baseLocalRot = transform.localRotation;
    }

    public void Kick(float amount)
    {
        TargetKick = Mathf.Clamp(TargetKick + amount, 0f, MaxKick);
    }

    void Update()
    {

        float dt = Time.deltaTime;
        CurrentKick = Mathf.Lerp(CurrentKick, TargetKick, 1f - Mathf.Exp(-KickSpeed * dt));
        TargetKick = Mathf.Lerp(TargetKick, 0f, 1f - Mathf.Exp(-ReturnSpeed * dt));

    }

    void LateUpdate()
    {
        // 抬头一般是 -pitch；如果方向反了，把 -currentKick 改成 +currentKick
        Quaternion recoilRot = Quaternion.Euler(-CurrentKick, 0f, 0);
        transform.localRotation = baseLocalRot * recoilRot;
    }

}
