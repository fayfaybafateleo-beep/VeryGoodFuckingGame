using UnityEngine;
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

    public GameObject HitMark;
    public CrossHairGenral HitMarkParent;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        HitMark = GameObject.FindGameObjectWithTag("HitMark");
        HitMarkParent = HitMark.GetComponentInParent<CrossHairGenral>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Health <= 0) Die();
    }
    public void ApplyHit(float baseDamage, int penetrateLevel, HitBoxPart hitbox)
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
    }
    void Die()
    {
        Destroy(gameObject, 0f);
        HitMark.GetComponent<Animator>().SetTrigger("Kill");
        HitMarkParent.AddKillShake(10f);
        HitMarkParent.HitMarkKillSoundPlay();
        Destroy(gameObject);
    }
}
