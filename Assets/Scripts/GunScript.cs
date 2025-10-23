using UnityEngine;
using Unity.Cinemachine;
using static UnityEditor.Experimental.GraphView.GraphView;
using UnityEditor.PackageManager;

public class GunScript : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [Header("Bullet")]
    public Transform FirePoint;
    public Bullet BulletPrefab;

    [Header("FireRate")]
    [Range(0, 5)]
    public float ReloadTime; // seconds
    private float ReloadTimer; // seconds

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


    [Header("ZeroTheFirePoint")]
    public Camera Camera;
    public float MaxDistance;
    public LayerMask HitLayers = ~0;         
    public LayerMask IgnoreLayers = 0;   
    public float Offset = 0.1f;          
    // Slug Count//
    public int SlugCount;

    public GameObject GunModel;
    public GameObject MuzzleFlash;
    public WeaponWaging WeaponWagingScript;


    // Update is called once per frame
    void Start()
    {
        Camera = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
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

        // 3) 应用（带可选的固定偏置角，修正模型轴）
        Quaternion aim = Quaternion.LookRotation(DirFromMuzzle, Vector3.up);
        FirePoint.rotation = aim;

        Debug.DrawRay(RayOrigin, RayDir * MaxDistance, Color.cyan);             // 相机射线
        Debug.DrawRay(FirePoint.position, DirFromMuzzle * MaxDistance, Color.red); // 枪口方向

        ReloadTimer -= Time.deltaTime;
       GunAnimator.speed = AnimatorSpeed;
        // Mouse pressed
        if (Input.GetMouseButton(0))
        {
            // Gun not ready to shoot yet
            if (ReloadTimer > 0)
            {
                // AudioSource.PlayOneShot(ClipCocking);
                return;
            }

            // Starts reloading
            ReloadTimer = ReloadTime;

            // Fire
            WeaponWagingScript.SuppressSwayOnFire();
            Bullet bullet = Instantiate(BulletPrefab, FirePoint.position, FirePoint.rotation);
            //muzzleFlash//
            // Instantiate(MuzzleFlash, FirePoint.position, FirePoint.rotation);
            GameObject muzzleFlash=Instantiate(MuzzleFlash, FirePoint.position, FirePoint.rotation);
            muzzleFlash.transform.SetParent(FirePoint);

            //Animations
            shake.AddRecoil(1f);
            GunAnimator.SetTrigger("Fire");
            // Sound
            AudioSource.PlayOneShot(ClipShooting);
            // Screenshake
            Impulse.GenerateImpulse();
        }
    }
}
