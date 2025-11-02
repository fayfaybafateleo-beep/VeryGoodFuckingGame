using System.Collections;
using System.Collections.Generic;
using Unity.Cinemachine;
using UnityEngine;


public class WeaponManager : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public List<GameObject> OriginWeaponsList;

    public List<GameObject> WeaponsOnEquipmentList;

    public List<GameObject> GrenadeList;
    [Header (" KeyOfInput")]
    public KeyCode Key = KeyCode.Q;
    public KeyCode Key2 = KeyCode.E;
    public KeyCode Key3 = KeyCode.F;

    [Header(" WeaponAttacher")]
    public GameObject WeaponParent;

    public bool IsWeaponSwaped=false;
    public int I = 0;
    [Header(" SwapCD")]
    public bool CanSwap = true;
    public Transform FP;
    Vector3 up = Vector3.up;

    [Header("GrenadeLaucner")]
    public Animator GL;
    public CinemachineImpulseSource Impulse;
    public bool IsBursting = false;
    public GameObject MuzzelFlash;
    //sound
    public AudioSource GLsound;
    public AudioClip GLFireClip;

    [Header("Melee")]
    public float MeleeRate;
    public float MeleeRateTimer;
    public Animator MeleeAnimator;
    public CinemachineImpulseSource MeleeImpulseSource;

    [Header("MeleeSound")]
    public AudioSource MeleeSound;
    public AudioClip FireClip;

    public enum FireControlState
    {
        AllowInput,
        CantInput
    }
    public FireControlState FCS;
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
    public void OnEnable()
    {
        //RecoverWhileGetOffTheCar
        EnableWeaponsWhileFinishInteract();

    }
    // Update is called once per frame
    void Update()
    {
        switch (FCS)
        {
            case FireControlState.AllowInput:
                if (Input.GetKeyDown(Key2) && IsBursting == false)
                {
                    GL.speed = GrenadeList[0].GetComponent<Grenades>().GLAnimationSpeed;
                    StartCoroutine(FireGrenadeBurst());
                }

                MeleeRateTimer += Time.deltaTime;

                if (MeleeRateTimer >= MeleeRate)
                {
                    MeleeRateTimer = MeleeRate;
                }

                if (Input.GetKeyDown(Key3))
                {
                    if (MeleeRateTimer < MeleeRate) return;
                    MeleeAnimator.SetTrigger("Fire");
                    MeleeHitImpulse();
                    MeleeAnimator.speed = 5;
                    MeleeSound.PlayOneShot(FireClip);
                    MeleeRateTimer = 0;
                }
            break;

            case FireControlState.CantInput:
                WeaponsOnEquipmentList[I + 1].GetComponent<GunScript>().GS = GunScript.GunState.CeaseFire;
                WeaponsOnEquipmentList[I].GetComponent<GunScript>().GS = GunScript.GunState.CeaseFire;
            break;
        }

        if (Input.GetKeyDown(Key) && CanSwap)
        {
            CanSwap = false;
            if (IsWeaponSwaped)
            {
                //StopSecondWeaponOnFire
                WeaponsOnEquipmentList[I + 1].GetComponent<GunScript>().GS = GunScript.GunState.CeaseFire;
                //SetGunActive

                var oldSway = WeaponsOnEquipmentList[I + 1].GetComponent<WeaponWaging>();
                if (oldSway) oldSway.LowerForReload(0.12f);

                WeaponsOnEquipmentList[I].GetComponent<GunScript>().IdleAnimation();
                WeaponsOnEquipmentList[I + 1].SetActive(false);
                WeaponsOnEquipmentList[I].SetActive(true);

                var newSwayA = WeaponsOnEquipmentList[I].GetComponent<WeaponWaging>();
                if (newSwayA) { newSwayA.SnapToLow(); newSwayA.RaiseFromLow(); }

                IsWeaponSwaped = !IsWeaponSwaped;
                //SwapingCD
                StartCoroutine(SwapGun(I));
            }
            else
            {

                //StopFirstWeaponOnFire
                WeaponsOnEquipmentList[I].GetComponent<GunScript>().GS = GunScript.GunState.CeaseFire;
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
                StartCoroutine(SwapGun(I + 1));
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

    IEnumerator FireGrenadeBurst()
    {
        IsBursting = true;

        if (GrenadeList.Count == 0 || GrenadeList[0] == null)
        {
            IsBursting = false;
            yield break;
        }
        // LoadingGLdata
        Grenades gScript = GrenadeList[0].GetComponent<Grenades>();
        int burstCount =  gScript.BurstCount;
        float burstInterval = gScript.BurstInterval ;

        for (int i = 0; i < burstCount; i++)
        {
    
            GL.SetTrigger("Fire");
            // Interval
            if (i < burstCount)
            {
                yield return new WaitForSeconds(burstInterval);
            }
               
        }

        IsBursting = false;
    }

    public void SpawnGuns(int activeGun)
    {
        //ClearAll
        if (WeaponsOnEquipmentList != null)
        {
            foreach (var gun in WeaponsOnEquipmentList)
            {
                if (gun) Destroy(gun);
            }
            WeaponsOnEquipmentList.Clear();
        }

        if (OriginWeaponsList != null)
        {
            Transform parent = WeaponParent ? WeaponParent.transform : this.transform;

            for (int count = 0; count < OriginWeaponsList.Count; count++)
            {
                var gunPrefab = OriginWeaponsList[count];
                if (!gunPrefab) { WeaponsOnEquipmentList.Add(null); continue; }

                GameObject newGun = Instantiate(gunPrefab, parent);
                newGun.transform.localPosition = Vector3.zero;
                newGun.transform.localRotation = Quaternion.identity;
                WeaponsOnEquipmentList.Add(newGun);
            }
        }

        // 3) ActivatedTheHolding
        if (WeaponsOnEquipmentList.Count > 0)
        {
            if (activeGun < 0 || activeGun >= WeaponsOnEquipmentList.Count)
                activeGun = 0;

            for (int k = 0; k < WeaponsOnEquipmentList.Count; k++)
            {
                var gun = WeaponsOnEquipmentList[k];
                bool active = (k == activeGun);

                if (gun)
                {
                    gun.SetActive(active);
                    var gs = gun.GetComponent<GunScript>();
                    if (gs) gs.GS = active ? GunScript.GunState.CanFire : GunScript.GunState.CeaseFire;
                }
            }
            // synqunized
            if (activeGun == I)
            {
                IsWeaponSwaped = false;
            }
            else if (activeGun == I + 1)
            {
                IsWeaponSwaped = true;
            }
            else
            {
                I = activeGun;
                IsWeaponSwaped = false;
            }
            CanSwap = true;
        }
    }
    public void GLFire()
   {
        Instantiate(GrenadeList[0], FP.position, FP.rotation);
        Instantiate(MuzzelFlash, FP.position, FP.rotation);
        GLsound.PlayOneShot(GLFireClip);
        Impulse.GenerateImpulse();
    }
    public void MeleeHitImpulse()
    {
        MeleeImpulseSource.ImpulseDefinition.AmplitudeGain = 0.5f;
        MeleeImpulseSource.ImpulseDefinition.FrequencyGain = 0.5f;
        MeleeImpulseSource.GenerateImpulse();
    }

    public void EnableWeaponsWhileFinishInteract()
    {
        CanSwap = true;
        if (IsWeaponSwaped)
        {
            WeaponsOnEquipmentList[I + 1].GetComponent<GunScript>().GS = GunScript.GunState.CanFire;
        }
        else
        {
            WeaponsOnEquipmentList[I].GetComponent<GunScript>().GS = GunScript.GunState.CanFire;
        }
    }
}
