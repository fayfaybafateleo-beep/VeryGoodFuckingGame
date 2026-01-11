using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class WeaponSeller : MonoBehaviour
{
    [Header(" WeaponList")]
    public List<GameObject> AllWeaponList;
    public int CurrentInedx;

    [Header(" WeaponManager")]
    public WeaponManager WM;

    [Header(" WeaponInfo")]
    public TextMeshPro NameText;
    public TextMeshPro NameText2;
    public TextMeshPro DataText;
    public TextMeshPro MoneyText;

    [Header(" InputSettings")]
    public KeyCode Key5 = KeyCode.N;
    public KeyCode Key6 = KeyCode.M;
    public KeyCode Key7 = KeyCode.B;
    public bool InTrigger;

    [Header(" Player")]
    public GameObject Player;
    public PlayerData PD;
    public int CurrentPlayerMoney;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        WM = GameObject.FindGameObjectWithTag("WeaponManager").GetComponent<WeaponManager>();
        Player = GameObject.FindGameObjectWithTag("Player");
        PD = Player.GetComponent<PlayerData>();

       
    }

    // Update is called once per frame
     void  Update()
     {
        //CashSync
        CurrentPlayerMoney = PD.Coins;

        //Info SHowing
        NameText.text = AllWeaponList[CurrentInedx].GetComponent<GunScript>().Name;
        NameText2.text = AllWeaponList[CurrentInedx].GetComponent<GunScript>().Name;
        DataText.text = "Dmage:" + AllWeaponList[CurrentInedx].GetComponent<GunScript>().GunDamage +
            "  RPM:" + AllWeaponList[CurrentInedx].GetComponent<GunScript>().FireRate + "\n"+
            "Magazine:" + AllWeaponList[CurrentInedx].GetComponent<GunScript>().MagazineCount +
            "  AP:" + AllWeaponList[CurrentInedx].GetComponent<GunScript>().GunPeneration;
        MoneyText.text = AllWeaponList[CurrentInedx].GetComponent<GunScript>().Cost.ToString() + " $";

        //ChoosingGun
        if (Input.GetKeyDown(Key5) && InTrigger) CurrentInedx--;

        if (Input.GetKeyDown(Key6) && InTrigger) CurrentInedx ++;

        if (CurrentInedx == AllWeaponList.Count) CurrentInedx = 0;

        if(CurrentInedx < 0) CurrentInedx = AllWeaponList.Count-1;

        if (Input.GetKeyDown(Key7) && InTrigger)
        {
            if(CurrentPlayerMoney>= AllWeaponList[CurrentInedx].GetComponent<GunScript>().Cost)
            {
                GameObject purchased = AllWeaponList[CurrentInedx];

                if (purchased != null)
                {
                    int activeIndex = WM.IsWeaponSwaped ? (WM.I + 1) : WM.I;

                    if (activeIndex >= 0 && activeIndex < WM.OriginWeaponsList.Count)
                    {
                        WM.OriginWeaponsList[activeIndex] = purchased;

                        WM.SpawnGuns(activeIndex);

                        PD.Coins -= AllWeaponList[CurrentInedx].GetComponent<GunScript>().Cost;
                        Debug.Log("Paid");
                    }
                }
            }
           
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
