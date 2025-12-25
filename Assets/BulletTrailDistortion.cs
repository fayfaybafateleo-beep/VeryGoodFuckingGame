using UnityEngine;

public class BulletTrailDistortion : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [Header("Noise")]
    public float Amplitude = 0.02f;
    public float Frequency = 30f;

    [Range(0f, 1f)]
    public float Smoothing = 0.15f;

    Vector3 originLocalPos;
    Vector3 currentOffset;

    void Awake()
    {
        originLocalPos = transform.localPosition;
    }

    void LateUpdate()
    {
        if (transform.parent == null)
        {
            transform.localPosition = originLocalPos;
            return;
        }

        float t = Time.time * Frequency;

        Vector3 targetOffset = new Vector3( Mathf.PerlinNoise(t, 0f) - 0.5f, Mathf.PerlinNoise(0f, t) - 0.5f,Mathf.PerlinNoise(t, t) - 0.5f ) * Amplitude;

        currentOffset = Vector3.Lerp(currentOffset, targetOffset, 1f - Smoothing);
        transform.localPosition = originLocalPos + currentOffset;
    }

    void OnDisable()
    {
        transform.localPosition = originLocalPos;
    }
}

