using UnityEngine;

public class Grenades : MonoBehaviour
{
    [Header("WayToDetonate")]
    public bool IsPercussion;
    public bool StartFuze = false;
    public float FuzeTime = 3f;
    public float FuzeTimer = 0f;
  
    [Header("ExplosionPrefab")]
    public GameObject Explosive;

    [Header("LuanchSettings")]
    [Range(0, 1000)]
    public float Velosity;
    [Range(0, 90)]
    public float launchAngle = 45f;
    public Rigidbody Rigidbody;
    public int BurstCount;
    public float BurstInterval;
    public float GLAnimationSpeed=1;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        FuzeTimer = FuzeTime;

        Vector3 direction = Quaternion.Euler(-launchAngle, 0f, 0f) * transform.forward;

        Rigidbody.linearVelocity = direction * Velosity;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (StartFuze && IsPercussion==false)
        {
            FuzeTimer -= Time.deltaTime;
        }
        if (FuzeTimer <= 0)
        {
            Explosion();
        }
    }
    public void OnCollisionEnter(Collision collision)
    {
        if (IsPercussion)
        {
            Explosion();
        }
        else
        {
            StartFuze = true;
        }
    }
    public void Explosion()
    {
        Instantiate(Explosive, transform.position, transform.rotation);
        Destroy(gameObject, 0);
    }

}
