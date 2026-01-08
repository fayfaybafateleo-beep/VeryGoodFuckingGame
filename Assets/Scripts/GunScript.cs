using UnityEngine;
using Unity.Cinemachine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.Profiling;
using TMPro;
using System.Collections;
using System.Collections.Generic;


public class GunScript : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [Header("WhenUcantEvenSay MYNAME")]
    public string Name;
    public int Cost;
    public string GunType;

    [Header("Bullet")]
    public Transform FirePoint;
    public int CurrentFirePointIndex = 0;
    public bool IsMultiFirepoint=false;
    public List<Transform> FirePoints = new List<Transform>();
    public Bullet BulletPrefab;
    public int PenertrateCount=1;

    [Header("FireRate")]
    [Range(0, 3000)]
    public float FireRate; 
    private float FireTimer;
    [Range(0, 3000)]
    public float BurstRate;
    public bool IsBurstFire;


    [Header("Burst Fire")]
    [Range(2, 30)] public int BurstCount = 3;  
    public bool IsBursting = false;
    public float BurstInterval;

    [Header("Reload")]
    public int MagazineCount;
    public int MagazineCounter;
    public float ReloadTime;
    public float ReloadTimer;
    public bool NeedReload;

    [Header("MOA")]
    public float VerticalSpreadAngle = 6f;
    public float HorizontalSpreadAngle = 6f;

    [Header("SlugNumber")]
    public int SlugCount;
    public GameObject Shell;
    public Transform ShellPoint;

    [Header("Sound")]
    public AudioSource AudioSource;
    public AudioClip ClipShooting;
    public AudioClip ClipCocking;

    [Header("Screenshake")]
    public CinemachineImpulseSource Impulse;
    public Test Recoil;
    public float RecoilStrength;

    [Header("Animations")]
    public Animator GunAnimator;
    public float AnimatorSpeed;
    public GunShake shake;

    [Header("GunData")]
    public float OriginGunDamage;
    public float GunDamage;
    public int GunPeneration;

    [Header("ZeroTheFirePoint")]
    public Camera Camera;
    public float MaxDistance;
    public LayerMask HitLayers = ~0;         
    public LayerMask IgnoreLayers = 0;   
    public float Offset = 0.1f;

    [Header("ZeroTheFirePoint")]
    public Volume GlobalVolume;     
    public MotionBlur MotionBlur;  

    public GameObject GunModel;
    public GameObject MuzzleFlash;
    public WeaponWaging WeaponWagingScript;

    [Header("ReloadHint")]
    public GameObject Reminder;
    public KeyCode ReloadKey ;
    public TextMeshPro ReloadText;

    [Header("AmmunitionSystem")]
    public float InitialCapasityRate;
    public int MaxCapasity;
    public int CurrentCapasity;
    public float AmmoConvertRate;

    [Header("Buff")]
    public BuffManager BM;

    [Header("MoveSpeedOnFire")]
    public float FireMoveSpeedMultiplier = 0.7f;  
    public bool IsFiringNow;                       
    public float DefaultMoveSpeedMultiplier = 1f;  
    public enum GunState
    {
       CanFire,
       CeaseFire,
       Reload
    }
    public GunState GS;
    // Update is called once per frame
    void Start()
    {
        Camera = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();

        GlobalVolume = GameObject.FindGameObjectWithTag("GlobalVolume").GetComponent<Volume>();
        GlobalVolume.profile.TryGet(out MotionBlur);

        MagazineCounter = MagazineCount;

        Reminder.SetActive(false);

        CurrentCapasity = Mathf.RoundToInt( InitialCapasityRate* MaxCapasity);

        ReloadText = Reminder.GetComponentInChildren<TextMeshPro>();

        BurstInterval = 60 / BurstRate ;

        BM = GameObject.FindGameObjectWithTag("BuffManager").GetComponent<BuffManager>();

        OriginGunDamage = GunDamage;

        Recoil = GameObject.FindGameObjectWithTag("Recoil").GetComponent<Test>();

        DefaultMoveSpeedMultiplier =1;
    }
    void Update()
    {
        //Damage
        GunDamage = OriginGunDamage * (1 + BM.DamageIncreaseRate);

        //Zero The FirePoint
        Vector3 RayOrigin = Camera.transform.position + Camera.transform.forward * Offset;
        Vector3 RayDir = Camera.transform.forward;

        int mask = HitLayers;
        if (IgnoreLayers.value != 0)
        {
          mask = HitLayers & ~IgnoreLayers;
        }
        Vector3 EndPoint;
        if (Physics.Raycast(RayOrigin, RayDir, out RaycastHit hit, MaxDistance, mask, QueryTriggerInteraction.Ignore))
        {
            EndPoint = hit.point;
        }
        else
        {
            EndPoint = RayOrigin + RayDir * MaxDistance;
        }
        Vector3 DirFromMuzzle = (EndPoint - FirePoint.position).normalized;

        Quaternion aim = Quaternion.LookRotation(DirFromMuzzle, Vector3.up);

        if(FirePoints.Count > 0)
        {
            foreach(var fp in FirePoints)
            {
                fp.rotation = aim;
            }
        }

        FirePoint.rotation = aim;

        if (MagazineCounter <= 0)
        {
     
            NeedReload = true;
        }

        if (NeedReload == false)
        {
            ReloadTimer = 0;
        }

        //Reload
        if (NeedReload)
        {
            GS = GunState.Reload;
        }

        if (GS == GunState.Reload)
        {
            Reminder.SetActive(true);
        }
        else
        {
            Reminder.SetActive(false);
        }

        //BackupAmmo
        if (CurrentCapasity <= 0)
        {
            CurrentCapasity = 0;
            if (MagazineCounter <= 0)
            {
                ReloadText.text = "NO AMMO";
            }
        }
        else
        {
            ReloadText.text = "RELOADING";
        }

        if(CurrentCapasity >= MaxCapasity)
        {
            CurrentCapasity = MaxCapasity;
        }
        switch (GS)
        {
            case GunState.CanFire:
                FireTimer -= Time.deltaTime;
                GunAnimator.speed = AnimatorSpeed;
                // MousePressed
                if (IsBurstFire)
                {
                    if (Input.GetMouseButton(0) && MagazineCounter > 0 && !IsBursting)
                    {
                        if (FireTimer > 0f) break;  

                        FireTimer = 60f / FireRate;

                        StartCoroutine(BurstRoutine());
                    }
                }
                else
                {
                    if (Input.GetMouseButton(0) && MagazineCounter > 0)
                    {
                        if (FireTimer > 0f) break;

                        FireTimer = 60f / FireRate;

                        GunFire();
                    }
                }

                //Reload
                if (Input.GetKeyDown(ReloadKey)&& MagazineCounter<MagazineCount && CurrentCapasity > 0)
                {
                    NeedReload = true;
                    Reminder.GetComponent<Animator>().SetTrigger("Start");
                    GS = GunState.Reload;
                }

                //Mov eSpeed Modificate
                bool holdingFire = Input.GetMouseButton(0);
                if (holdingFire || IsBursting)
                {
                    StartFireSlow(); // [MOD]
                }
                else
                {
                    StopFireSlow();  // [MOD]
                }
                break;
            case GunState.CeaseFire:
                SetBlur(0);
                IsBursting = false;
                //Reload
                if (MagazineCounter <= 0)
                {
                    GS = GunState.Reload;
                }

                StopFireSlow();
                break;

            case GunState.Reload:
                IsBursting = false;
                if (NeedReload && CurrentCapasity > 0)
                {
                    ReloadTimer += Time.deltaTime;
                }
                SetBlur(0);
                if (ReloadTimer >= ReloadTime)
                {
                
                    FinishReloading();
                }

                StopFireSlow();
                break;

        }
        if (Input.GetMouseButtonUp(0))
        {
            SetBlur(0);
            StopFireSlow();
        }

      
    }
    public void IdleAnimation()
    {
        GunAnimator.SetTrigger("Idle");
    }

    public void SetBlur(float intensity)
    {
      MotionBlur.intensity.Override(intensity);
    }
    public void ShellEject()
    {
        if (Shell == null) return;
        GameObject shell = Instantiate(Shell, ShellPoint.position, Quaternion.Euler(90,0,0));

        Rigidbody rb = shell.GetComponent<Rigidbody>();

        Vector3 ejectDir = (-ShellPoint.right + ShellPoint.up).normalized; 
        rb.AddForce(ejectDir * Random.Range(1f, 2f), ForceMode.Impulse);

        rb.AddTorque(Random.insideUnitSphere * 2f, ForceMode.Impulse);

        Destroy(shell, 5f);
    }
    public void FinishReloading()
    {
        if (NeedReload==false || CurrentCapasity<=0) return;

        //Ammo
        if (CurrentCapasity+MagazineCounter <= MagazineCount)
        {
            int addonAmmo = CurrentCapasity;
            CurrentCapasity -= CurrentCapasity;
            MagazineCounter += addonAmmo;
        }
        else
        {
            CurrentCapasity -= (MagazineCount - MagazineCounter);
            MagazineCounter = MagazineCount;
        }
        GS = GunState.CanFire;
        NeedReload = false;
        ReloadTimer = 0;
        //Animation
        Reminder.GetComponent<Animator>().SetTrigger("End");
    }

    public void GetBackUpAmmo(float ammoBoxMultiPulier)
    {
        CurrentCapasity += Mathf.RoundToInt(ammoBoxMultiPulier * AmmoConvertRate * MagazineCount);
    }

    public void GunFire()
    {
        Transform currentFirePoint;

        if (FirePoints != null && FirePoints.Count > 0)
        {
            currentFirePoint = FirePoints[CurrentFirePointIndex];

            CurrentFirePointIndex++;
            if (CurrentFirePointIndex >= FirePoints.Count)
                CurrentFirePointIndex = 0;
        }
        else
        {
            currentFirePoint = FirePoint;
        }

        // Sound
        AudioSource.PlayOneShot(ClipShooting);
        // Fire
        WeaponWagingScript.SuppressSwayOnFire();
        //SlugCount
        SetBlur(1);
        for (int i = 0; i < SlugCount; i++)
        {
            Vector2 c = Random.insideUnitCircle;

            float yaw = c.x * HorizontalSpreadAngle;
            float pitch = c.y * VerticalSpreadAngle;

            Quaternion spreadRot = currentFirePoint.rotation * Quaternion.Euler(pitch, yaw, 0f);

            //DataSync
            Bullet bullet = Instantiate(BulletPrefab, currentFirePoint.position, spreadRot);
            bullet.GetComponent<Bullet>().Damage = GunDamage;
            bullet.GetComponent<Bullet>().PenatrateLevel = GunPeneration;
            bullet.GetComponent<Bullet>().MaxPenetrateTargets = PenertrateCount;
            bullet.GetComponent<Bullet>().GunType=GunType;
        }
        //muzzleFlash//
        GameObject muzzleFlash = Instantiate(MuzzleFlash, currentFirePoint.position, currentFirePoint.rotation);
        muzzleFlash.transform.SetParent(currentFirePoint);

        //Shell//
        ShellEject();

        //Animations
        shake.AddRecoil(1f);
        GunAnimator.SetTrigger("Fire");

        //ammo
        MagazineCounter -= 1;

        //VeiwKick
        Recoil.Kick(RecoilStrength);

        // Screenshake
        Impulse.GenerateImpulse();
    }

    public IEnumerator BurstRoutine()
    {
        IsBursting = true;

        int shots = Mathf.Min(BurstCount, MagazineCounter);

        for (int i = 0; i < shots; i++)
        {
            if (GS != GunState.CanFire || MagazineCounter <= 0) break;

            GunFire();  

            if (i < shots - 1)
            {
                yield return new WaitForSeconds(BurstInterval);
            }
        }

        StopFireSlow();
        IsBursting = false;
    }
    public void StartFireSlow()
    {
        if (BM == null) return;
        if (IsFiringNow) return;

        IsFiringNow = true;
        BM.SpeedModificator = FireMoveSpeedMultiplier;
    }

    public void StopFireSlow()
    {
        if (BM == null) return;

        IsFiringNow = false;
        BM.SpeedModificator = DefaultMoveSpeedMultiplier; 
    }
}
