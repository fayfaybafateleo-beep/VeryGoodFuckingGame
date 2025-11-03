using UnityEngine;

public class CarControl : MonoBehaviour
{
    [Header("Mount / Control")]
    public GameObject Player;
    public float accel = 15f;     // 推力
    public float maxSpeed = 25f;  // 最高前向速度
    public float coastDamp = 3f;  // 松油门时阻尼

    Rigidbody rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false;
        rb.linearDamping = 0.2f;
        rb.angularDamping = 0.5f;

        if (Player == null)
            Player = GameObject.FindGameObjectWithTag("Player");
    }

    void FixedUpdate()
    {
        if (!(Player && Player.GetComponent<PlayerGetInCar>()?.IsMounted == true))
            return;

        float move = Input.GetAxis("Vertical");
        Vector3 forward = transform.forward;

        // 当前速度沿车头方向的分量
        float speed = Vector3.Dot(rb.linearVelocity, forward);

        // 超速且同向加速时不再推力
        if (Mathf.Abs(speed) > maxSpeed && Mathf.Sign(speed) == Mathf.Sign(move))
            move = 0f;

        // 推力
        rb.AddForce(forward * move * accel, ForceMode.Acceleration);

        // 松油门阻尼
        if (Mathf.Approximately(move, 0f))
        {
            Vector3 forwardVel = forward * speed;
            rb.AddForce(-forwardVel * coastDamp, ForceMode.Acceleration);
        }
    }

}
