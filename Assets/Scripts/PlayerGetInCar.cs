using StarterAssets;
using TMPro;
using UnityEngine;

public class PlayerGetInCar : MonoBehaviour
{
    [Header("CarDetection")]
    public GameObject Car;
    public CarControl CC;
    public KeyCode Key = KeyCode.V;     
    public float MountDistance = 3f;         

    [Header("PlayerControl&Weapon")]
    public FirstPersonController PlayerController;
    public GameObject MainCamera;

    [Header("Seats")]
    public GameObject SeatPoint;
    public GameObject DropPoint;
    public bool IsMounted = false;

    [Header("Fade Settings")]
    public float FadeDuration = 0.5f;
    private float FadeTimer;
    public CanvasGroup CG;
    public GameObject GetInText;
    public GameObject GetOutText;


    public Transform OriginalParent;
    public int OriginalSiblingIndex;
    public WeaponManager WM;

    public void Start()
    {
       PlayerController = GetComponent<FirstPersonController>();

       Car = GameObject.FindGameObjectWithTag("Car");

        CC = Car.GetComponent<CarControl>();

       SeatPoint = GameObject.FindGameObjectWithTag("SeatPoint");

       DropPoint = GameObject.FindGameObjectWithTag("DropPoint");

       MainCamera = GameObject.FindGameObjectWithTag("MainCamera");

        WM = GameObject.FindGameObjectWithTag("WeaponManager").GetComponent<WeaponManager>();

        GetInText.SetActive(false);
    }

    public void Update()
    {
        // Dismount
        if (IsMounted && Input.GetKeyDown(Key) && CC.CanGetDown)
        {
            DismountVehicle();
            return;
        }

        if (CC.CanGetDown && IsMounted)
        {
            GetOutText.SetActive(true);
        }
        else
        {
            GetOutText.SetActive(false);
        }

        if (IsMounted) return; 

        if (Car == null) return;

        float distance = Vector3.Distance(transform.position, Car.transform.position);

        if (distance <= MountDistance && Input.GetKeyDown(Key))
        {
            MountVehicle();
        }


        //Show
        if (distance <= MountDistance && IsMounted == false)
        {
            GetInText.SetActive(true);
            FadeTimer = FadeDuration;  // 重置淡出计时器，让它保持显示
        }

        //Fadeout
        if (GetInText.activeSelf && IsMounted == false)
        {
            if (FadeTimer > 0)
            {
                FadeTimer -= Time.deltaTime;
                CG.alpha = 1f;
            }
            else
            {
                CG.alpha -= Time.deltaTime / FadeDuration;

                if (CG.alpha <= 0f)
                {
                    CG.alpha = 0f;
                    GetInText.SetActive(false);
                }
            }
        }

       
    }

    public void MountVehicle()
    {
        if (SeatPoint == null) return;

        // RecordParent
        OriginalParent = transform.parent;
        OriginalSiblingIndex = transform.GetSiblingIndex();

        IsMounted = true;

        // Disable PlayerControl
        PlayerController.CS = FirstPersonController.ControllerState.StopMove;

        // Disable Weapons
        foreach (Transform child in MainCamera.transform)
        {
            if (child.name == "OverLayCamera") continue;
            child.gameObject.SetActive(false);
        }
        // SitOnSeat
        transform.SetParent(SeatPoint.transform, worldPositionStays: false);
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;

        GetInText.SetActive(false);
    }

    public void DismountVehicle()
    {
        if (!IsMounted) return;
 
        transform.SetParent(OriginalParent, worldPositionStays: true);
        if (OriginalParent != null)
        {
            transform.SetSiblingIndex(OriginalSiblingIndex); // RecoverSerial
        }

        Transform vt = Car.transform;
        Quaternion dropRot = Quaternion.LookRotation(vt.forward, Vector3.up);

        transform.position = DropPoint.transform.position;
        transform.rotation = dropRot;



        PlayerController.CS = FirstPersonController.ControllerState.CanMove;
        // Enable Weapons
        foreach (Transform child in MainCamera.transform)
        {
            if (child.name == "OverLayCamera") continue;
            child.gameObject.SetActive(true);
        }

        WM.RecoverGunBurst();

        IsMounted = false;
    }
}

