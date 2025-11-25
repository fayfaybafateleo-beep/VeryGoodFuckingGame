using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class MissionSelect : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public List<GameObject> Levels;
    public int CurrentInedx;

    [Header(" WeaponManager")]
    public WeaponManager WM;

    [Header(" InputSettings")]
    public KeyCode Key5 = KeyCode.V;
    public KeyCode Key6 = KeyCode.C;
    public KeyCode Key7 = KeyCode.B;

    public bool InTrigger;
    void Start()
    {
        WM = GameObject.FindGameObjectWithTag("WeaponManager").GetComponent<WeaponManager>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(Key5) && InTrigger) CurrentInedx++;

        if (Input.GetKeyDown(Key6) && InTrigger) CurrentInedx--;

        if (CurrentInedx == Levels.Count) CurrentInedx = 0;

        if (CurrentInedx < 0) CurrentInedx = Levels.Count - 1;

        if (Input.GetKeyDown(Key7) && InTrigger)
        {
            GameObject currentLV = Levels[CurrentInedx];
            currentLV.GetComponent<GameLevelControl>().ResetLevel();
            currentLV.GetComponent<GameLevelControl>().IsSeleted = true;
            currentLV.GetComponent<GameLevelControl>().LS=GameLevelControl.LevelState.NotActive;
        }
    }
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            InTrigger = true;
            WM.FCS = WeaponManager.FireControlState.CantInput;
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            InTrigger = false;
            WM.FCS = WeaponManager.FireControlState.AllowInput;
            WM.EnableWeaponsWhileFinishInteract();
        }
    }
}
