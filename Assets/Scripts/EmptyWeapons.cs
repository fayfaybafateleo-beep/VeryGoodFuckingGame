using UnityEngine;

public class EmptyWeapons : MonoBehaviour
{
    public GameObject FirstChild;
    public GameObject SecondChild;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (transform.childCount >= 2)
        {
            FirstChild = transform.GetChild(0).gameObject;
            SecondChild = transform.GetChild(1).gameObject;
        }
        Invoke("EmptyWeapon", 0.01f);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void EmptyWeapon()
    {
        FirstChild.GetComponent<GunScript>().CurrentCapasity = 0;
        FirstChild.GetComponent<GunScript>().MagazineCounter = 0;

        SecondChild.GetComponent<GunScript>().CurrentCapasity = 0;
        SecondChild.GetComponent<GunScript>().MagazineCounter = 0;
    }
}
