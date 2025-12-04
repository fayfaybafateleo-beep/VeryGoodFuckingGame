using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class GameLevelControl : MonoBehaviour
{
    [Header("LevelData")]
    public string LevelName;
    public int HazardLevel;
    public float HideDistance = 2f;

    [Header("UiGuidence")]
    public GameObject Guidence;
    public Camera PlayerCamera;

    [Header("TriggerZone")]
    public GameObject StartingPoint;
    public GameObject EndingPoint;
    public LevelTriggers StartTrigger;
    public LevelTriggers EndTrigger;

    [Header("Bools")]
    public bool IsSeleted;
    public bool IsActive;

    [Header("SeprateZones")]
    public GameObject Zone1;
    public GameObject Zone2;
    public GameObject Zone3;

    [Header("Manager")]
    public SectionManager SM;

    public List<SectionTrigger> Triggers;
    public List<LevelTriggers> LevelTriggers;
    public List<EnemySectionSpawner> Spawners;
    public List<SectorDoor> Gates;
    public GameObject ExitTrigger;

    [Header("CarPark")]
    public GameObject CarPark;
    public enum LevelState
    { 
        NotActive,
        Active,
        End
    }
    public LevelState LS;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        PlayerCamera = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
    }

    // Update is called once per frame
    void Update()
    {
        if (IsSeleted)
        {
            CarPark.SetActive(true);
        }

        float distance = Vector3.Distance(PlayerCamera.transform.position, Guidence.transform.position);
        if (IsSeleted && distance > HideDistance && LS == LevelState.NotActive)
        {
            Guidence.SetActive(true);
        }
        if (IsSeleted == false)
        {
            Guidence.SetActive(false);
            CarPark.SetActive(false);
        }
       
        if (IsSeleted == false)
        {
            return;
        }


        switch (LS) 
        {
            case LevelState.NotActive:
                IsActive = false;
                ExitTrigger.SetActive(false);


               
                

                if (StartTrigger.IsTriggered)
                {
                    LS = LevelState.Active;
                }
                break;
            case LevelState.Active:
                IsActive = true;
                ExitTrigger.SetActive(false);

                if (EndTrigger.IsTriggered)
                {
                    LS = LevelState.End;
                }

                Guidence.SetActive(false);

                break;
            case LevelState.End:
                ExitTrigger.SetActive(true);
                IsSeleted = false;

                Guidence.SetActive(false);
                break;
        }

    }
    public void ResetLevel()
    {
        foreach(var st in Triggers)
        {
            st.ResetTrigger();
        }
        foreach (var st2 in LevelTriggers)
        {
            st2.ResetTrigger();
        }
        foreach (var st3 in Spawners)
        {
            st3.ResetState();
        }
        SM.ResetState();
        LS = LevelState.NotActive;
        foreach (var st4 in Gates)
        {
            st4.ResetGate();
        }
    }

}

