using System.Collections.Generic;
using UnityEngine;


public class WeaponManager : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public List<GameObject> OriginWeaponsList;

    public List<GameObject> WeaponsOnEquipmentList;
    [Header (" KeyOfSwapWeapon")]
    public KeyCode Key = KeyCode.Q;
    [Header(" WeaponAttacher")]
    public GameObject WeaponParent;

    public bool IsWeaponSwaped=false;
    public int I = 0;
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
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(Key))
        {
            if (IsWeaponSwaped)
            {
                WeaponsOnEquipmentList[I + 1].SetActive(false);
                WeaponsOnEquipmentList[I].SetActive(true);
                IsWeaponSwaped=!IsWeaponSwaped;
            }
            else
            {
                WeaponsOnEquipmentList[I + 1].SetActive(true);
                WeaponsOnEquipmentList[I].SetActive(false);
                IsWeaponSwaped = !IsWeaponSwaped;
            }
        }
    }
}
