using UnityEngine;

public class MuzzleFlash : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public float DestroyTime;
    [Range(0, 360)]
    public float MuzzleFlashOffset;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Vector3 rot = transform.localEulerAngles;

        // 修改其中一个分量（Z 轴）
        rot.z = Random.Range(-MuzzleFlashOffset,MuzzleFlashOffset); // 设置为绝对角度（例如 45°）
        transform.localEulerAngles = rot;
        Destroy(gameObject, DestroyTime);
    }

    // Update is called once per frame
    void Update()
    {
       
    }
}
