using UnityEngine;

public class PlayerAntiFall : MonoBehaviour
{
    [Header("FallData")]
    public float FallY = -10f;        
    public LayerMask GroundLayer;     
    public float GroundCheckOffset = 0.2f;  
    public float GroundCheckRadius = 0.3f;  

    public CharacterController Controller;
    public Vector3 LastSafePosition;


    void Start()
    {
        Controller = GetComponent<CharacterController>();
        LastSafePosition = transform.position;   
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
        if (transform.position.y < FallY)
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
