using UnityEngine;

public class Bullet : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [Header("Physics")]
    public Rigidbody Rigidbody;
   
    public float Speed; // m/s

    [Space]
    // When duration = 0, the bullets is destroyed
    [Range(1, 10)]
    public float Duration = 5f; // seconds

    [Header("Impact")]
    [Range(0, 1000)]
    public float Force = 100; // Netwons
    [Range(0.1f, 1)]
    public float Radius = 0.25f; // m

    // Start is called before the first frame update

    //modify//
    public GameObject UI;
    void Start()
    {
        Rigidbody.linearVelocity = transform.forward * Speed;
        UI = GameObject.FindGameObjectWithTag("HitMark");
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
        // Add explosive force to objects (if they have a rigidbody)
        Rigidbody rigidbody = collision.collider.GetComponent<Rigidbody>();
        if (rigidbody != null)
        {
            rigidbody.AddExplosionForce(Force, transform.position, Radius);
            Debug.Log("hit!" + rigidbody.name);
            UI.GetComponent<Animator>().SetTrigger("Hit");
        }

        // Destroy the bullet on collision
        Destroy(gameObject);
    }

}
