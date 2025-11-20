using TMPro;
using UnityEngine;

public class DetectionRay : MonoBehaviour
{
    [Header("RayCastInfo")]
    public float MaxDistance = 100f;         
    public LayerMask EnemyLayer;            
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void DetectEnemy()
    {
        // 从屏幕中心发射射线
        Camera cam = Camera.main;
        if (cam == null)
        {
            Debug.LogWarning("场景里没有标记为 MainCamera 的摄像机！");
            return;
        }

        // Viewport 坐标(0.5, 0.5)是屏幕正中心
        Ray ray = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        Debug.DrawRay(ray.origin, ray.direction * MaxDistance, Color.red, 1f);

        if (Physics.Raycast(ray, out RaycastHit hit, MaxDistance, EnemyLayer))
        {
            // 命中了某个 collider
            EnemyHealth eh = GetComponentInParent<EnemyHealth>();
            EnemyBehaviour eb  = GetComponentInParent<EnemyBehaviour>();
            if (eh != null)
            {
                
            }
            else
            {
               
            }
            if(eb!=null && eb.ES == EnemyBehaviour.EnemyState.Shock)
            {

            }
        }
        else
        {
            //
        }
    }
}
