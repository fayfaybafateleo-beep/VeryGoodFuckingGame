using System.Collections;
using UnityEngine;

public class EnemyArtillery : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [Header("Player")]
    public Transform player;

    [Header("Damage")]
    public int Damage;

    [Header("MOA")]
    public float MinRadius = 2f;
    public float MaxRadius = 8f;

    [Header("Ground Snap")]
    public LayerMask groundMask = ~0;
    public float RayStartHeight = 30f;
    public float RaycastDistance = 100f;
    public float GroundOffset = 0.02f;

    [Header("Try Settings")]
    public int MaxTries = 20;

    [Header("Align To Ground")]
    public bool AlignToGroundNormal = false;

    [Header("Explosion")]
    public GameObject ExplosionPrefab;
    public bool IsExploded;

    [Header("Size")]
    public Transform OuterRing;
    public Transform InnerRing;

    [Header("FuzeTime")]
    public float FuzeTime = 0.5f;

    void Start()
    {
      player = GameObject.FindGameObjectWithTag("Player").transform;
        LocatePlayer();

        InnerRing.localScale = Vector3.zero;

        StartCoroutine(GrowRoutine());
    }

    public void LocatePlayer()
    {
        Vector3 playerPos = player.position;

        // Random point in ring around player
        Vector2 dir2 = Random.insideUnitCircle.normalized;
        float r = Random.Range(MinRadius, MaxRadius);
        Vector3 candidateXZ = playerPos + new Vector3(dir2.x, 0f, dir2.y) * r;
        Vector3 rayOrigin = candidateXZ + Vector3.up * RayStartHeight;

        if (Physics.Raycast(rayOrigin, Vector3.down, out RaycastHit hit, RaycastDistance, groundMask, QueryTriggerInteraction.Ignore))
        {
            // Optional slope check
            float slope = Vector3.Angle(hit.normal, Vector3.up);

            // Set to Player
            transform.position = hit.point + hit.normal * GroundOffset;

            // Optional align rotation to ground normal (keeps forward projected)
            if (AlignToGroundNormal)
            {
                Vector3 forward = Vector3.ProjectOnPlane(transform.forward, hit.normal);
                if (forward.sqrMagnitude < 0.0001f) forward = Vector3.ProjectOnPlane(player.forward, hit.normal);
                transform.rotation = Quaternion.LookRotation(forward.normalized, hit.normal);
            }
        }
    }

    public void Explosion()
    {
        if (IsExploded == false)
        {
            GameObject e = Instantiate(ExplosionPrefab, transform.position, Quaternion.identity);
            e.GetComponent<EnemyExplosive>().Damage = Damage;
            e.GetComponent<EnemyExplosive>().Radius = OuterRing.transform.localScale.x;
            IsExploded =true;
        }
        Destroy(gameObject);
    }

    IEnumerator GrowRoutine()
    {
        Vector3 targetScale = OuterRing.localScale;
        float t = 0f;

        while (t < 1f)
        {
            float dt =Time.deltaTime;
            t += dt / Mathf.Max(0.0001f, FuzeTime);

            float k = Mathf.Clamp01(t);
            // SmoothStep
            k = k * k * (3f - 2f * k);

            InnerRing.localScale = Vector3.LerpUnclamped(Vector3.zero, targetScale, k);
            yield return null;
        }
        
        InnerRing.localScale = targetScale;

        Explosion();
    }
}
