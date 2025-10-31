using System.Collections.Generic;
using Unity.Cinemachine;
using UnityEngine;

public class MeleeHitScript : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [Header("FistSize")]
    public Transform StartPoint;
    public Transform EndPoint; 
    public float Radius = 0.15f;
    public LayerMask HitMask = ~0;

    [Header("Damage")]
    public float Damage = 10f;
    public int PenatrateLevel = 5;
    public float KnockBackForce;

    [Header("Effects (可选)")]
    public GameObject BulletHole;     
    public GameObject HitEffect;    
    public GameObject HitEffect2;    
    public GameObject ExtraHitEffect; 
    [Range(0f, 1f)] public float ExtraEffectChance = 0.2f;

    [Header("HitMark")]
    public GameObject HitMark;
    public CrossHairGenral HitMarkParent;

    [Header("ScreenShake")]
    public CinemachineImpulseSource MeleeImpulseSource;

    [Header("Sound")]
    public AudioSource MeleeSound;
    public AudioClip HitClip;

    [Header("SweepOrDebug")]
    public int maxSweepSteps = 3;    // 插值步数，避免高速漏检
    public bool debugGizmos = false;

    private bool WindowOpen;
    private bool HitSomethingThisWindow;
    private Vector3 lastStart, lastEnd;

    private readonly HashSet<Collider> hitColliders = new();
    private readonly HashSet<EnemyHealth> hitOwners = new();

    private void Awake()
    {
        if (HitMark == null)
        {
            HitMark = GameObject.FindGameObjectWithTag("HitMark");
        }
        if (HitMark != null && HitMarkParent == null)
        {
            HitMarkParent = HitMark.GetComponentInParent<CrossHairGenral>();
        }
           
    }
    public void BeginWindow()
    {
        WindowOpen = true;
        HitSomethingThisWindow = false;
        hitColliders.Clear();
        hitOwners.Clear();
        if (StartPoint) lastStart = StartPoint.position;
        if (EndPoint) lastEnd = EndPoint.position;
    }

    /// <summary>动画事件：关窗，并返回本窗口是否命中过</summary>
    public bool EndWindow()
    {
        WindowOpen = false;
        bool result = HitSomethingThisWindow;
        hitColliders.Clear();
        hitOwners.Clear();
        return result;
    }

    public void Update()
    {
        if (!WindowOpen || StartPoint == null || EndPoint == null) return;

        Vector3 curStart = StartPoint.position;
        Vector3 curEnd = EndPoint.position;

        for (int i = 1; i <= maxSweepSteps; i++)
        {
            float t = (float)i / maxSweepSteps;
            Vector3 s = Vector3.Lerp(lastStart, curStart, t);
            Vector3 e = Vector3.Lerp(lastEnd, curEnd, t);

            var overlaps = Physics.OverlapCapsule(s, e, Radius, HitMask, QueryTriggerInteraction.Collide);
            foreach (var col in overlaps)
            {
                // OneTIme结算
                HitBoxPart hb = null;
                col.TryGetComponent(out hb);

                EnemyHealth enemy = null;
                if (hb != null) enemy = hb.Owner;
                if (enemy == null) enemy = col.transform.GetComponentInParent<EnemyHealth>();

                if (enemy != null && hitOwners.Contains(enemy)) continue;
                if (hitColliders.Contains(col)) continue;

                
                if (hb != null)
                {
                    hitColliders.Add(col);
                    if (enemy != null) hitOwners.Add(enemy);

                    HitSomethingThisWindow = true;
                    //The hit point
                    Vector3 mid = (s + e) * 0.5f;
                    Vector3 normal = (e - s).sqrMagnitude > 1e-6f ? (e - s).normalized : -transform.forward;

                    
                    if (BulletHole != null)
                    {
                        var bh = Instantiate(BulletHole, mid + normal * 0.001f, Quaternion.LookRotation(normal));
                        bh.transform.SetParent(hb.transform);
                    }

                    // 命中特效（与你的判定一致）
                    if (hb.Thoughness > PenatrateLevel)
                    {
                        if (HitEffect != null)
                        {
                            var fx = Instantiate(HitEffect, mid + normal * 0.001f, Quaternion.LookRotation(normal));
                            fx.transform.SetParent(col.transform);
                        }
                    }
                    else
                    {
                        if (HitEffect2 != null)
                        {
                            var fx2 = Instantiate(HitEffect2, mid + normal * 0.1f, Quaternion.LookRotation(normal));
                            fx2.transform.SetParent(col.transform);
                            MeleeSound.PlayOneShot(HitClip);
                        }

                        if (enemy != null && ExtraHitEffect != null && Random.value <= ExtraEffectChance)
                        {
                            var extra = Instantiate(ExtraHitEffect, mid + normal * 0.1f, Quaternion.LookRotation(normal));
                            extra.transform.SetParent(col.transform);
                        }
                    }
                    if (enemy != null)
                    {
                        enemy.ApplyHit(Damage, PenatrateLevel, hb, mid);
                        Rigidbody enemyRb = enemy.GetComponent<Rigidbody>();
                        hb.ApplyPartDamage(Damage, PenatrateLevel);
                        HitScreenShake(true);
                        // HitEffect
                        if (HitMark != null)
                        {
                            var anim = HitMark.GetComponent<Animator>();
                            if (anim) anim.SetTrigger("Hit");
                        }
                        if (HitMarkParent != null)
                        {
                            HitMarkParent.AddShake(1f);
                            HitMarkParent.HitMarkHitSoundPlay();
                        }
                        if (!enemyRb.isKinematic)
                        {
                            Vector3 rawDir = (enemy.transform.position - transform.position);
                            Vector3 flatDir = Vector3.ProjectOnPlane(rawDir, Vector3.up).normalized;

                            if (flatDir.sqrMagnitude < 1e-4f)
                            {
                                flatDir = Vector3.ProjectOnPlane(transform.forward, Vector3.up).normalized; 
                            }
                            enemyRb.AddForce(flatDir * KnockBackForce, ForceMode.VelocityChange); 
                        }
                    }
                    else
                    {
                        HitScreenShake(false);
                    }
                }
            }
        }

        lastStart = curStart;
        lastEnd = curEnd;
    }

    void OnDrawGizmosSelected()
    {
        if (!debugGizmos || StartPoint == null || EndPoint == null) return;
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(StartPoint.position, Radius);
        Gizmos.DrawWireSphere(EndPoint.position, Radius);
        Gizmos.DrawLine(StartPoint.position, EndPoint.position);
    }
    public void HitScreenShake(bool isHitEnemy)
    {
        if (isHitEnemy)
        {
            MeleeImpulseSource.ImpulseDefinition.AmplitudeGain = 1f;
            MeleeImpulseSource.ImpulseDefinition.FrequencyGain = 1f;
        }
        else
        {
            MeleeImpulseSource.ImpulseDefinition.AmplitudeGain = 0.3f;
            MeleeImpulseSource.ImpulseDefinition.FrequencyGain = 0.3f;
        }
        MeleeImpulseSource.GenerateImpulse();
    }
}

