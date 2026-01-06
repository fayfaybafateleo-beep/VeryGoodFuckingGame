using UnityEngine;

public class Compass : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [Header("CompassData")]
    public Transform Target;
    public float TurnSpeed = 12f;
    public Transform This;

    [Header("Compass")]
    public GameObject Arrow;
    public GameObject ArrowBody;

    [Header("BaseLocation")]
    public Transform BaseLocation;
    void Update()
    {
        if (Target != null)
        {
            //ActiveArrow
            Arrow.SetActive(true);
            ArrowBody.SetActive(true);

            Vector3 dir = Target.position - This.transform.position;
            dir.y = 0f;

            if (dir.sqrMagnitude < 0.0001f) return;

            Quaternion targetRot = Quaternion.LookRotation(dir, Vector3.up);

            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, TurnSpeed * Time.deltaTime);
        }
        else
        {
            Arrow.SetActive(false);
            ArrowBody.SetActive(false);
        }
        
     
    }
    public void SetBaseLocation()
    {
        Target = BaseLocation;
    }
}
