using UnityEngine;

public class SpeedLineDetection : MonoBehaviour
{
    public Rigidbody RB;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        float speed = RB.linearVelocity.magnitude;
        Debug.Log("ËÙ¶È£º" + speed);
    }
}
