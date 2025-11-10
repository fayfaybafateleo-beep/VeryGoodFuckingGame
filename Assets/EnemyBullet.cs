using UnityEngine;

public class EnemyBullet : MonoBehaviour
{
    [Header("Physics")]
    public Rigidbody Rigidbody;
    public float Speed;
    [Range(1, 10)]
    public float Duration = 5f;

    [Header("Data")]
    public float Damage;

    [Header("HitEffect")]
    public GameObject HitEffect;
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
        // Destroy the bullet on collision
        Destroy(gameObject);
    }
}
