using System.Collections.Generic;
using TMPro;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;
using static UnityEngine.UI.Image;

public class EnemyHealth : MonoBehaviour
{
    [Header("EnemyBehaviourContorl")]
    public EnemyBehaviour EBehaviour;
    [Header(" EnemyData")]
    public float Health;
    public int Thougthness;

    [Header("DamgeMultiplier")]
    float PenertrateRate = 1f;
    float OverPenertrateRate = 1.5f;
    float NotPenertrateRate = 0.3f;

    [Header("HitmarkUsing")]
    public GameObject HitMark;
    public CrossHairGenral HitMarkParent;

    [Header("RidgiBody")]
    public Rigidbody RB;

    [Header("HitText")]
    public GameObject HitText;

    [Header("DeadEffect")]
    public bool IsDead;
    public Vector3 Dir;
    public Vector3 LastHitPoint;

    public Animator EnemyAnimator;
    public LayerMask BodiesLayer;
    public GameObject BloodSPlash;
    public GameObject TextObject;
    public string Text="Brutality!!!";
    public float TimeToDestroy;
    public float KinematicActiveAfterDie;
    public float ForcePerDamage;
    public float AngularPerDamage;
    public float UpwardForce;
    public float FinalDamage;

    public List<GameObject> EnemyLightList;
    public Material LightsOffMaterial;

    [Header("BodyParts")]
    public GameObject Head;
    public GameObject Lower;
    public GameObject Upper;

    [Header("Audio")]
    public AudioSource AudioSource;
    public AudioClip Destruction;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        HitMark = GameObject.FindGameObjectWithTag("HitMark");
        HitMarkParent = HitMark.GetComponentInParent<CrossHairGenral>();
  
    }

    // Update is called once per frame
    void Update()
    {
        if (Health <= 0 && IsDead==false)
        {
            Die();
        }

       
    }
    public void ApplyHit(float baseDamage, int penetrateLevel, HitBoxPart hitbox,Vector3 hitPoint)
    {
        float dmg = baseDamage;
        int toughness = Thougthness;

        if (hitbox)
        {
            dmg *= hitbox.damageMultiplier;
        }

        // Damage settlemtn through thougthness
        if (toughness > penetrateLevel) dmg *= NotPenertrateRate;   
        else if (toughness < penetrateLevel) dmg *= OverPenertrateRate;   

        Health -= dmg;
        // Record the damage and bullet pos
        FinalDamage = dmg;
        LastHitPoint = hitPoint;
        Vector3 spawnPos;
        Dir = (transform.position - hitPoint).normalized;

        Vector3 randomOffset = new Vector3(Random.Range(-0.2f, 0.2f),  Random.Range(0.01f, 0.2f),  Random.Range(-0.3f, 0.3f) );
        if (hitbox)
        {
            spawnPos = hitPoint + randomOffset;
        }
        else
        {
            spawnPos = transform.position + randomOffset;
        }

        if (hitbox.GetComponent<HitBoxPart>().IsCriticalPoint)
        {
            GameObject text = Instantiate(HitText, spawnPos, Quaternion.identity);
            text.GetComponentInChildren<TextMeshPro>().text = dmg.ToString();
            text.GetComponentInChildren<TextMeshPro>().color = Color.red;
            text.transform.localScale = new Vector3(1.2f, 1.2f, 1.2f);
            Destroy(text, 1f);
        }
        else
        {
            GameObject text = Instantiate(HitText, spawnPos, Quaternion.identity);
            text.GetComponentInChildren<TextMeshPro>().text = dmg.ToString();
            Destroy(text, 1f);
        }
        
    }
    void Die()
    {
        if (IsDead) return;                                   // One time excute
        //Kill Confrim
        EBehaviour.ES = EnemyBehaviour.EnemyState.Die;
        IsDead = true;
        Debug.Log("ED");
        
        
        //Hitmark using
        HitMark.GetComponent<Animator>().SetTrigger("Kill");
        HitMarkParent.AddKillShake(10f);
        HitMarkParent.HitMarkKillSoundPlay();

        //BodyEffect
        RB.constraints = RigidbodyConstraints.None;
        RB.isKinematic = false;
        EnemyAnimator.SetTrigger("Die");
        Instantiate(BloodSPlash, LastHitPoint,Quaternion.identity);

        float impulseMag = FinalDamage * ForcePerDamage;
        Vector3 force = Dir * impulseMag;
        RB.AddForce(Vector3.up * UpwardForce*FinalDamage, ForceMode.Impulse);
        RB.AddTorque(Random.onUnitSphere * AngularPerDamage * FinalDamage, ForceMode.Impulse);
        RB.AddForceAtPosition(force, LastHitPoint, ForceMode.Impulse);
        //BodyStationary
        Invoke("EnemyKinematic", KinematicActiveAfterDie);
        //lightsOff
        foreach (GameObject lights in EnemyLightList)
        {
            if (lights == null) continue;
            lights.GetComponent<Renderer>().material = LightsOffMaterial;
        }
    }
    public void EnemyKinematic()
    {
        //SwapToOtherLayer
        SwapLayer();
        DestroyBody();
    }
    public void DestroyBody()
    {
        RB.linearDamping = 20;
        Destroy(gameObject, 3f);
    }
    public void BodyPartDesctructSound()
    {
       AudioSource.PlayOneShot(Destruction);
    }
    public void SwapLayer()
    {
        gameObject.layer = LayerMask.NameToLayer("AutoDestroy");
        foreach(Transform child in GetComponentsInChildren<Transform>(true))
        {
            child.gameObject.layer = LayerMask.NameToLayer("AutoDestroy");
        }
        RB.AddForce(Vector3.down * 9.7f, ForceMode.Force);
    }

    public void GoreExcution()
    {
        Health = -10;

        GameObject text = Instantiate(TextObject, transform.position, Quaternion.identity);
        text.GetComponentInChildren<TextMeshPro>().text =Text;
        Destroy(text, 1f);
        foreach (HitBoxPart part in GetComponentsInChildren<HitBoxPart>())
        {
            part.GoreExcution(part.transform.position);
        }
    }
}
