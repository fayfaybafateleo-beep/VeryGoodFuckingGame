using Unity.VisualScripting;
using UnityEngine;

public class ParkingZone : MonoBehaviour
{
    public GameObject Car;
    public CarControl CC;

    public bool IsOccupied = false;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Car = GameObject.FindGameObjectWithTag("Car");
        CC = Car.GetComponent<CarControl>();
    }

    // Update is called once per frame
    void Update()
    {
      
        
    }
    public void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Car")
        {
            CC.CanGetDown = true;
        }
    }

    public void OnTriggerStay(Collider other)
    {
        if (other.gameObject.tag == "Car")
        {
            CC.CanGetDown = true;
        }
    }

    public void OnTriggerExit(Collider other)
    {
        if (other.gameObject.tag == "Car")
        {
            CC.CanGetDown = false;
        }
    }
    private void OnDisable()
    {
        CC.CanGetDown = false;
    }
}
