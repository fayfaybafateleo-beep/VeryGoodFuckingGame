using System.Collections.Generic;
using UnityEngine;

public class WeaponSeller : MonoBehaviour
{
    [Header(" WeaponList")]
    public List<GameObject> AllWeaponList;
    [Header(" WeaponManager")]
    public WeaponManager WM;
    [Header(" InputSettings")]
    public KeyCode Key = KeyCode.N;
    public KeyCode Key2 = KeyCode.M;
    public KeyCode Key3 = KeyCode.B;
    public bool InTrigger;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        WM = GameObject.FindGameObjectWithTag("WeaponManager").GetComponent<WeaponManager>();
    }

    // Update is called once per frame
     void  FixedUpdate()
    {
        if (Input.GetKeyDown(Key3) &&InTrigger)
        {
            int indexToReplace = 0; 

            if (WM.OriginWeaponsList != null)
            {
                WM.OriginWeaponsList[0] = AllWeaponList[indexToReplace];  
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
