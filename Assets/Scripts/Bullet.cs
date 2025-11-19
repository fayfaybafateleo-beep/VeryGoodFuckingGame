using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;

public class Bullet : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [Header("Physics")]
    public Rigidbody Rigidbody;
    public float Speed;

    [Header("HitEffect")]
    public GameObject BulletHole;
    public GameObject HitEffect;
    public GameObject ExtraHitEffect;
    public GameObject HitEffect2;
    [Range(1, 10)]
    public float Duration = 5f;

    [SerializeField, Range(0f, 1f)]
    private float ExtraEffectChance = 0.3f;

    public GameObject HitMark;
    public CrossHairGenral HitMarkParent;
    public string EnemyTag = "Enemy";

    [Header("GunDatas")]
    public float Damage;
    public int PenatrateLevel;

    [Header("Penetration")]
    private HashSet<Transform> hitEnemyRoots = new HashSet<Transform>();
    public int MaxPenetrateTargets = 1;
    private int PenetratedCount = 0;
    void Start()
    {
        Rigidbody.linearVelocity = transform.forward * Speed;
        HitMark = GameObject.FindGameObjectWithTag("HitMark");
        HitMarkParent = HitMark.GetComponentInParent<CrossHairGenral>();
    }

    void Update()
    {
        // If the bullet has no more time to live
        // it gets destroyed
        Duration -= Time.deltaTime;
        if (Duration <= 0)
            Destroy(gameObject);
    }

    // Bullets die on collision
   void OnCollisionEnter(Collision collision)
    {
        var hb = collision.collider.GetComponent<HitBoxPart>();
        var enemy = hb ? hb.Owner : collision.collider.transform.parent.GetComponentInParent<EnemyHealth>();
        if (hb != null)
        {
            //Instantiate BulletHole For all RB object
            var hit = collision.GetContact(0);
            GameObject bulletHole= Instantiate(BulletHole, hit.point + hit.normal * 0.001f, Quaternion.LookRotation(hit.normal));
            bulletHole.transform.SetParent(hb.transform);

            //HitEffect
            if (hb.Thoughness>PenatrateLevel)
            {
                GameObject hitEffect1 = Instantiate(HitEffect, hit.point + hit.normal * 0.001f, Quaternion.LookRotation(hit.normal));
                hitEffect1.transform.SetParent(collision.transform);
            }
            if (hb.Thoughness <= PenatrateLevel)
            {
                GameObject hitEffect2 = Instantiate(HitEffect2, hit.point + hit.normal * 0.1f, Quaternion.LookRotation(hit.normal));
                if (enemy && Random.value <= ExtraEffectChance)
                {
                   
                        GameObject extraHitEffect = Instantiate(ExtraHitEffect, hit.point + hit.normal * 0.1f, Quaternion.LookRotation(hit.normal));
                        extraHitEffect.transform.SetParent(collision.transform);
                    
                }

            }

            //Dule Damage 

            if (enemy)
            {
                enemy.ApplyHit(Damage, PenatrateLevel, hb,collision.GetContact(0).point);
                hb.ApplyPartDamage(Damage, PenatrateLevel);
                HitMark.GetComponent<Animator>().SetTrigger("Hit");
                HitMarkParent.AddShake(1f);
                HitMarkParent.HitMarkHitSoundPlay();

                //pENERTRAEcOUNT
                Transform enemyRoot = enemy.transform.root;

                bool isNewEnemy = hitEnemyRoots.Add(enemyRoot);

                if (isNewEnemy)
                {
                   PenetratedCount++;
                }

                bool tooHardToPenetrate = hb.Thoughness > PenatrateLevel;
                bool exceededPenetrateCount = PenetratedCount >= MaxPenetrateTargets;

                if (tooHardToPenetrate || exceededPenetrateCount)
                {
                    Destroy(gameObject);
                    return;
                }
                else
                {
                    Collider bulletCol = GetComponent<Collider>();
                    if (bulletCol != null)
                    {
                        // 忽略这只敌人身上所有的 collider
                        foreach (var col in enemyRoot.GetComponentsInChildren<Collider>())
                        {
                            Physics.IgnoreCollision(bulletCol, col);
                        }
                    }

                    if (Rigidbody != null)
                    {
                        Rigidbody.linearVelocity = transform.forward * Speed;
                    }
                    return;
                }
            }   
        }
        
        // Destroy the bullet on collision
        Destroy(gameObject);
    }
   
}
