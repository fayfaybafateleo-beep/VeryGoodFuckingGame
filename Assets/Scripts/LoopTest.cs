using UnityEngine;

public class LoopTest : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public float Speed = 5f;         
    public float LoopWidth = 30f;


    [Header("Vehicle")]
    public Transform Vehicle;

    [Header("Drift")]
    public float VerticalDrift = 0.2f; 
    public float DriftSpeed = 0.5f;

    [Header("Shake")]
    public float ShakeAmount = 0.05f;
    public float ShakeSpeed = 8f;

    private Vector3 vehicleStartLocalPos;
    private float driftSeed;
    private float shakeSeed;

    void Start()
    {
        driftSeed = Random.value * 10f;
        shakeSeed = Random.value * 10f;
        vehicleStartLocalPos = Vehicle.localPosition;
    }
    void Update()
    {
        //Ground
        transform.Translate(Vector3.left * Speed * Time.deltaTime, Space.World);

        if (transform.position.x <= -LoopWidth)
        {
            transform.position += Vector3.right * LoopWidth;
        }

        //Vehicle
         float drift = Mathf.PerlinNoise(Time.time * DriftSpeed, driftSeed);
        drift = (drift - 0.5f) * 2f;

        float shake = Mathf.PerlinNoise(Time.time * ShakeSpeed, shakeSeed);
        shake = (shake - 0.5f) * 2f;

        Vector3 offset = new Vector3(
            0f,
            drift * VerticalDrift + shake * ShakeAmount,
            0f
        );

        Vehicle.localPosition = vehicleStartLocalPos + offset;
    }
}
