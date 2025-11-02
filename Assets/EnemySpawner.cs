using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [Header("EnemyType")]
    public GameObject Enemy;
    public int InsNUm;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "Bullet")
        {
            for (int i = 0; i <InsNUm; i++)
            {
                Vector2 offset = Random.insideUnitCircle * 2f;  
                Vector3 spawnPos = transform.position + new Vector3(offset.x, 0f, offset.y);

                Instantiate(Enemy, spawnPos, Quaternion.identity);

            }
        }
    }
}
