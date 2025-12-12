using UnityEngine;

public class RecoilComplie : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [Header("References")]
    public Transform cameraPivot; 
   // public ViewKickRecoil recoil; 

    [Header("Settings")]
    public float sensitivity = 2.0f;
    public float minPitch = -85f;
    public float maxPitch = 85f;

    float yaw;
    float pitch;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

    //    if (recoil == null && cameraPivot != null)
   //         recoil = cameraPivot.GetComponent<ViewKickRecoil>();
    }

    void Update()
    {
        float mx = Input.GetAxisRaw("Mouse X") * sensitivity;
        float my = Input.GetAxisRaw("Mouse Y") * sensitivity;

        yaw += mx;
        pitch -= my;

        // 叠加 recoil（关键：recoil 是额外偏移，玩家鼠标仍然能抵消它）
    //    Vector2 rk = recoil ? recoil.UpdateRecoil() : Vector2.zero;

     //   float finalPitch = Mathf.Clamp(pitch + rk.x, minPitch, maxPitch);
     //   float finalYaw = yaw + rk.y;

        // yaw：转玩家根物体（左右）
    //    transform.rotation = Quaternion.Euler(0f, finalYaw, 0f);
        // pitch：转 pivot（上下）
    //    if (cameraPivot != null)
      //      cameraPivot.localRotation = Quaternion.Euler(finalPitch, 0f, 0f);
    }
}
