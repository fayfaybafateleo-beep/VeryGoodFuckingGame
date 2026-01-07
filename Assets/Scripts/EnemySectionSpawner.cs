using System.Collections.Generic;
using UnityEngine;

public class EnemySectionSpawner : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [Header("Section")]
    public SectionManager.SectionType Section;
    public SectionManager SectionManager;

    [Header("Spawner")]
    public List<GameObject> ObjectList = new List<GameObject>();
    public List<GameObject> EliteList = new List<GameObject>();
    public List<GameObject> MiniBossList = new List<GameObject>();


    public List<Transform> SpawnPositions = new List<Transform>(); 
    public float SpawnInterval = 2f;   
    public int MaxSpawnInThisSection = 10;


    public int MaxEliteInThisSection = 2;
    public int MaxMiniBossInThisSection = 1;

    public int LocalEliteCount = 0;
    public int LocalMiniBossCount = 0;

    private float SpawnTimer = 0f;
    private int LocalSpawnCount = 0;

    public AnimationCurve EliteChance = AnimationCurve.Linear(0, 0f, 10, 0.35f); 
    public AnimationCurve MiniBossChance = AnimationCurve.Linear(0, 0f, 10, 0.08f);

    public int DangerLevel;
    private void Start()
    {
      
            if (SectionManager == null)
            {
                SectionManager = FindFirstObjectByType<SectionManager>();
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

        GameObject prefab = PickPrefabByDanger();
        if (prefab == null) return;

        Vector2 offset = Random.insideUnitCircle * 1f;

        Transform spawnPoint = transform;
        Vector3 offsetPoisition = spawnPoint.position;
        if (SpawnPositions.Count > 0)
        {
            spawnPoint = SpawnPositions[Random.Range(0, SpawnPositions.Count)] ;
            offsetPoisition = spawnPoint.position + new Vector3(offset.x, 0f, offset.y);
        }

        GameObject enemy=   Instantiate(prefab, offsetPoisition, spawnPoint.rotation);

        enemy.GetComponent<EnemyHealth>().Section= Section;
        enemy.GetComponent<EnemyHealth>().SectionManager = SectionManager;

        LocalSpawnCount++;
        SectionManager.RegisterSpawn();  
    }
    public void ResetState()
    {
        LocalSpawnCount = 0;
        SpawnTimer = 0f;
     
    }

    public GameObject PickPrefabByDanger()
    {
        //MiniBoss
        if (MiniBossList.Count > 0 && LocalMiniBossCount < MaxMiniBossInThisSection)
        {
            float miniBossChance = Mathf.Clamp01(MiniBossChance.Evaluate(DangerLevel));
            if (Random.value < miniBossChance)
            {
                LocalMiniBossCount++;
                return MiniBossList[Random.Range(0, MiniBossList.Count)];
            }
        }

        // Elite
        if (EliteList.Count > 0 && LocalEliteCount < MaxEliteInThisSection)
        {
            float eliteChance = Mathf.Clamp01(EliteChance.Evaluate(DangerLevel));
            if (Random.value < eliteChance)
            {
                LocalEliteCount++;
                return EliteList[Random.Range(0, EliteList.Count)];
            }
        }

        // NormalMob
        return ObjectList[Random.Range(0, ObjectList.Count)];
    }
}
