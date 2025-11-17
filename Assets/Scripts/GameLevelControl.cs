using UnityEngine;

public class GameLevelControl : MonoBehaviour
{
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
        
    }

    // Update is called once per frame
    void Update()
    {
        if (IsSeleted == false)
        {
            return;
        }

        switch (LS) 
        {
            case LevelState.NotActive:
                IsActive = false;

                if (StartTrigger.IsTriggered)
                {
                    LS = LevelState.Active;
                }
                break;
            case LevelState.Active:
                IsActive = true;

                if (EndTrigger.IsTriggered)
                {
                    LS = LevelState.End;
                }
                break;
            case LevelState.End:

                break;
        }

    }

}
