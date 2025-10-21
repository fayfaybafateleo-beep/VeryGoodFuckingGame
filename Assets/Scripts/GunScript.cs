using UnityEngine;
using Unity.Cinemachine;

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
    // Slug Count//
    public int SlugCount;

    public GameObject GunModel;
    public GameObject MuzzleFlash;

    
  
    // Update is called once per frame
    void Update()
    {
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
