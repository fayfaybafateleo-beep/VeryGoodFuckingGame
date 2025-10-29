using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class WeaponManager : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public List<GameObject> OriginWeaponsList;

    public List<GameObject> WeaponsOnEquipmentList;
    [Header (" KeyOfSwapWeapon")]
    public KeyCode Key = KeyCode.Q;
    public KeyCode Key2 = KeyCode.E;
    [Header(" WeaponAttacher")]
    public GameObject WeaponParent;

    public bool IsWeaponSwaped=false;
    public int I = 0;
    [Header(" SwapCD")]
    public bool CanSwap = true;
    public Transform FP;
    Vector3 up = Vector3.up;


    void Awake()
    {
        foreach (GameObject GunPrefab in OriginWeaponsList)
        {
            if (GunPrefab != null)
            {
               GameObject NewGun= Instantiate(GunPrefab, this.transform);
                WeaponsOnEquipmentList.Add(NewGun);
                NewGun.transform.SetParent(WeaponParent.transform);
                NewGun.transform.localPosition = Vector3.zero;
                NewGun.transform.localRotation = Quaternion.identity;
            }
           
        }
    }
    void Start()
    {
        WeaponsOnEquipmentList[I + 1].SetActive(false);
        WeaponsOnEquipmentList[I + 1].GetComponent<GunScript>().GS = GunScript.GunState.CeaseFire;
      
    }
   
    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(Key2))
        {
        }

        if (Input.GetKeyDown(Key) &&CanSwap)
        {
            CanSwap = false;
            if (IsWeaponSwaped)
            {
                //StopSecondWeaponOnFire
                WeaponsOnEquipmentList[I+1].GetComponent<GunScript>().GS = GunScript.GunState.CeaseFire;
                //SetGunActive

                var oldSway = WeaponsOnEquipmentList[I+1].GetComponent<WeaponWaging>();
                if (oldSway) oldSway.LowerForReload(0.12f);

                WeaponsOnEquipmentList[I ].GetComponent<GunScript>().IdleAnimation();
                WeaponsOnEquipmentList[I + 1].SetActive(false);
                WeaponsOnEquipmentList[I].SetActive(true);

                var newSwayA = WeaponsOnEquipmentList[I].GetComponent<WeaponWaging>();                 
                if (newSwayA) { newSwayA.SnapToLow(); newSwayA.RaiseFromLow(); }

                IsWeaponSwaped =!IsWeaponSwaped;
                //SwapingCD
                StartCoroutine(SwapGun(I ));
            }
            else
            {
               
                //StopFirstWeaponOnFire
                WeaponsOnEquipmentList[I ].GetComponent<GunScript>().GS = GunScript.GunState.CeaseFire;
                //SetGunActive

                 var oldSway = WeaponsOnEquipmentList[I].GetComponent<WeaponWaging>();
                 if (oldSway) oldSway.LowerForReload(0.12f);


                WeaponsOnEquipmentList[I + 1].GetComponent<GunScript>().IdleAnimation();
                WeaponsOnEquipmentList[I].SetActive(false);
                WeaponsOnEquipmentList[I + 1].SetActive(true);

                var newSwayB = WeaponsOnEquipmentList[I + 1].GetComponent<WeaponWaging>();             
                if (newSwayB) { newSwayB.SnapToLow(); newSwayB.RaiseFromLow(); }
                IsWeaponSwaped = !IsWeaponSwaped;
                //SwapingCD
                StartCoroutine(SwapGun( I+1));
            }
        }
  
    }
    IEnumerator SwapGun(int gunSelected)
    {
        var Swap = WeaponsOnEquipmentList[gunSelected].GetComponent<WeaponWaging>(); 

        float waitTime = Swap.raiseDuration ;

        yield return new WaitForSecondsRealtime(waitTime);

        WeaponsOnEquipmentList[gunSelected].GetComponent<GunScript>().GS = GunScript.GunState.CanFire;

        CanSwap = true;
    }
   

  
     
    


}
