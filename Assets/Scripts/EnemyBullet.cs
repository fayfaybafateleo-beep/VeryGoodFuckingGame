using UnityEngine;
using UnityEngine.Splines.ExtrusionShapes;

public class EnemyBullet : MonoBehaviour
{
    [Header("Physics")]
    public Rigidbody Rigidbody;
    public float Speed;
    [Range(1, 10)]
    public float Duration = 5f;

    [Header("Data")]
    public int Damage;

    [Header("HitEffect")]
    public GameObject HitEffect;

    [Header("SpecialBullet")]
    public bool IsExplosive;
    public GameObject Explosion;
    public float ExplosionDamage;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Rigidbody.linearVelocity = transform.forward * Speed;
    }

    // Update is called once per frame
    void Update()
    {
        Duration -= Time.deltaTime;
        if (Duration <= 0)
            Destroy(gameObject);
    }
    void OnCollisionEnter(Collision collision)
    {
        var hit = collision.GetContact(0);
        //HitEffecT
        GameObject hitEffect1 = Instantiate(HitEffect, hit.point + hit.normal * 0.001f, Quaternion.LookRotation(hit.normal));
        hitEffect1.transform.SetParent(collision.transform);

        if (IsExplosive)
        {
            GameObject explosion= Instantiate(Explosion,collision.transform.position, Quaternion.identity);
            
        }

        if (collision.gameObject.GetComponent<PlayerHealth>() != null)
        {
            collision.gameObject.GetComponent<PlayerHealth>().TakeDamage(Damage);
        }
        // Destroy the bullet on collision
        Destroy(gameObject);
    }
}
