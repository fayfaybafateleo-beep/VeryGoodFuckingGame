using UnityEngine;

public class SizeSynchronizer : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public GameObject Owner;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        this.transform.localScale = Owner.transform.localScale;
    }
}
