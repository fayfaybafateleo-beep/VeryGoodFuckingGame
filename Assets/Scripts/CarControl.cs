using JetBrains.Annotations;
using UnityEngine;
using Unity.Cinemachine;
using System.Collections.Generic;

public class CarControl : MonoBehaviour
{
    [Header("Move")]
    public float Speed = 15f;        
    public float MaxSpeed = 25f;      
    public float BreakDamp = 3f;

    [Header("Boost (Shift)")]
    public float BoostForceMultiplier = 1.6f;  
    public float BoostMaxSpeedMultiplier = 1.3f; 
    public ParticleSystem SpeedLine;

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
    public bool CanGetDown;

    [Header("Impulse")]
    public CinemachineImpulseSource Impulse;   
    public float BoostImpulseInterval = 0.06f; 
    public float BoostImpulseGain = 1.0f;     
    private float _nextImpulseTime;

    [Header("PanelLight")]
    public List<Renderer> Renderers;
    public Color TargetColor = Color.green;
    public Color BaseColor;

    MaterialPropertyBlock _mpb;
    void Awake()
    {
        RB = GetComponent<Rigidbody>();
        RB.useGravity = false;
        RB.linearDamping = LinearDamping;
        RB.angularDamping = AngularDamping;
        RB.constraints = RigidbodyConstraints.FreezeRotation | RigidbodyConstraints.FreezePositionY;
        RB.interpolation = RigidbodyInterpolation.Interpolate;
        RB.collisionDetectionMode = CollisionDetectionMode.Continuous;

        _mpb = new MaterialPropertyBlock();

        Player = GameObject.FindGameObjectWithTag("Player");

        Renderers[0].GetPropertyBlock(_mpb);
        BaseColor = Renderers[0].sharedMaterial.GetColor("_BaseColor");
        SpeedLine.Stop();
    }

    void FixedUpdate()
    {
        if (Player.GetComponent<PlayerGetInCar>().IsMounted)
        {
            RB.linearDamping = 1;

            bool boosting = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
            float speedMul = boosting ? BoostForceMultiplier : 1f;
            float maxSpeedMul = boosting ? BoostMaxSpeedMultiplier : 1f;
            float curMaxSpeed = MaxSpeed * maxSpeedMul;

            //Forwar&Backward
            float move = Input.GetAxis("Vertical");
            Vector3 fwd = transform.forward;
            float speedAlong = Vector3.Dot(RB.linearVelocity, fwd);

            //SpeedLimit
            if (Mathf.Abs(speedAlong) > curMaxSpeed && Mathf.Sign(speedAlong) == Mathf.Sign(move))
            {
                move = 0f;
            }

            //ChangeColor
            if (boosting)
            {
                ChangeColor();
                SpeedLine.Play();
            }
            else
            {
                ChangeBackColor();
                SpeedLine.Stop();
            }

            RB.AddForce(fwd * move * Speed * speedMul, ForceMode.Acceleration);

            //Break
            if (Mathf.Approximately(move, 0f))
            {
                Vector3 forwardVel = fwd * speedAlong;
                RB.AddForce(-forwardVel * BreakDamp, ForceMode.Acceleration);
            }

            //ScreenShake
            if (boosting)
            {
                SendBoostImpulse();
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
            float speedRatio = Mathf.Clamp01(speedMag / Mathf.Max(1f, curMaxSpeed));
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
            RB.linearDamping = Mathf.MoveTowards(RB.linearDamping, 200, 40f * Time.deltaTime);
        }
        
    }

    public void ReGenerateCar()
    {
        this.transform.position = new Vector3(Player.transform.position.x, 0.96f, Player.transform.position.y);
    }

    public void SendBoostImpulse()
    {
 
        if (Time.time < _nextImpulseTime) return;
        _nextImpulseTime = Time.time + BoostImpulseInterval;
        Impulse.GenerateImpulse();
    }

    void ChangeColor()
    {
        foreach (Renderer r in Renderers)
        {
            r.GetPropertyBlock(_mpb);
            _mpb.SetColor("_BaseColor", TargetColor); 
            r.SetPropertyBlock(_mpb);
        }
    }

    void ChangeBackColor()
    {
        foreach (Renderer r in Renderers)
        {
            r.GetPropertyBlock(_mpb);
            _mpb.SetColor("_BaseColor", BaseColor);
            r.SetPropertyBlock(_mpb);
        }
    }
}
