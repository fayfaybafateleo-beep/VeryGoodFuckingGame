using System.Collections.Generic;
using UnityEngine;

public class WeaponSeller : MonoBehaviour
{
    [Header(" WeaponList")]
    public List<GameObject> AllWeaponList;
    public int CurrentInedx;
    [Header(" WeaponManager")]
    public WeaponManager WM;
    [Header(" InputSettings")]
    public KeyCode Key5 = KeyCode.N;
    public KeyCode Key6 = KeyCode.M;
    public KeyCode Key7 = KeyCode.B;
    public bool InTrigger;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        WM = GameObject.FindGameObjectWithTag("WeaponManager").GetComponent<WeaponManager>();
    }

    // Update is called once per frame
     void  Update()
    {
        //ChoosingGun
        if (Input.GetKeyDown(Key5) && InTrigger) CurrentInedx++;

        if (Input.GetKeyDown(Key6) && InTrigger) CurrentInedx --;

        if (CurrentInedx == AllWeaponList.Count) CurrentInedx = 0;

        if(CurrentInedx < 0) CurrentInedx = AllWeaponList.Count-1;

        if (Input.GetKeyDown(Key7) && InTrigger)
        {
            GameObject purchased = AllWeaponList[CurrentInedx];

            if (purchased != null)
            {
                int activeIndex = WM.IsWeaponSwaped ? (WM.I + 1) : WM.I;

                if (activeIndex >= 0 && activeIndex < WM.OriginWeaponsList.Count)
                {
                    WM.OriginWeaponsList[activeIndex] = purchased;

                    WM.SpawnGuns(activeIndex);
                    Debug.Log("Paid");
                }
            }
        }
    }
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            InTrigger = true;
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            InTrigger = false;
        }
    }
    
        
   
}
