using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class ObjectSpawner : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [Header("EnemyType")]
    public GameObject Enemy;
    public SectionManager SM;
    public int InsNUm;

    public List<GameObject> ObjectList;

    public bool CanInstan = false;

    public float SpawnCD;
    public float SpawnCurrentCD;

    public bool IsSectionSpawner;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (IsSectionSpawner == false)
        {
            return;
        }

        if (SpawnCurrentCD>=SpawnCD)
        {
            SpawnCurrentCD = SpawnCD;
        }
        SpawnCurrentCD += Time.deltaTime;
        
    }
    public void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "Bullet")
        {
            for (int i = 0; i <InsNUm; i++)
            {

                Vector2 offset = Random.insideUnitCircle * 1f;  
                Vector3 spawnPos = transform.position + new Vector3(offset.x, 0f, offset.y);

                int index = Random.Range(0, ObjectList.Count);
                GameObject prefabToSpawn = ObjectList[index];

                Instantiate(prefabToSpawn, spawnPos, Quaternion.identity);

            }
        }
    }
}
