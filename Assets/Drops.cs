using UnityEngine;

public class Drops : MonoBehaviour
{
    [Header("ObjectType")]
    [Tooltip("Ammo1,Ammo2,AmmoG,Health,Gold")]
    public string Type;

    [Header("Reference")]
    public WeaponManager WMScript;
    public GameObject WM;

    [Header("DropsData")]
    public float GainMultipulier;
    public bool IsUsed=false;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        WM = GameObject.FindGameObjectWithTag("WeaponManager");
        WMScript = WM.GetComponent<WeaponManager>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void OnTriggerEnter(Collider other)
    {
        if (IsUsed) return;
        if (other.gameObject.tag == "Player")
        {

            //DifferentTypes,DifferentEffect
            if (Type == "Ammo1")
            {
                WM.transform.GetChild(0).GetComponent<GunScript>().GetBackUpAmmo(GainMultipulier);
            }
            if (Type == "Ammo2")
            {
                WM.transform.GetChild(1).GetComponent<GunScript>().GetBackUpAmmo(GainMultipulier);
            }
            if (Type == "AmmoG")
            {
                WMScript.GetBackUpGrenade();
            }
            Destroy(gameObject, 0f);
        }
    }
}
