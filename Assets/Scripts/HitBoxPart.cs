using UnityEngine;
using TMPro;
using System.Collections;

public class HitBoxPart : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [Header("HeadShotDamage")]
    public float damageMultiplier = 2.0f;
    public bool IsCriticalPoint=false;

    [Header("PartHealth")]
    public bool destructible = false;
    public int Thoughness;
    public float partMaxHealth = 20;             
     public float partHealth;   

    [Header("PartDestrcutionEffect")]
    public Transform BloodPoint;
    public GameObject Blood;
    public GameObject BloodSplash;
    public GameObject TextObject;
    public string Text;

    public EnemyHealth Owner;

    [Header("Gore Settings")]
    public GameObject meatChunkPrefab;
    public GameObject ExcutionBloodSPlash;

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
    public void ApplyPartDamage(float baseDamage,int penatrateLevel)
    {
        if (partHealth <= 0 ||  destructible == false) return;
        //PenatrateLevelPass
        float thoughnessMultipulier =
       (penatrateLevel > Thoughness) ? 1.5f :
       (penatrateLevel < Thoughness) ? 0.3f :
        1f;

        float final = baseDamage * damageMultiplier*thoughnessMultipulier;

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
        if (IsCriticalPoint)
        {
            Owner.AddCritical();
        }
        else
        {
            Owner.AddDIssect();
        }

           
        SpawnGore(this.transform.position);

        Destroy(gameObject);
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
            float scaleX = Random.Range(MinScale, MaxScale);
            float scaleY= Random.Range(MinScale, MaxScale);
            float scaleZ = Random.Range(MinScale, MaxScale);
            chunk.transform.localScale = new Vector3(scaleX,scaleY,scaleZ);

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
    public void GoreExcution(Vector3 origin)
    {
        //Goresssssss
        Owner.BodyPartDesctructSound();
        GameObject bloodSplash = Instantiate(ExcutionBloodSPlash, this.transform.position, this.transform.rotation);
        int count = Random.Range(MinChunks, MaxChunks + 1);
       
        for (int i = 0; i < count; i++)
        {
            // Offset
            Vector3 spawnPos = origin + Random.insideUnitSphere * 0.1f;
            if(meatChunkPrefab != null)
            {
                GameObject chunk = Instantiate(meatChunkPrefab, spawnPos, Random.rotation);
                float scaleX = Random.Range(MinScale, MaxScale + 0.2f);
                float scaleY = Random.Range(MinScale , MaxScale + 0.2f);
                float scaleZ = Random.Range(MinScale , MaxScale + 0.2f);
                chunk.transform.localScale = new Vector3(scaleX, scaleY, scaleZ);
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

        Destroy(gameObject, 0f);
    }

}
