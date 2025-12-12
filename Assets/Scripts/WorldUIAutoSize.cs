using UnityEngine;

public class WorldUIAutoSize : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public Camera PlayerCamera;
    public float SizeMultiplier = 0.1f;     
    public float HideDistance = 2f;         

    private Vector3 InitialScale;

    void Start()
    {
        InitialScale = transform.localScale;
        PlayerCamera = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
    }

    void Update()
    {
        float distance = Vector3.Distance(PlayerCamera.transform.position, transform.position);

        float scaleFactor = distance * SizeMultiplier;
        transform.localScale = InitialScale * scaleFactor;
    }
}
