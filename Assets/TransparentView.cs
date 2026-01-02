using UnityEngine;

public class TransparentView : MonoBehaviour
{
    [Header("OverlayCamera")]
    public GameObject Cam;
   
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Cam = GameObject.FindGameObjectWithTag("TransparentUiCam");
    }

    public void ActivateScope()
    {
        Cam.SetActive(true);
    }
    public void DisableScope()
    {
        Cam.SetActive(false);
    }
}
