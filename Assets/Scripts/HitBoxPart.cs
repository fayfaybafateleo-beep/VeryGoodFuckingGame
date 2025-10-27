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



    void Awake()
    {
        if (this.transform.parent != null && this.transform.parent.GetComponent<EnemyHealth>() != null)
        {
            Owner = GetComponentInParent<EnemyHealth>();
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

        // 按部位倍率结算（最简：不依赖外部贯穿/韧性逻辑）
        float final = baseDamage * damageMultiplier;

        // 扣除整型生命
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
        
    }
}
