using UnityEngine;

public class EnemyArtillery : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [Header("Target")]
    public Transform player;

    [Header("Random Area (Ring)")]
    public float MinRadius = 2f;
    public float MaxRadius = 8f;

    [Header("Ground Snap")]
    public LayerMask groundMask = ~0;
    public float RayStartHeight = 30f;
    public float RaycastDistance = 100f;
    public float GroundOffset = 0.02f;

    [Header("Try Settings")]
    public int MaxTries = 20;

    [Header("Align To Ground (optional)")]
    public bool AlignToGroundNormal = false;

    void Start()
    {
      player = GameObject.FindGameObjectWithTag("Player").transform;
        LocatePlayer();
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
}
