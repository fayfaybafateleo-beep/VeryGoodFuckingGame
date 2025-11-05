using UnityEngine;
using Unity.Cinemachine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.Profiling;


public class GunScript : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [Header("WhenUcantEvenSay MYNAME")]
    public string Name;
    [Header("Bullet")]
    public Transform FirePoint;
    public Bullet BulletPrefab;
    public int MagazineCount;

    [Header("FireRate")]
    [Range(0, 3000)]
    public float FireRate; // RPM
    private float FireTimer; // seconds

    [Header("MOA")]
    public float SpreadAngle = 6f;

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

    public enum GunState
    {
       CanFire,
       CeaseFire
    }
    public GunState GS;
    // Update is called once per frame
    void Start()
    {
        Camera = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();

        GlobalVolume = GameObject.FindGameObjectWithTag("GlobalVolume").GetComponent<Volume>();
        GlobalVolume.profile.TryGet(out MotionBlur);
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

        Debug.DrawRay(RayOrigin, RayDir * MaxDistance, Color.cyan);             // 相机射线
        Debug.DrawRay(FirePoint.position, DirFromMuzzle * MaxDistance, Color.red); // 枪口方向
        switch (GS)
        {
            case GunState.CanFire:
                FireTimer -= Time.deltaTime;
                GunAnimator.speed = AnimatorSpeed;
                // Mouse pressed
                if (Input.GetMouseButton(0))
                {
                    SetBlur(1);
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
                    for (int i = 0; i < SlugCount; i++)
                    {
                        Vector2 c = Random.insideUnitCircle; 
                                                            
                        float yaw = c.x * SpreadAngle;
                        float pitch = c.y * SpreadAngle;

                        Quaternion spreadRot = FirePoint.rotation * Quaternion.Euler(pitch, yaw, 0f);

                        Bullet bullet = Instantiate(BulletPrefab, FirePoint.position, spreadRot);
                        bullet.GetComponent<Bullet>().Damage = GunDamage;
                        bullet.GetComponent<Bullet>().PenatrateLevel = GunPeneration;
                    }

                    //muzzleFlash//
                    // Instantiate(MuzzleFlash, FirePoint.position, FirePoint.rotation);
                    GameObject muzzleFlash = Instantiate(MuzzleFlash, FirePoint.position, FirePoint.rotation);
                    muzzleFlash.transform.SetParent(FirePoint);
                    //Shell//
                    ShellEject();
                    //Animations
                    shake.AddRecoil(1f);
                    GunAnimator.SetTrigger("Fire");

                    // Screenshake
                    Impulse.GenerateImpulse();
                }
                break;
            case GunState.CeaseFire:
                SetBlur(0);
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
}
