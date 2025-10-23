using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    [Header(" EnemyData")]
    public float Health;
    public int Thougthness;

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
        if (Health <= 0)
        {
            Destroy(gameObject, 0f);
            HitMark.GetComponent<Animator>().SetTrigger("Kill");
            HitMarkParent.AddKillShake(10f);
            HitMarkParent.HitMarkHitSoundPlay();
        }
    }
}
