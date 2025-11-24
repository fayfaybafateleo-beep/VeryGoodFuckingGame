using UnityEngine;
using UnityEngine.UI;
using TMPro;


public class MagazineCounterUI : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [Header("WeaponManager")]
    public GameObject WeaponManager;
    public WeaponManager WMScript;
    public GameObject Player;
    public PlayerGetInCar PIC;
    public int CurrentAP;

    [Header("UI")]
    public TextMeshProUGUI Text;
    public TextMeshProUGUI GLAmmoText;
    public TextMeshProUGUI GunNumber;
    public Image ReloadBar;
    public bool IsSecondGun;
    public bool IsReloading;

    void Start()
    {
        WeaponManager = GameObject.FindGameObjectWithTag("WeaponManager");
        WMScript = WeaponManager.GetComponent<WeaponManager>();

        Player = GameObject.FindGameObjectWithTag("Player");
        PIC = Player.GetComponent<PlayerGetInCar>();
    }

    // Update is called once per frame
    void Update()
    {
        IsSecondGun = !WMScript.IsWeaponSwaped;

        //DIsableWhenInCar
        if (PIC.IsMounted)
        {
            Text.gameObject.SetActive(false);
            ReloadBar.gameObject.SetActive(false);
            GLAmmoText.gameObject.SetActive(false);
            GunNumber.gameObject.SetActive(false);
        }
        else
        {
            Text.gameObject.SetActive(true);
            ReloadBar.gameObject.SetActive(true);
            GLAmmoText.gameObject.SetActive(true);
            GunNumber.gameObject.SetActive(true);
        }

        //Get child
        Transform parent = WeaponManager.transform; 
        Transform firstGun = parent.GetChild(0);  
        Transform secondGun = parent.GetChild(1);

        GunScript gs = null;
        //Detect WhichGun
        if (IsSecondGun)
        {
            GunNumber.text = "1";

            gs = firstGun.GetComponent<GunScript>();
            CurrentAP = gs.GunPeneration;
            Text.text = gs.MagazineCounter.ToString() + "/" + gs.CurrentCapasity.ToString();
            if (gs.GS == GunScript.GunState.Reload)
            {
                ReloadBar.fillAmount = gs.ReloadTimer / gs.ReloadTime;
            }
           
        }
        else
        {
            GunNumber.text = "2";

            gs = secondGun.GetComponent<GunScript>();
            CurrentAP = gs.GunPeneration;
            Text.text = gs.MagazineCounter.ToString() + "/" + gs.CurrentCapasity.ToString();
            if (gs.GS == GunScript.GunState.Reload)
            {
                ReloadBar.fillAmount = gs.ReloadTimer / gs.ReloadTime;
            }
             
        }
        //DetectReloadState
        if (gs.GS == GunScript.GunState.Reload)
        {
            ReloadBar.enabled = true;
        }
        else
        {
            ReloadBar.enabled = false;
        }

        //GL UI
        GLAmmoText.text = WMScript.CurrentCount.ToString();
    }
}
