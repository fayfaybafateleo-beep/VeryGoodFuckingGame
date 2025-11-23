using System.Collections.Generic;
using UnityEngine;

public class EnemySectionSpawner : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [Header("Section")]
    public SectionManager.SectionType Section;
    public SectionManager SectionManager;

    [Header("À¢π÷…Ë÷√")]
    public List<GameObject> ObjectList = new List<GameObject>();  
    public List<Transform> SpawnPositions = new List<Transform>(); 
    public float SpawnInterval = 2f;   
    public int MaxSpawnInThisSection = 10; 

    private float SpawnTimer = 0f;
    private int LocalSpawnCount = 0;


    private void Start()
    {
        if (SectionManager == null)
        {
            SectionManager = SectionManager.Instance;

            if (SectionManager == null)
            {
                SectionManager = FindFirstObjectByType<SectionManager>();
            }
        }
    }

    private void Update()
    {
        MaxSpawnInThisSection = SectionManager.TargetSpawnCount;
        if (SectionManager == null) return;

        // ActiveWhileSectionManagerIsActive
        if (!SectionManager.IsActive(Section)) return;
        if (SectionManager.IsDone(Section)) return;

        //CountCheck
        if (SectionManager.CurrentSpawnCount >= SectionManager.TargetSpawnCount)
            return;

        //LocalCountCheck
        if (LocalSpawnCount >= MaxSpawnInThisSection)
            return;

        // CD
        SpawnTimer += Time.deltaTime;
        if (SpawnTimer < SpawnInterval) return;
        SpawnTimer = 0f;

        Spawn();
    }

    private void Spawn()
    {
        if (ObjectList.Count == 0) return;

     
        GameObject prefab = ObjectList[Random.Range(0, ObjectList.Count)];

        Transform spawnPoint = transform;
        if (SpawnPositions.Count > 0)
        {
            spawnPoint = SpawnPositions[Random.Range(0, SpawnPositions.Count)];
        }

        GameObject enemy=   Instantiate(prefab, spawnPoint.position, spawnPoint.rotation);
        enemy.GetComponent<EnemyHealth>().Section= Section;

        LocalSpawnCount++;
        SectionManager.RegisterSpawn();  
    }
    public void ResetState()
    {
        LocalSpawnCount = 0;
        SpawnTimer = 0f;
     
    }
}
