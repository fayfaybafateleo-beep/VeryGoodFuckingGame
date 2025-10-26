using UnityEngine;
using static UnityEditor.Recorder.OutputPath;
using static UnityEngine.Rendering.DebugUI;

public class EnemyHealth : MonoBehaviour
{
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

    [Header("DeadEffect")]
    public bool IsDead;
    public Vector3 Dir;
    public Vector3 LastHitPoint;

    public float KinematicActiveAfterDie;
    public float ForcePerDamage;
    public float AngularPerDamage;
    public float UpwardForce;
    public float FinalDamage;
    public Animator EnemyAnimator;
    public float TimeToDestroy;

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
        Dir = (transform.position - hitPoint).normalized;    
    }
    void Die()
    {
        if (IsDead) return;                                   // One time excute
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
        float impulseMag = FinalDamage * ForcePerDamage;
        Vector3 force = Dir * impulseMag;
        RB.AddForce(Vector3.up * UpwardForce*FinalDamage, ForceMode.Impulse);
        RB.AddTorque(Random.onUnitSphere * AngularPerDamage * FinalDamage, ForceMode.Impulse);
        RB.AddForceAtPosition(force, LastHitPoint, ForceMode.Impulse);
        //BodyStationary
        Invoke("EnemyKinematic", KinematicActiveAfterDie);
    }
    public void EnemyKinematic()
    {
        RB.isKinematic = true;
        DestroyBody();
    }
    public void DestroyBody()
    {
        Destroy(gameObject, TimeToDestroy);
    }
    public void BodyPartDesctructSound()
    {
       AudioSource.PlayOneShot(Destruction);
    }
}
