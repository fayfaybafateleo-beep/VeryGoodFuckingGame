using UnityEngine;
using Unity.Cinemachine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.Profiling;
using TMPro;


public class GunScript : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [Header("WhenUcantEvenSay MYNAME")]
    public string Name;
    [Header("Bullet")]
    public Transform FirePoint;
    public Bullet BulletPrefab;

    [Header("FireRate")]
    [Range(0, 3000)]
    public float FireRate; 
    private float FireTimer;

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

    [Header("Animations")]
    public Animator GunAnimator;
    public float AnimatorSpeed;
    public GunShake shake;

    [Header("GunData")]
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
    }
    void Update()
    {
        //ZeroTheFirePoint
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
        FirePoint.rotation = aim;

        Debug.DrawRay(RayOrigin, RayDir * MaxDistance, Color.cyan);             
        Debug.DrawRay(FirePoint.position, DirFromMuzzle * MaxDistance, Color.red);

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
                // Mouse pressed
                if (Input.GetMouseButton(0))
                {
                    // Gun not ready to shoot yet
                    if (FireTimer > 0)
                    {
                        // AudioSource.PlayOneShot(ClipCocking);
                        return;
                    }

                    // Starts CountDown in RPM rate
                    FireTimer = 60 / FireRate;
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

                        Quaternion spreadRot = FirePoint.rotation * Quaternion.Euler(pitch, yaw, 0f);

                        Bullet bullet = Instantiate(BulletPrefab, FirePoint.position, spreadRot);
                        bullet.GetComponent<Bullet>().Damage = GunDamage;
                        bullet.GetComponent<Bullet>().PenatrateLevel = GunPeneration;
                    }
                    //muzzleFlash//
                    GameObject muzzleFlash = Instantiate(MuzzleFlash, FirePoint.position, FirePoint.rotation);
                    muzzleFlash.transform.SetParent(FirePoint);

                    //Shell//
                    ShellEject();

                    //Animations
                    shake.AddRecoil(1f);
                    GunAnimator.SetTrigger("Fire");

                    //ammo
                    MagazineCounter -= 1;

                    // Screenshake
                    Impulse.GenerateImpulse();
                }

                //Reload
                if (Input.GetKeyDown(ReloadKey)&& MagazineCounter<MagazineCount && CurrentCapasity > 0)
                {
                    NeedReload = true;
                    Reminder.GetComponent<Animator>().SetTrigger("Start");
                    GS = GunState.Reload;
                }
                break;
            case GunState.CeaseFire:
                SetBlur(0);

                //Reload
                if (MagazineCounter <= 0)
                {
                    GS = GunState.Reload;
                }
                break;

            case GunState.Reload:
                if (NeedReload && CurrentCapasity > 0)
                {
                    ReloadTimer += Time.deltaTime;
                }
                SetBlur(0);
                if (ReloadTimer >= ReloadTime)
                {
                
                    FinishReloading();
                }

                break;

        }
        if (Input.GetMouseButtonUp(0))
        {
            SetBlur(0);
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
}
