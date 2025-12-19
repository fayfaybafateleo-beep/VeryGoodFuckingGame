using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyBehaviour : MonoBehaviour
{
    public string Name;

    [Header("Target")]
    public GameObject Target;
    public NavMeshAgent Agent;
    public bool IsFlesh;

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
    public int Damage;
    public int SlugCount = 1;
    public Transform FirePoint;
    public GameObject EnemyBullet;
    public GameObject EnemyMuzzleFlash;
    public bool Lock = false;

    [Header("FlySetting")]
    public bool IsFlying;
    public float FlyHeight;

    [Header("Burst Fire")]
    public int BurstCount = 3;
    public float BurstRate = 0.2f;
    public bool IsBurstFiring = false;
    private int BurstShotsFired = 0;

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

    [Header("AttackRhythm")]
    public bool UseAttackRhythm = true;
    [UnityEngine.Range(0.1f, 5f)]
    public float ShootWindowMin = 0.9f;
    [UnityEngine.Range(0.1f, 5f)]
    public float ShootWindowMax = 1.7f;
    [UnityEngine.Range(0.1f, 5f)]
    public float RepositionWindowMin = 0.6f;
    [UnityEngine.Range(0.1f, 5f)]
    public float RepositionWindowMax = 1.2f;

    [Header("LineOfSight")]
    public bool UseLineOfSight = true;
    [UnityEngine.Range(0.05f, 1f)]
    public float LOSGrace = 0.2f;
    public float AimHeight = 1.15f;

    [Header("Flank")]
    public bool UseFlankWhenNoLOS = true;
    [UnityEngine.Range(0.5f, 12f)]
    public float FlankRadius = 5f;
    [UnityEngine.Range(0.05f, 2f)]
    public float FlankRefreshTime = 0.6f;
    [UnityEngine.Range(0.5f, 20f)]
    public float FlankSampleRadius = 2.5f;

    bool HasLOS = true;
    float LOSTimer = 0f;
    bool IsRepositioning = false;
    float PhaseTimer = 0f;
    float PhaseDuration = 1f;
    Vector3 FlankPoint;
    float FlankTimer = 0f;

    // Individual Random
    [Header("IndividualRandom")]
    public bool UseIndividualFactor = true; 
    [UnityEngine.Range(0.5f, 1.5f)]
    public float IndividualFactor;     
    [UnityEngine.Range(0.5f, 1.5f)]
    public float IndividualFactorMin = 0.85f; 
    [UnityEngine.Range(0.5f, 1.5f)]
    public float IndividualFactorMax = 1.15f; 

    [Header("IndividualApply")]
    public bool ApplyToAttackWindow = true;  
    public bool ApplyToFlankRadius = true;   
    public bool ApplyToFlankRefresh = false; 
    public bool ApplyToMOA = false;          

    void Start()
    {
        Target = GameObject.FindGameObjectWithTag("Player");

        Agent.updatePosition = false;
        Agent.updateRotation = false;
        Agent.avoidancePriority = Random.Range(20, 80);

        EH = GetComponent<EnemyHealth>();

        if (IsFlying)
        {
            Agent.baseOffset = FlyHeight;
        }

        // IndividualFactor 
        if (UseIndividualFactor)
        {
            IndividualFactor = Random.Range(IndividualFactorMin, IndividualFactorMax);
        }
        else
        {
            IndividualFactor = 1f;
        }
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

                //AntiStuck
                if (Agent.enabled && Agent.isOnNavMesh)
                {
                    Agent.SetDestination(Target.transform.position);
                    Agent.nextPosition = Rigidbody.position;
                }

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

                if (UseLineOfSight)
                {
                    bool nowLOS = CheckLineOfSight();
                    if (nowLOS)
                    {
                        HasLOS = true;
                        LOSTimer = 0f;
                    }
                    else
                    {
                        LOSTimer += Time.deltaTime;
                        if (LOSTimer >= LOSGrace)
                            HasLOS = false;
                    }
                }
                else
                {
                    HasLOS = true;
                    LOSTimer = 0f;
                }

                if (UseAttackRhythm)
                {
                    PhaseTimer += Time.deltaTime;

                    // NoSign,Repos
                    if (!HasLOS && UseFlankWhenNoLOS)
                    {
                        if (!IsRepositioning)
                        {
                            StartRepositionPhase();
                        }
                    }

                    // WindowSwitch
                    if (PhaseTimer >= PhaseDuration && !IsBurstFiring)
                    {
                        if (IsRepositioning)
                        {
                            StartShootPhase();
                        }
                        else
                        {
                            StartRepositionPhase();
                        }
                    }
                }
                else
                {
                    IsRepositioning = false;
                }

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

                if (Target != null && FirePoint != null)
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

                //Reposition
                if (IsRepositioning && UseFlankWhenNoLOS && Target != null)
                {
                    FlankTimer += Time.deltaTime;

                    //FlankRefresh 
                    float flankRefresh = FlankRefreshTime;
                    if (UseIndividualFactor && ApplyToFlankRefresh)
                    {
                        flankRefresh = FlankRefreshTime * IndividualFactor;
                    }

                    if (FlankTimer >= flankRefresh)
                    {
                        FlankTimer = 0f;
                        PickFlankPoint();
                    }

                    if (Agent.enabled && Agent.isOnNavMesh)
                    {
                        //Flanking
                        Agent.SetDestination(FlankPoint);
                        Agent.nextPosition = Rigidbody.position;
                    }

                    Vector3 moveDir = Agent.desiredVelocity.normalized;

                    Vector3 toPlayer2 = Target.transform.position - transform.position;
                    toPlayer2.y = 0f;
                    float sqrDist2 = toPlayer2.sqrMagnitude;

                    Vector3 strafe2 = Vector3.zero;
                    if (sqrDist2 <= Mathf.Max(StrafeBeginDistance * StrafeBeginDistance, AttackRange * AttackRange * 1.5625f))
                    {
                        Vector3 forward2 = toPlayer2.sqrMagnitude > EPS ? toPlayer2.normalized : Vector3.zero;
                        Vector3 right2 = Vector3.Cross(Vector3.up, forward2).normalized;
                        strafe2 = right2 * StrafeSign * (StrafeStrength * 1.15f);
                    }

                    Vector3 separation2 = Vector3.zero;
                    if (SeparationRadius > 0.01f)
                    {
                        Collider[] hits2 = Physics.OverlapSphere(transform.position, SeparationRadius, EnemyLayer, QueryTriggerInteraction.Ignore);
                        foreach (var hit in hits2)
                        {
                            if (hit.attachedRigidbody == null || hit.attachedRigidbody == Rigidbody) continue;
                            Vector3 away2 = transform.position - hit.transform.position;
                            away2.y = 0f;
                            float dist2 = away2.magnitude + EPS;
                            separation2 += away2.normalized * (SeparationStrength / dist2);
                        }
                    }

                    Vector3 blended2 = moveDir + strafe2 + separation2;
                    if (blended2.sqrMagnitude > 1f) blended2.Normalize();

                    Rigidbody.AddForce(blended2 * MoveForce, ForceMode.Acceleration);

                    if (Rigidbody.linearVelocity.magnitude > MaxSpeed)
                    {
                        Rigidbody.linearVelocity = Rigidbody.linearVelocity.normalized * MaxSpeed;
                    }
                }

                //FireCountDown
                AttackRateTimer += Time.deltaTime;

                if (!IsBurstFiring && AttackRateTimer >= AttackRate && Target != null && Lock)
                {
                    if (!UseAttackRhythm || !IsRepositioning)
                    {
                        if (!UseLineOfSight || HasLOS)
                        {
                            AttackRateTimer = 0;
                            StartCoroutine(BurstRoutine());
                        }
                    }
                }

                float qtaDist = (Target.transform.position - transform.position).sqrMagnitude;
                if (!IsBurstFiring && qtaDist > AttackRange * AttackRange)
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

        //Flanking
        HasLOS = true;
        LOSTimer = 0f;
        FlankTimer = 999f;
        if (UseAttackRhythm)
        {
            StartShootPhase();
        }
        else
        {
            IsRepositioning = false;
            PhaseTimer = 0f;
            PhaseDuration = 999f;
        }
    }

    void QuitAttack()
    {
        ES = EnemyState.Moving;
        StrafeTimer = 0f;
        NextStrafeFlip = Random.Range(StrafeChangeIntervalMin, StrafeChangeIntervalMax);

        IsRepositioning = false;
        PhaseTimer = 0f;
        PhaseDuration = 1f;
        LOSTimer = 0f;
        HasLOS = true;
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

    private IEnumerator BurstRoutine()
    {
        IsBurstFiring = true;
        BurstShotsFired = 0;
        AttackRateTimer = 0f;

        Rigidbody.linearVelocity = Vector3.zero;

        while (BurstShotsFired < BurstCount)
        {
            EnemyAnimator.SetTrigger("Fire");

            for (int i = 0; i < SlugCount; i++)
            {
                Vector2 c = Random.insideUnitCircle;

                // MOA
                float h = HorizontalSpreadAngle;
                float v = VerticalSpreadAngle;
                if (UseIndividualFactor && ApplyToMOA)
                {
                    h = HorizontalSpreadAngle * IndividualFactor;
                    v = VerticalSpreadAngle * IndividualFactor;
                }

                float yaw = c.x * h;
                float pitch = c.y * v;

                if (FirePoint != null)
                {
                    Quaternion spreadRot = FirePoint.rotation * Quaternion.Euler(pitch, yaw, 0f);
                    GameObject bullet = Instantiate(EnemyBullet, FirePoint.position, spreadRot);
                    bullet.GetComponent<EnemyBullet>().Damage = Damage;
                }
            }

            if (EnemyMuzzleFlash != null && FirePoint != null)
            {
                GameObject muzzleFlash = Instantiate(EnemyMuzzleFlash, FirePoint.position, FirePoint.rotation);
                muzzleFlash.transform.SetParent(FirePoint);
            }

            BurstShotsFired++;

            yield return new WaitForSeconds(BurstRate);
        }

        IsBurstFiring = false;
    }

    bool CheckLineOfSight()
    {
        if (Target == null) return false;

        Vector3 targetPos = Target.transform.position + Vector3.up * AimHeight;

        Vector3 origin;
        if (FirePoint != null)
            origin = FirePoint.position;
        else
            origin = transform.position + Vector3.up * 1.2f;

        Vector3 dir = targetPos - origin;
        float dist = dir.magnitude;
        if (dist <= 0.001f) return true;

        if (Physics.Raycast(origin, dir.normalized, out RaycastHit hit, dist, LineOfSightMask, QueryTriggerInteraction.Ignore))
        {
            if (hit.collider != null)
            {
                // CheckIfPlayer
                if (hit.collider.gameObject == Target || hit.collider.transform.root.gameObject == Target)
                    return true;
            }
            return false;
        }

        return true;
    }

    void StartRepositionPhase()
    {
        IsRepositioning = true;
        PhaseTimer = 0f;

        //ApplyAttackWindow
        float min = RepositionWindowMin;
        float max = RepositionWindowMax;
        if (UseIndividualFactor && ApplyToAttackWindow)
        {
            min = RepositionWindowMin * IndividualFactor;
            max = RepositionWindowMax * IndividualFactor;
        }
        PhaseDuration = Random.Range(min, max);

        FlankTimer = 999f; // Refreshing
        PickFlankPoint();
    }

    void StartShootPhase()
    {
        IsRepositioning = false;
        PhaseTimer = 0f;

        //ApplyAttackWindow 
        float min = ShootWindowMin;
        float max = ShootWindowMax;
        if (UseIndividualFactor && ApplyToAttackWindow)
        {
            min = ShootWindowMin * IndividualFactor;
            max = ShootWindowMax * IndividualFactor;
        }
        PhaseDuration = Random.Range(min, max);
    }

    void PickFlankPoint()
    {
        if (Target == null)
        {
            FlankPoint = transform.position;
            return;
        }

        Vector3 playerPos = Target.transform.position;
        Vector3 toPlayer = playerPos - transform.position;
        toPlayer.y = 0f;

        Vector3 right = toPlayer.sqrMagnitude > EPS ? Vector3.Cross(Vector3.up, toPlayer.normalized).normalized : transform.right;

        //  ApplyToFlankRadius
        float radius = FlankRadius;
        if (UseIndividualFactor && ApplyToFlankRadius)
        {
            radius = FlankRadius * IndividualFactor;
        }

        Vector3 c1 = playerPos + right * radius;
        Vector3 c2 = playerPos - right * radius;

        Vector3 p1 = SampleToNavMesh(c1);
        Vector3 p2 = SampleToNavMesh(c2);

        float s1 = ScoreFlankPoint(p1);
        float s2 = ScoreFlankPoint(p2);

        FlankPoint = (s1 >= s2) ? p1 : p2;
    }

    Vector3 SampleToNavMesh(Vector3 pos)
    {
        if (NavMesh.SamplePosition(pos, out NavMeshHit hit, FlankSampleRadius, NavMesh.AllAreas))
            return hit.position;
        return pos;
    }

    float ScoreFlankPoint(Vector3 p)
    {
        float score = 0f;

        float d = (p - transform.position).sqrMagnitude;
        score -= d;

        // SimpleLosTest
        Vector3 origin = p + Vector3.up * 1.2f;
        Vector3 targetPos = Target.transform.position + Vector3.up * AimHeight;
        Vector3 dir = targetPos - origin;
        float dist = dir.magnitude;

        bool los = true;
        if (dist > 0.001f)
        {
            if (Physics.Raycast(origin, dir.normalized, out RaycastHit hit, dist, LineOfSightMask, QueryTriggerInteraction.Ignore))
            {
                if (!(hit.collider.gameObject == Target || hit.collider.transform.root.gameObject == Target))
                    los = false;
            }
        }

        if (los) score += 100000f;

        return score;
    }
}
