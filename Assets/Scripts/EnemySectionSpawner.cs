using System.Collections.Generic;
using UnityEngine;

public class EnemySectionSpawner : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [Header("Section")]
    public SectionManager.SectionType Section;
    public SectionManager SectionManager;

    [Header("刷怪设置")]
    public List<GameObject> ObjectList = new List<GameObject>();  // 要刷的 prefab
    public List<Transform> SpawnPositions = new List<Transform>(); // 刷怪点（可多个随机）
    public float SpawnInterval = 2f;   // 两次刷怪间隔时间
    public int MaxSpawnInThisSection = 10; // 这个 Section 最多刷多少只（局部上限，可选）

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

        // 随机选择一个 prefab
        GameObject prefab = ObjectList[Random.Range(0, ObjectList.Count)];

        // 随机选择一个出生点，如果没有就用自己的 Transform
        Transform spawnPoint = transform;
        if (SpawnPositions.Count > 0)
        {
            spawnPoint = SpawnPositions[Random.Range(0, SpawnPositions.Count)];
        }

        GameObject enemy=   Instantiate(prefab, spawnPoint.position, spawnPoint.rotation);
        enemy.GetComponent<EnemyHealth>().Section= Section;

        LocalSpawnCount++;
        SectionManager.RegisterSpawn();  // 通知 SectionManager：又刷了一个
    }

}
