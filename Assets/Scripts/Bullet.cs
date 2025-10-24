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
    public GameObject HitEffect2;
    [Range(1, 10)]
    public float Duration = 5f; 

    [Header("Impact")]
    [Range(0, 1000)]
    public float Force = 100; 
    [Range(0.1f, 1)]
    public float Radius = 0.25f;

    public GameObject HitMark;
    public CrossHairGenral HitMarkParent;
    public string EnemyTag = "Enemy";

    [Header("GunDatas")]
    public float Damage;
    public int PenatrateLevel;

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
        var enemy = hb ? hb.owner : collision.collider.GetComponentInParent<EnemyHealth>();
        if (hb != null)
        {
            //Instantiate BulletHole For all RB object
            var hit = collision.GetContact(0);
            GameObject bulletHole= Instantiate(BulletHole, hit.point + hit.normal * 0.001f, Quaternion.LookRotation(hit.normal));
            bulletHole.transform.SetParent(collision.transform);

            //HitEffect
            if (hb.Thoughness>PenatrateLevel)
            {
                GameObject hitEffect1 = Instantiate(HitEffect, hit.point + hit.normal * 0.001f, Quaternion.LookRotation(hit.normal));
                hitEffect1.transform.SetParent(collision.transform);
            }
            if (hb.Thoughness <= PenatrateLevel)
            {
                GameObject hitEffect2 = Instantiate(HitEffect2, hit.point + hit.normal * 0.1f, Quaternion.LookRotation(hit.normal));
               // hitEffect2.transform.SetParent(collision.transform);
            }

            //Dule Damage 

            if (enemy)
            {
                enemy.ApplyHit(Damage, PenatrateLevel, hb);
                HitMark.GetComponent<Animator>().SetTrigger("Hit");
                HitMarkParent.AddShake(1f);
                HitMarkParent.HitMarkHitSoundPlay();
            }   
        }
        
        // Destroy the bullet on collision
        Destroy(gameObject);
    }
   
}
