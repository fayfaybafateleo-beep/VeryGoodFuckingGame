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
    [Header("WholeLevelControl")]
    public GameLevelControl GLC;

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
    [Header("Kill Targets ")]
    public int Section1KillTarget = 20;  
    public int Section2KillTarget = 30;   
    public int Section3KillTarget = 40;   
    public Dictionary<SectionType, int> KillCounts = new Dictionary<SectionType, int>();
    public int ShowKillCounts;

    [Header("UI Display")]
    public SectionType CurrentSection;
    public int CurrentSectionKillTarget;
    public int CurrentSectionKillCount;
    public bool CurrentSectionDone;


  
    private void Awake()
    {

        foreach (SectionType st in Enum.GetValues(typeof(SectionType)))
        {
            Active[st] = false;
            Done[st] = false;
            KillCounts[st] = 0;
        }
        TargetSpawnCount = 0;
        CurrentSpawnCount = 0;

        SyncToInspectorBools();
    }

    public void Start()
    {
        
    }

    public void SetActive(SectionType section, bool value)
    {
        if (GLC.IsActive == false) return;
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
            KillCounts[section] = 0;
            ShowKillCounts = 0;

            CurrentSectionKillTarget = GetKillTargetForSection(section);
            CurrentSectionKillCount = 0;
            CurrentSectionDone = false;
        }
    }

    public void SetDone(SectionType section, bool value)
    {
        if (GLC.IsActive == false) return;
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
        if (GLC.IsActive == false) return;
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

    public void RegisterKill(SectionType section)
    {
        if (GLC.IsActive == false)
        {
            return;
        }
            if (!KillCounts.ContainsKey(section))
        {
            KillCounts[section] = 0;
        }
           

        KillCounts[section]++;
        ShowKillCounts ++;
        int target = GetKillTargetForSection(section);


        if (target <= 0)
        {
            return;
        }
           

        // Meet Requirements
        if (KillCounts[section] >= target && !IsDone(section))
        {
            SetDone(section, true);
            SetActive(section, false);  
            Debug.Log("Done");
        }
    }

    private int GetKillTargetForSection(SectionType section)
    {
        switch (section)
        {
            case SectionType.Section1: return Section1KillTarget;
            case SectionType.Section2: return Section2KillTarget;
            case SectionType.Section3: return Section3KillTarget;
            default: return 0;
        }
    }

    public void ResetState()
    {
        foreach (SectionType st in Enum.GetValues(typeof(SectionType)))
        {
            Active[st] = false;
            Done[st] = false;
            KillCounts[st] = 0;
        }

        CurrentSpawnCount = 0;
        TargetSpawnCount = 0;

        SyncToInspectorBools();
    }


    void Update()
    {
       
    }
}



