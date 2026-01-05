using UnityEngine;

public class LoopTest : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public float Speed = 5f;         
    public float LoopWidth = 30f;      

    void Update()
    {
        transform.Translate(Vector3.left * Speed * Time.deltaTime, Space.World);

        if (transform.position.x <= -LoopWidth)
        {
            transform.position += Vector3.right * LoopWidth;
        }
    }
}
