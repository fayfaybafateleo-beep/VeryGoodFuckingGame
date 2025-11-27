using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyBehaviour : MonoBehaviour
{
    public string Name;

    [Header("Target")]
    public GameObject Target;
    public NavMeshAgent Agent;

    [Header("Movement")]
    [UnityEngine.Range(0, 100)] public float MoveForce = 10f;
    [UnityEngine.Range(0, 100)] public float MaxSpeed = 5f;
    [UnityEngine.Range(0, 360)] public float AngleOffset;
    [UnityEngine.Range(0, 100)] public float TurningSpeed;

    [Header("Animation")]
    public Animator EnemyAnimator;

    [Header("Combat")]
    [UnityEngine.Range(0, 1000)] public float AttackRange = 2.0f;
    [UnityEngine.Range(0, 1000)] public float AttackRate = 2f;
    public float AttackRateTimer = 0f;
    public float Damage;
    public int SlugCount=1;
    public Transform FirePoint;
    public GameObject EnemyBullet;
    public GameObject EnemyMuzzleFlash;
    public bool Lock=false;

    public float FacingAngleTolerance = 12f;
    public bool XiaoTou = true;
    public LayerMask LineOfSightMask = ~0; 

    [Header("MOA")]
    public float VerticalSpreadAngle = 6f;
    public float HorizontalSpreadAngle = 6f;

    [Header("Rigidbody")]
    public Rigidbody Rigidbody;

    [Header("ImportantParts")]
    public List<GameObject> ImportantPartList;

    public bool Shocked = false;


    public enum EnemyState 
    {
        Moving, 
        Attack, 
        Shock, 
        Die
    }
    public EnemyState ES;

    [Header("EnemyHealth")]
    public EnemyHealth EH;

    [Header("SmartMovement")]
    [UnityEngine.Range(0, 100)] 
    public float StrafeBeginDistance = 6f;
    [UnityEngine.Range(0, 20)] 
    public float StrafeStrength = 1.0f;
    [UnityEngine.Range(0.1f, 5f)] 
    public float StrafeChangeIntervalMin = 1.2f;
    [UnityEngine.Range(0.1f, 5f)] 
    public float StrafeChangeIntervalMax = 2.5f;
    [UnityEngine.Range(0.1f, 5f)] 
    public float SeparationRadius = 1.2f;
    [UnityEngine.Range(0f, 20f)] 
    public float SeparationStrength = 3.0f;
    public LayerMask EnemyLayer;

    float StrafeSign = 1f;
    float StrafeTimer = 0f;
    float NextStrafeFlip = 1.5f;
    const float EPS = 0.0001f;


    [Header("Panic")]
    public GameObject PanicFantom;
    void Start()
    {
        Target = GameObject.FindGameObjectWithTag("Player");

        Agent.updatePosition = false;
        Agent.updateRotation = false;
        Agent.avoidancePriority = Random.Range(20, 80);

        EH = GetComponent<EnemyHealth>();
    }

    void FixedUpdate()
    {
        // Reset attack timer when not attacking
        if (ES == EnemyState.Die || ES == EnemyState.Shock)
        {
            AttackRateTimer = 0;
        }

        if (AttackRateTimer > AttackRate)
        {
            AttackRateTimer = AttackRate;
        }
        // Shocked check
        bool allDestroyed = !ImportantPartList.Exists(p => p);
        if (allDestroyed && !Shocked)
        {
            if (ES != EnemyState.Die)
            {
                EnemyShock();
            }
            Shocked = true;
        }

        switch (ES)
        {
            case EnemyState.Moving:
                if (Target == null) return;


                EnemyAnimator.SetBool("Run", true);
                Agent.SetDestination(Target.transform.position);
                Agent.nextPosition = Rigidbody.position;

                Vector3 dir = Agent.desiredVelocity.normalized;

                // Smart Move
                Vector3 toPlayer = Target.transform.position - transform.position;
                toPlayer.y = 0f;
                float sqrDist = toPlayer.sqrMagnitude;

                Vector3 strafe = Vector3.zero;
                if (sqrDist <= Mathf.Max(StrafeBeginDistance * StrafeBeginDistance, AttackRange * AttackRange * 1.5625f))
                {
                    Vector3 forward = toPlayer.sqrMagnitude > EPS ? toPlayer.normalized : Vector3.zero;
                    Vector3 right = Vector3.Cross(Vector3.up, forward).normalized;
                    strafe = right * StrafeSign * StrafeStrength;
                }

                Vector3 separation = Vector3.zero;
                if (SeparationRadius > 0.01f)
                {
                    Collider[] hits = Physics.OverlapSphere(transform.position, SeparationRadius, EnemyLayer, QueryTriggerInteraction.Ignore);
                    foreach (var hit in hits)
                    {
                        if (hit.attachedRigidbody == null || hit.attachedRigidbody == Rigidbody) continue;
                        Vector3 away = transform.position - hit.transform.position;
                        away.y = 0f;
                        float dist = away.magnitude + EPS;
                        separation += away.normalized * (SeparationStrength / dist);
                    }
                }

                Vector3 blended = dir + strafe + separation;
                if (blended.sqrMagnitude > 1f) blended.Normalize();

                Rigidbody.AddForce(blended * MoveForce, ForceMode.Acceleration);

                if (Rigidbody.linearVelocity.magnitude > MaxSpeed)
                    Rigidbody.linearVelocity = Rigidbody.linearVelocity.normalized * MaxSpeed;

                // Face player
                Vector3 faceDir = Target.transform.position - transform.position;
                faceDir.y = 0f;
                float angle = Mathf.Atan2(faceDir.x, faceDir.z) * Mathf.Rad2Deg + 180f;
                Quaternion rot = Quaternion.Euler(0f, angle, 0f);
                transform.rotation = Quaternion.Slerp(transform.rotation, rot, Time.unscaledDeltaTime * TurningSpeed);

                // Strafe timer
                StrafeTimer += Time.fixedDeltaTime;
                if (StrafeTimer >= NextStrafeFlip)
                {
                    StrafeTimer = 0f;
                    StrafeSign *= -1f;
                    NextStrafeFlip = Random.Range(StrafeChangeIntervalMin, StrafeChangeIntervalMax);
                }

                //FireCountDown
                AttackRateTimer += Time.deltaTime;

                // EnterAttack Mode
                float atkDist = (Target.transform.position - transform.position).sqrMagnitude;
                if (atkDist <= AttackRange * AttackRange)
                    EnterAttack();
                break;

            case EnemyState.Attack:
                EnemyAnimator.SetBool("Run", false);

                //Facing To Player
                Vector3 dir2 = Target.transform.position - transform.position;
                dir2.y = 0f;
                float angle2 = Mathf.Atan2(dir2.x, dir2.z) * Mathf.Rad2Deg + 180f;
                Quaternion rot2 = Quaternion.Euler(0f, angle2, 0f);
                transform.rotation = Quaternion.Slerp(transform.rotation, rot2, Time.unscaledDeltaTime * TurningSpeed);

                //FirePointAimming
                if (FirePoint != null && Target != null)
                {
                    Vector3 targetPos = Target.transform.position + Vector3.up * 1.15f;
                    Vector3 aimDir = targetPos - FirePoint.position;

                    //InCasePlayerIsTooClose
                    if (aimDir.sqrMagnitude > 0.0001f)
                    {
                        Quaternion aimRot = Quaternion.LookRotation(aimDir.normalized, Vector3.up);
                        FirePoint.rotation = aimRot;
                    }
                }

                if (Target != null && FirePoint!=null)
                {
                    //CheckIsFacing
                    Vector3 toTarget = Target.transform.position - transform.position;
                    toTarget.y = 0f;
                    Vector3 forward = -transform.forward;

                    float dot = Vector3.Dot(forward.normalized, toTarget.normalized);
                    float cosThreshold = Mathf.Cos(30f * Mathf.Deg2Rad); 

                    if (dot >= cosThreshold)
                    {
                        Lock = true;
                    }
                    else
                    {
                        Lock = false;
                    }
                }
                else
                {
                    Lock = false;
                }

                //FireCountDown
                AttackRateTimer += Time.deltaTime;

                if (AttackRateTimer >= AttackRate && Target != null && Lock)
                {
                    AttackRateTimer = 0;
                    EnemyAnimator.SetTrigger("Fire");

                    for (int i = 0; i < SlugCount; i++)
                    {
                        Vector2 c = Random.insideUnitCircle;

                        float yaw = c.x * HorizontalSpreadAngle;
                        float pitch = c.y * VerticalSpreadAngle;

                        Quaternion spreadRot = FirePoint.rotation * Quaternion.Euler(pitch, yaw, 0f);

                        GameObject bullet = Instantiate(EnemyBullet, FirePoint.position, spreadRot);
                        bullet.GetComponent<EnemyBullet>().Damage = Damage;
                    }
                    //muzzleFlash//
                    GameObject muzzleFlash = Instantiate(EnemyMuzzleFlash, FirePoint.position, FirePoint.rotation);
                    muzzleFlash.transform.SetParent(FirePoint);

                }

                float qtaDist = (Target.transform.position - transform.position).sqrMagnitude;
                if (qtaDist > AttackRange * AttackRange)
                    QuitAttack();
                break;

            case EnemyState.Die:
                Agent.enabled = false;
                break;

            case EnemyState.Shock:
                if (EH.Health <= 0)
                {
                    EnemyAnimator.SetTrigger("Die");
                    ES = EnemyState.Die;
                }
               
                PanicFantom.SetActive(true);
                Agent.enabled = false;
                break;
        }
    }

    void EnterAttack()
    {
        if (ES == EnemyState.Attack) return;
        ES = EnemyState.Attack;
        Rigidbody.linearVelocity = Vector3.zero;
    }

    void QuitAttack()
    {
        ES = EnemyState.Moving;
        StrafeTimer = 0f;
        NextStrafeFlip = Random.Range(StrafeChangeIntervalMin, StrafeChangeIntervalMax);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, SeparationRadius);

        Gizmos.color = Color.cyan;
        if (Target != null)
        {
            float range = Mathf.Max(StrafeBeginDistance, AttackRange * 1.25f);
            Gizmos.DrawWireSphere(Target.transform.position, range);
        }
    }

    public void EnemyShock()
    {
        if (Shocked) return;
        EH.ShockedText();
        ES = EnemyState.Shock;
        EnemyAnimator.SetTrigger("Shock");
        EH.PlayRandomDeathSFX();
    }
}
