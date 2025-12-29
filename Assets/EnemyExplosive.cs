using System.Collections.Generic;
using UnityEngine;

public class EnemyExplosive : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [Header("Damage")]
    public int Damage = 60;
    [Range(0f, 100f)]
    public float Radius = 6f;

    [Header("Force")]
    public float ExplosionForce = 8f;
    public float UpwardsModifier = 0.5f;
    public LayerMask ForceAffectsMask = ~0;
    public QueryTriggerInteraction TriggerMode = QueryTriggerInteraction.Ignore;

    [Header("LOS")]
    public LayerMask ObstructionMask = ~0; 

    [Header("LifeTime")]
    public float DestroyAfter = 2f;

    bool exploded;

    const int MAX_COLS = 64;
    Collider[] _cols = new Collider[MAX_COLS];

    HashSet<PlayerHealth> _damagedPlayers = new HashSet<PlayerHealth>();

    void Start()
    {
        Detonate();
    }

    public void Detonate()
    {
        if (exploded) return;
        exploded = true;

        Vector3 center = transform.position;

        int n = Physics.OverlapSphereNonAlloc(center, Radius, _cols, ~0, TriggerMode);

        _damagedPlayers.Clear();

        for (int i = 0; i < n; i++)
        {
            var col = _cols[i];
            if (!col) continue;

            Vector3 targetPoint = col.ClosestPoint(center);

            if (Physics.Linecast(center, targetPoint, out RaycastHit hit, ObstructionMask, TriggerMode))
            {
                if (hit.collider != col) continue;
            }

            if (((1 << col.gameObject.layer) & ForceAffectsMask) != 0)
            {
                var rb = col.attachedRigidbody;
                if (rb)
                    rb.AddExplosionForce(ExplosionForce, center, Radius, UpwardsModifier, ForceMode.Impulse);
            }

            var ph = col.GetComponentInParent<PlayerHealth>();
            if (ph && _damagedPlayers.Add(ph))
            {
                ph.TakeDamage(Damage);
            }
        }

        Destroy(gameObject, Mathf.Max(0.01f, DestroyAfter));
    }
}
