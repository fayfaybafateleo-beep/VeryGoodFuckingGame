using UnityEngine;
using UnityEngine.UI;
using TMPro;


public class MagazineCounterUI : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [Header("WeaponManager")]
    public GameObject WeaponManager;
    public WeaponManager WMScript;

    [Header("UI")]
    public TextMeshProUGUI Text;
    public Image ReloadBar;
    public bool IsSecondGun;
    public bool IsReloading;

    void Start()
    {
        WeaponManager = GameObject.FindGameObjectWithTag("WeaponManager");
        WMScript = WeaponManager.GetComponent<WeaponManager>();
    }

    // Update is called once per frame
    void Update()
    {
        IsSecondGun = !WMScript.IsWeaponSwaped;

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
