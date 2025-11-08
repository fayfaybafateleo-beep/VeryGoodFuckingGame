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

    [Header("UI")]
    public TextMeshProUGUI Text;
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
        }
        else
        {
            Text.gameObject.SetActive(true);
            ReloadBar.gameObject.SetActive(true);
        }

        //Get child
        Transform parent = WeaponManager.transform; 
        Transform firstGun = parent.GetChild(0);  
        Transform secondGun = parent.GetChild(1);

        GunScript gs = null;
        //Detect WhichGun
        if (IsSecondGun)
        {
            gs = firstGun.GetComponent<GunScript>();
            Text.text = gs.MagazineCounter.ToString() + "/" + gs.CurrentCapasity.ToString();
            if (gs.GS == GunScript.GunState.Reload)
            {
                ReloadBar.fillAmount = gs.ReloadTimer / gs.ReloadTime;
            }
        }
        else
        {
            gs = secondGun.GetComponent<GunScript>();
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
    }
}
