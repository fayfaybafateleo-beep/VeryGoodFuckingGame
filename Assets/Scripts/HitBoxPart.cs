using UnityEngine;

public class HitBoxPart : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [Header("HeadShotDamage")]
    public float damageMultiplier = 2.0f;

    [Header("PartHealth")]
    public bool destructible = false;           // 这个部位是否可被破坏
    public float partMaxHealth = 20;             // 部位最大生命
     public float partHealth;   // 当前生命

    [Header("PartDestrcutionEffect")]
    public Transform BloodPoint;
    public GameObject Blood;
    public GameObject BloodSplash;
    public int Thoughness;
    public EnemyHealth Owner;

    [Header("Gore Settings")]
    public GameObject meatChunkPrefab;  
    public int MinChunks = 3;           
    public int MaxChunks = 8;           
    public float MinScale = 0.3f;       
    public float MaxScale = 1.2f;      
    public float Force = 8f;   
    public float UpwardBias = 0.5f;    
    public float TorqueForce = 5f;      
    public float ChunkLife = 5f;   

    void Awake()
    {
        if (this.transform.parent != null && this.transform.parent.parent != null && this.transform.parent.parent.GetComponent<EnemyHealth>() != null)
        {
            Owner = transform.parent.parent.GetComponent<EnemyHealth>();
            Thoughness = Owner.Thougthness;
        }
        else
        {
            //do not thing
        }
        partHealth = partMaxHealth;
    }
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
    public void ApplyPartDamage(float baseDamage)
    {
        if (partHealth <= 0 &&  destructible == false) return;
     
        float final = baseDamage * damageMultiplier;

        partHealth -= final;

        if (partHealth <= 0 && destructible)
        {
            BreakPart();
        }
    }
    public void BreakPart()
    {
        Owner.BodyPartDesctructSound();
        GameObject bloodEffect=  Instantiate(Blood, BloodPoint.position, BloodPoint.rotation);
        bloodEffect.transform.SetParent(Owner.transform);
        bloodEffect.transform.localScale = Owner.transform.localScale;

        GameObject bloodSplash = Instantiate(BloodSplash, BloodPoint.position, BloodPoint.rotation);
        bloodSplash.transform.SetParent(Owner.transform);
        bloodSplash.transform.localScale = Owner.transform.localScale;
        Destroy(gameObject, 0f);
        SpawnGore(this.transform.position);
    }
    public void SpawnGore(Vector3 origin)
    {
        if (meatChunkPrefab == null) return;

        int count = Random.Range(MinChunks, MaxChunks + 1);

        for (int i = 0; i < count; i++)
        {
            // Offset
            Vector3 spawnPos = origin + Random.insideUnitSphere * 0.1f;

            GameObject chunk = Instantiate(meatChunkPrefab, spawnPos, Random.rotation);
            float scale = Random.Range(MinScale, MaxScale);
            chunk.transform.localScale = Vector3.one * scale;

            // RandomForce
            Rigidbody rb = chunk.GetComponent<Rigidbody>();
            if (rb)
            {
                Vector3 randomDir = (Random.onUnitSphere + Vector3.up * UpwardBias).normalized;
                float randomForce = Random.Range(Force * 0.7f, Force * 1.3f);
                rb.AddForce(randomDir * randomForce, ForceMode.Impulse);
                rb.AddTorque(Random.onUnitSphere * TorqueForce, ForceMode.Impulse);
            }

            Destroy(chunk, ChunkLife);
        }
    }
}
