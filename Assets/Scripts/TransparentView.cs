using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

public class TransparentView : MonoBehaviour
{
    [Header("OverlayCamera")]
    public GameObject Cam;
    public bool IsUsing;

    [Header("UI")]
    public Animator ScopeAnimator;

    [Header("Input")]
    public KeyCode Key = KeyCode.U;
    public float InputCooldown = 1f;
    private float lastInputTime;

    [Header("ScreenShake")]
    public CinemachineImpulseSource Source;

    [Header("Player")]
    public PlayerHealth PH;
    public PlayerGetInCar PIC;

    [Header("ScopeSound")]
    public AudioSource ASource;
    public AudioClip Scope;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Cam = GameObject.FindGameObjectWithTag("TransparentUiCam");
        Cam.SetActive(false);

        PH = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerHealth>();
        PIC = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerGetInCar>();
    }
    void Update()
    {
        if (PH.PS == PlayerHealth.PlayerState.Alive)
        {
            if (Input.GetKeyDown(Key))
            {
                if (Time.time - lastInputTime < InputCooldown)
                {
                    return;
                }
                lastInputTime = Time.time;
                if (IsUsing)
                {
                    DisableScope();
                }
                else
                {
                    ActivateScope();
                }
            }
        }
      
    }
    public void ActivateScope()
    {
        ScopeAnimator.SetTrigger("Use");
       
        IsUsing = !IsUsing;
    }
    public void DisableScope()
    {
        ScopeAnimator.SetTrigger("NotUse");
       
        IsUsing = !IsUsing;
    }

    public void EnableCam()
    {
        Cam.SetActive(true);
       
    }

    public void DisableCam()
    {
        Cam.SetActive(false);
    }
    public void DownShake()
    {
        Source.GenerateImpulse();
    }
    public void UpShake()
    {
        Source.GenerateImpulse(-1);
    }
    public void ScopeSound()
    {
        ASource.PlayOneShot(Scope);
    }
}
