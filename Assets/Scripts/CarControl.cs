using JetBrains.Annotations;
using UnityEngine;

public class CarControl : MonoBehaviour
{
    [Header("Move")]
    public float Speed = 15f;        
    public float MaxSpeed = 25f;      
    public float BreakDamp = 3f;      

    [Header("Turn (MoveRotation)")]
    public float TurnSpeed = 120f;    
    //TurnSpeedCurve
    public AnimationCurve turnBySpeed = AnimationCurve.Linear(0f, 1f, 1f, 0.35f);

    [Header("AntiDrift")]
    public float SideDamp = 8f;       
    public float VerticalDamp = 2f;   
    public float ExtraYawDamp = 2f; 

    [Header("RB")]
    public float LinearDamping = 0.2f;
    public float AngularDamping = 0.5f;
    public Rigidbody RB;

    [Header("Refs")]
    public GameObject Player;
    void Awake()
    {
        RB = GetComponent<Rigidbody>();
        RB.useGravity = false;
        RB.linearDamping = LinearDamping;
        RB.angularDamping = AngularDamping;
        RB.constraints = RigidbodyConstraints.FreezeRotation | RigidbodyConstraints.FreezePositionY;
        RB.interpolation = RigidbodyInterpolation.Interpolate;
        RB.collisionDetectionMode = CollisionDetectionMode.Continuous;

        Player = GameObject.FindGameObjectWithTag("Player");
    }

    void FixedUpdate()
    {
        if (Player.GetComponent<PlayerGetInCar>().IsMounted)
        {
            RB.linearDamping = 1;

            //Forwar&Backward
            float move = Input.GetAxis("Vertical");
            Vector3 fwd = transform.forward;
            float speedAlong = Vector3.Dot(RB.linearVelocity, fwd);
            //SpeedLimit
            if (Mathf.Abs(speedAlong) > MaxSpeed && Mathf.Sign(speedAlong) == Mathf.Sign(move))
            {
                move = 0f;
            }
            RB.AddForce(fwd * move * Speed, ForceMode.Acceleration);

            //Break
            if (Mathf.Approximately(move, 0f))
            {
                Vector3 forwardVel = fwd * speedAlong;
                RB.AddForce(-forwardVel * BreakDamp, ForceMode.Acceleration);
            }

            Vector3 v = RB.linearVelocity;
            Vector3 right = transform.right;
            Vector3 up = Vector3.up;

            // L&R
            float sideSpeed = Vector3.Dot(v, right);
            RB.AddForce(-right * sideSpeed * SideDamp, ForceMode.Acceleration);

            float vertSpeed = Vector3.Dot(v, up);
            RB.AddForce(-up * vertSpeed * VerticalDamp, ForceMode.Acceleration);

            //ShuaiJian
            float steer = Input.GetAxis("Horizontal");
            float speedMag = RB.linearVelocity.magnitude;
            float speedRatio = Mathf.Clamp01(speedMag / Mathf.Max(1f, MaxSpeed));
            float steerScale = turnBySpeed.Evaluate(speedRatio); 

            float yawDeg = steer * TurnSpeed * steerScale * Time.fixedDeltaTime;

            if (Mathf.Abs(yawDeg) > 0.0001f)
            {
                Quaternion dq = Quaternion.Euler(0f, yawDeg, 0f);
                RB.MoveRotation(RB.rotation * dq);
            }
            else
            {
                float yawVel = RB.angularVelocity.y;
                RB.AddTorque(Vector3.up * (-yawVel * ExtraYawDamp), ForceMode.Acceleration);
            }
        }
        else
        {
            RB.linearDamping = Mathf.MoveTowards(RB.linearDamping, 100, 20f * Time.deltaTime);
        }

    }
}
