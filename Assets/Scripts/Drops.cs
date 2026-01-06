using System.Runtime.CompilerServices;
using System.Threading;
using UnityEngine;

public class Drops : MonoBehaviour
{
    [Header("ObjectType")]
    [Tooltip("Ammo1,Ammo2,AmmoG,Health,Coin,Armour")]
    public string Type;

    [Header("Reference")]
    public WeaponManager WMScript;
    public GameObject WM;
    public GunScript GS1;
    public GunScript GS2;
    public GameObject Player;
    public PlayerData PD;
    public PlayerHealth PH;

    [Header("DropsData")]
    public float GainMultipulier;
    public int CoinCount;
    public bool IsUsed=false;
    public float Timer;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        WM = GameObject.FindGameObjectWithTag("WeaponManager");
        WMScript = WM.GetComponent<WeaponManager>();

        Player= GameObject.FindGameObjectWithTag("Player");
        PD=Player.GetComponent<PlayerData>();
        PH = Player.GetComponent<PlayerHealth>();

        GS1 = WM.transform.GetChild(0).GetComponent<GunScript>();
        GS2 = WM.transform.GetChild(1).GetComponent<GunScript>();
    }

    // Update is called once per frame
    void Update()
    {
        if (!IsUsed)
        {
            Timer += Time.unscaledDeltaTime;

            if (Timer >= 3f)
            {

              /*  if (Type == "Ammo1")
                {
                    RecoverAmmo1();
                }
                if (Type == "Ammo2")
                {
                    RecoverAmmo2();
                }
                if (Type == "AmmoG")
                {
                    RecoverGrenade();
                }
              
                if(Type == "Health")
                {
                    GetHealth();
                }
                if (Type == "Armour")
                {
                    GetArmour();
                }*/

                if (Type == "Coin")
                {
                    GetCoin();
                    Destroy(transform.parent.gameObject);
                    IsUsed = true;
                }
            }
            if (Timer >= 120f)
            {
                Destroy(transform.parent.gameObject);
                IsUsed = true;
            }
        }
    }
    public void OnTriggerEnter(Collider other)
    {
        if (IsUsed) return;
        if (other.gameObject.tag == "Player")
        {

            //DifferentTypes,DifferentEffect
            if (Type == "Ammo1")
            {
                RecoverAmmo1();
            }
            if (Type == "Ammo2")
            {
                RecoverAmmo2();
            }
            if (Type == "AmmoG")
            {
                RecoverGrenade();
            }
            if(Type == "Coin")
            {
                GetCoin();
            }
            if (Type == "Health")
            {
                GetHealth();
            }
            if (Type == "Armour")
            {
                GetArmour();
            }
            var parent = transform.parent;
            Destroy(parent.gameObject, 0f);
        }
    }
    public void RecoverAmmo1()
    {
        GS1.GetBackUpAmmo(GainMultipulier);
    }
    public void RecoverAmmo2()
    {
        GS2.GetBackUpAmmo(GainMultipulier);
    }
    public void RecoverGrenade()
    {
        WMScript.GetBackUpGrenade();
    }
    public void GetCoin()
    {
        PD.Coins += CoinCount;;
    }
    public void GetArmour()
    {
        PH.GainArmour(GainMultipulier);
    }
    public void GetHealth()
    {
        PH.GainHealth(GainMultipulier);
    }
}
