using UnityEngine;

public class PlayerAntiFall : MonoBehaviour
{
    [Header("FallData")]
    public float FallY = -10f;        // 掉出平台高度阈值（比场景最低平台再低一点）
    public LayerMask GroundLayer;     // 地面/平台使用的 Layer
    public float GroundCheckOffset = 0.2f;  // 检测脚下的偏移
    public float GroundCheckRadius = 0.3f;  // 检测脚下的小球半径

    public CharacterController Controller;
    public Vector3 LastSafePosition;

    // 如果你的移动脚本里有垂直速度，可以在这里引用：
    // public PlayerMovement movement;   // 举例：你自己的移动脚本，里面有 velocity.y 之类

    void Start()
    {
        Controller = GetComponent<CharacterController>();
        LastSafePosition = transform.position;   // 起始点当作一个安全点
    }

    void Update()
    {
        RecordSafePosition();
        CheckFall();
    }


    void RecordSafePosition()
    {
        // OnlyRecordOnGround
        if (!Controller.isGrounded) return;

        Vector3 checkPos = transform.position + Vector3.down * GroundCheckOffset;
        bool onGround = Physics.CheckSphere(checkPos, GroundCheckRadius, GroundLayer);

        if (onGround)
        {
            LastSafePosition = transform.position;
        }
    }

    void CheckFall()
    {
        if (transform.position.y < FwallY)
        {
            Respawn();
        }
    }

    //TPBack
    public void Respawn()
    {
        Controller.enabled = false;                
        transform.position = LastSafePosition;
        Controller.enabled = true;

    }
}
