using UnityEngine;

public class PlayerGetInCar : MonoBehaviour
{
    [Header("CarDetection")]
    public GameObject Vehicle;               
    public KeyCode enterKey = KeyCode.V;     
    public float mountDistance = 3f;         

    [Header("PlayerControl")]
    public CharacterController PlayerController;

    [Header("Seats")]
    public GameObject SeatPoint;
    public GameObject DropPoint;
    public bool IsMounted = false;

    
    public Transform OriginalParent;
    public int OriginalSiblingIndex;

    public void Start()
    {
       PlayerController = GetComponent<CharacterController>();

       Vehicle = GameObject.FindGameObjectWithTag("Car");

       SeatPoint = GameObject.FindGameObjectWithTag("SeatPoint");

       DropPoint = GameObject.FindGameObjectWithTag("DropPoint");

    }

    public void Update()
    {
        // Dismount
        if (IsMounted && Input.GetKeyDown(enterKey))
        {
            DismountVehicle();
            return;
        }

        if (IsMounted) return; 

        if (Vehicle == null) return;

        float distance = Vector3.Distance(transform.position, Vehicle.transform.position);

        if (distance <= mountDistance && Input.GetKeyDown(enterKey))
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
        PlayerController.enabled = false;

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

        Transform vt = Vehicle.transform;
        Quaternion dropRot = Quaternion.LookRotation(vt.forward, Vector3.up);

        transform.position = DropPoint.transform.position;
        transform.rotation = dropRot;

        PlayerController.enabled = true;

        IsMounted = false;
    }
}

