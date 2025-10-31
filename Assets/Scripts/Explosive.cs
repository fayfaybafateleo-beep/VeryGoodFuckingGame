using UnityEngine;
using System.Collections;
using System.Collections.Generic;
public class Explosive : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created


    [Header("Damage")]
    public float Damage = 100f;
    [Range(0, 100f)]
    public float Radius = 6f;               // 伤害半径
    public int PenetrateLevel = 0;        // 传给 ApplyHit 的穿透等级（如无可置0）

    [Header("Force")]
    public float ExplosionForce = 8f;       // 冲击力(Impulse)
    public float UpwardsModifier = 0.5f;    // 向上偏移
    public LayerMask ForceAffectsMask = ~0; // 受力层（一般默认）

    [Header("Effects")]
    public GameObject ScorchDecal;          // 烧痕贴花（可选）
    public GameObject HitEffect;
    [Range(0, 1f)]
    public float ExtraEffectChance = 0.3f;

    public string EnemyTag = "Enemy";       
    public CrossHairGenral HitMarkParent;   
    public GameObject HitMark;              

    [Header("Line of Sight")]
    public LayerMask ObstructionMask = ~0;  // ExplosionBlock
    public QueryTriggerInteraction TriggerMode = QueryTriggerInteraction.Ignore;

    [Header("ExplosionSOund")]
    public AudioSource ExplosionSoundSource;
    public AudioClip ExplosionSound;

    bool exploded;


    void Start()
    {
        if (HitMark == null)
        {
            HitMark = GameObject.FindGameObjectWithTag("HitMark");
        }
        if (HitMarkParent == null && HitMark)
        {
            HitMarkParent = HitMark.GetComponentInParent<CrossHairGenral>();
        }
        Detonate();
    }

    void OnCollisionEnter(Collision c)
    {
        
    }

    public void Detonate()
    {
        if (exploded) return;
        ExplosionSoundSource.PlayOneShot(ExplosionSound);

        exploded = true;

        Vector3 center = transform.position;

        // PushForce
        var colsForce = Physics.OverlapSphere(center, Radius, ForceAffectsMask, TriggerMode);
        foreach (var col in colsForce)
        {
            if (col.attachedRigidbody)
            {
                col.attachedRigidbody.AddExplosionForce(ExplosionForce, center, Radius, UpwardsModifier, ForceMode.Impulse);

            }
               
        }
        var cols = Physics.OverlapSphere(center, Radius, ~0, TriggerMode);

        HashSet<EnemyHealth> damagedEnemies = new HashSet<EnemyHealth>();

        foreach (var col in Physics.OverlapSphere(center, Radius, ~0, TriggerMode))
        {
            var hb = col.GetComponent<HitBoxPart>();
            var enemy = hb ? hb.Owner : col.GetComponentInParent<EnemyHealth>();
            if (!enemy) continue;


            Vector3 targetPoint = col.ClosestPoint(center);
            if (Physics.Linecast(center, targetPoint, out RaycastHit block, ObstructionMask, TriggerMode))
                if (block.collider != col) continue;

            float dmg = Damage;

            if (hb)
            {
                // if Destructible
                if (hb.destructible && hb.partHealth > 0)
                {
                    hb.ApplyPartDamage(dmg, PenetrateLevel);
                    enemy.ApplyHit(dmg, PenetrateLevel, hb, targetPoint);
                }
                else
                {
                    continue;
                }
            }
        
            if (!damagedEnemies.Contains(enemy))
            {
                damagedEnemies.Add(enemy);
                if (HitMark) HitMark.GetComponent<Animator>()?.SetTrigger("Hit");
                HitMarkParent?.AddShake(1.5f);
                HitMarkParent?.HitMarkHitSoundPlay();


                enemy.ApplyHit(dmg, PenetrateLevel, null, targetPoint);
            }

            Vector3 normal = (targetPoint - center).normalized;
            Instantiate(HitEffect, targetPoint + normal * 0.05f, Quaternion.LookRotation(normal, Vector3.up))
                .transform.SetParent(col.transform);
        }

        // ScorchDecalInstantiate
        if (ScorchDecal && Physics.Raycast(center + Vector3.up * 0.2f, Vector3.down, out var rh, 2.0f, ~0, TriggerMode))
        {
            Instantiate(ScorchDecal, rh.point + rh.normal * 0.02f, Quaternion.LookRotation(rh.normal));
        }
    }


}
