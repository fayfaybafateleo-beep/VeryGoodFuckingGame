using UnityEngine;

public class LevelTriggers : MonoBehaviour
{
    [Header("StateOfTrigger")]
    public bool IsEnd;
    public bool IsTriggered;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Player")
        {
            IsTriggered = true;
        }
    }
    public void ResetTrigger()
    {
        IsTriggered = false;
    }
}
