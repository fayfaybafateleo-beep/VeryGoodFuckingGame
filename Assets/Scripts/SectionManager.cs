using NUnit.Framework;
using System;
using System.Collections.Generic;
using UnityEngine;

public class SectionManager : MonoBehaviour
{
    public enum SectionType
    {
        Section1,
        Section2,
        Section3
    }

    [Header("Sections Overall Inspect")]
    public bool Section1Active;
    public bool Section2Active;
    public bool Section3Active;

    public bool Section1Done;
    public bool Section2Done;
    public bool Section3Done;

    [Header("SpawnCount")]
    public int TargetSpawnCount;      
    public int CurrentSpawnCount;     

    [Header("SectionSpawnCount")]
    public int Section1MaxSpawn = 20; 
    public int Section2MaxSpawn = 30; 
    public int Section3MaxSpawn = 40; 

    [Header("SpawnerList")]
    public List<GameObject> SpawnerList;

    public Dictionary<SectionType, bool> Active = new Dictionary<SectionType, bool>();
    public Dictionary<SectionType, bool> Done = new Dictionary<SectionType, bool>();

    public static SectionManager Instance { get; private set; }

    private void Awake()
    {
        Instance = this;

        foreach (SectionType st in Enum.GetValues(typeof(SectionType)))
        {
            Active[st] = false;
            Done[st] = false;
        }
        TargetSpawnCount = 0;
        CurrentSpawnCount = 0;

        SyncToInspectorBools();
    }

    public void SetActive(SectionType section, bool value)
    {
        Active[section] = value;
        switch (section)
        {
            case SectionType.Section1:
                Section1Active = value;
                break;
            case SectionType.Section2:
                Section2Active = value;
                break;
            case SectionType.Section3:
                Section3Active = value;
                break;
        }

        if (value)
        {
            CurrentSpawnCount = 0;                          
            TargetSpawnCount = GetMaxSpawnForSection(section); 
        }
    }

    public void SetDone(SectionType section, bool value)
    {
        Done[section] = value;

        switch (section)
        {
            case SectionType.Section1:
                Section1Done = value;
                break;
            case SectionType.Section2:
                Section2Done = value;
                break;
            case SectionType.Section3:
                Section3Done = value;
                break;
        }
    }

    public bool IsActive(SectionType section)
    {
        return Active.TryGetValue(section, out bool v) && v;
    }

    public bool IsDone(SectionType section)
    {
        return Done.TryGetValue(section, out bool v) && v;
    }

//Spawn Restirct
    public void RegisterSpawn()
    {
        if (TargetSpawnCount <= 0)
        {
            CurrentSpawnCount++;
            return;
        }

        CurrentSpawnCount++;

        if (CurrentSpawnCount >= TargetSpawnCount)
        {
            foreach (SectionType st in Enum.GetValues(typeof(SectionType)))
            {
                SetActive(st, false);
            }
        }
    }

    private void SyncToInspectorBools()
    {
        Section1Active = Active[SectionType.Section1];
        Section2Active = Active[SectionType.Section2];
        Section3Active = Active[SectionType.Section3];

        Section1Done = Done[SectionType.Section1];
        Section2Done = Done[SectionType.Section2];
        Section3Done = Done[SectionType.Section3];
    }


    private int GetMaxSpawnForSection(SectionType section)
    {
        switch (section)
        {
            case SectionType.Section1:
                return Section1MaxSpawn;
            case SectionType.Section2:
                return Section2MaxSpawn;
            case SectionType.Section3:
                return Section3MaxSpawn;
            default:
                return 0;
        }
    }

     void Update()
    {
       
    }
}



