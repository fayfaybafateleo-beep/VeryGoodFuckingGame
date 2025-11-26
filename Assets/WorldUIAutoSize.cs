using UnityEngine;

public class WorldUIAutoSize : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public Camera PlayerCamera;
    public float SizeMultiplier = 0.1f;     
    public float HideDistance = 2f;         

    private Vector3 initialScale;

    void Start()
    {
        initialScale = transform.localScale;
        PlayerCamera = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
    }

    void Update()
    {
        float distance = Vector3.Distance(PlayerCamera.transform.position, transform.position);

        // 距离近时自动隐藏
        if (distance < HideDistance)
        {
            if (gameObject.activeSelf)
                gameObject.SetActive(false);
            return;
        }
        else
        {
            if (!gameObject.activeSelf)
                gameObject.SetActive(true);
        }

        // 保持视觉大小
        float scaleFactor = distance * SizeMultiplier;
        transform.localScale = initialScale * scaleFactor;
    }
}
