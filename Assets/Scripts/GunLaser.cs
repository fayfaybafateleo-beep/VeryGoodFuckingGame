using Unity.VisualScripting;
using UnityEngine;

public class GunLaser : MonoBehaviour
{
    [Header("Cam")]
    public Camera MainCamera;

    [Header("Raycast")]
    public float Offset = 0.05f;
    public float MaxDistance = 50f;
    public LayerMask HitLayers = ~0;
    public LayerMask IgnoreLayers;
    public QueryTriggerInteraction TriggerRule = QueryTriggerInteraction.Ignore;

    [Header("Laser")]
    public LineRenderer LR;

    [Header("EndDot")]
    public bool ShowEndDot = true;          
    public GameObject RedDot;         
    public float EndDotSize = 0.05f;        // sizeOfDot
    public Color EndDotColor = Color.red;   

    Transform DotPos;                        
    const float SurfaceOffset = 0.01f;       

    void Awake()
    {
        LR = GetComponent<LineRenderer>();
        LR.positionCount = 2;
        LR.useWorldSpace = false;

        MainCamera = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();

        var go = Instantiate(RedDot);
        DotPos = go.transform;
        DotPos.localScale = Vector3.one * EndDotSize;
        DotPos.gameObject.SetActive(false);
      
    }
    public void OnDisable()
    {
        if (DotPos != null)
        {
            DotPos.gameObject.SetActive(false);

        }
    }
    public void OnEnable()
    {
        if (DotPos != null)
        {
            DotPos.gameObject.SetActive(true);
        }
    }
    public void OnDestroy()
    {
        if (DotPos != null)
        {
            Destroy(DotPos.gameObject, 0f);
        }
    }
    void LateUpdate()
    {
        Vector3 rayOrigin = MainCamera.transform.position + MainCamera.transform.forward * Offset;
        Vector3 rayDir = MainCamera.transform.forward;

        int mask = HitLayers;
        if (IgnoreLayers.value != 0)
        {
            mask = HitLayers & ~IgnoreLayers;
        }

        // HitPoint
        Vector3 endPoint;
        bool hitSomething = Physics.Raycast(rayOrigin, rayDir, out RaycastHit hit, MaxDistance, mask, TriggerRule);
        if (hitSomething)
        {
            endPoint = hit.point + hit.normal * SurfaceOffset; // Avoid Z conflict
        }
        else
        {
            endPoint = rayOrigin + rayDir * MaxDistance;
        }
           

        // Laser line£¨
        LR.SetPosition(0, Vector3.zero);
        LR.SetPosition(1, transform.InverseTransformPoint(endPoint));

        // DotPosition&Using
    
      if (hitSomething)
      {
        DotPos.gameObject.SetActive(true);
        DotPos.position = hit.point + hit.normal * SurfaceOffset;
        DotPos.rotation = Quaternion.LookRotation(-hit.normal, Vector3.up); // ÌùÃæ
      }
      else
      {
        DotPos.gameObject.SetActive(false);
      }
        
    }
}
