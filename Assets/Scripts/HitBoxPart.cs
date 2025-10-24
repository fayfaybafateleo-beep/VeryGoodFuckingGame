using UnityEngine;

public class HitBoxPart : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [Header ("HeadShotDamage")]
    public float damageMultiplier = 2.0f;

    public int Thoughness;
    public EnemyHealth owner;

    void Awake()
    {
        if(this.transform.parent!=null &&this.transform.parent.GetComponent<EnemyHealth>() != null)
        {
            owner = GetComponentInParent<EnemyHealth>();
            Thoughness = owner.Thougthness;
        }
        else
        {
            //do not thing
        }
           
    }
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
