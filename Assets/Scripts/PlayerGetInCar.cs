using StarterAssets;
using UnityEngine;

public class PlayerGetInCar : MonoBehaviour
{
    [Header("CarDetection")]
    public GameObject Car;               
    public KeyCode Key = KeyCode.V;     
    public float mountDistance = 3f;         

    [Header("PlayerControl&Weapon")]
    public FirstPersonController PlayerController;
    public GameObject MainCamera;

    [Header("Seats")]
    public GameObject SeatPoint;
    public GameObject DropPoint;
    public bool IsMounted = false;

    
    public Transform OriginalParent;
    public int OriginalSiblingIndex;

    public void Start()
    {
       PlayerController = GetComponent<FirstPersonController>();

       Car = GameObject.FindGameObjectWithTag("Car");

       SeatPoint = GameObject.FindGameObjectWithTag("SeatPoint");

       DropPoint = GameObject.FindGameObjectWithTag("DropPoint");

       MainCamera = GameObject.FindGameObjectWithTag("MainCamera");
    }

    public void Update()
    {
        // Dismount
        if (IsMounted && Input.GetKeyDown(Key))
        {
            DismountVehicle();
            return;
        }

        if (IsMounted) return; 

        if (Car == null) return;

        float distance = Vector3.Distance(transform.position, Car.transform.position);

        if (distance <= mountDistance && Input.GetKeyDown(Key))
        {
            MountVehicle();
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

        IsMounted = false;
    }
}

