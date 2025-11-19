using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class SectionManager : MonoBehaviour
{
    [Header("Sections")]
    public bool Section1Done;
    public bool Section2Done;
    public bool Section3Done;

    public bool Section1Active;
    public bool Section2Active;
    public bool Section3Active;

    public int TargetSpawnCount;
    public int CurrentSpawnCount;

    public List<GameObject> SpawnerList;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (CurrentSpawnCount >= TargetSpawnCount)
        {
             Section1Active=false;
             Section2Active = false;
             Section3Active = false;
        }
    }
}
