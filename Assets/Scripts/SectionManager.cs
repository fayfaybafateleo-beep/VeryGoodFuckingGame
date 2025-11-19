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

        SyncToInspectorBools();
    }

    public void SetActive(SectionType section, bool value)
    {
        Active[section] = value;

     //SyncData
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

    public void RegisterSpawn()
    {
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

    private void Update()
    {
     
    }

}

