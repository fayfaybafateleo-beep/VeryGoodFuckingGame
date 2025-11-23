using UnityEngine;

public class SectorDoor : MonoBehaviour
{
    [Header("Blocker")]
    public GameObject Blocker;

    [Header("Section")]
    public SectionManager SectionManager;                    
    public SectionManager.SectionType RequiredSection;       

    [Header("Player")]
    public Transform Player;                                 
    public float OpenDistance = 5f;

    [Header("Animation")]
    public Animator GateAnimator;

    [Header("ExitTrigger")]
    public LevelTriggers LT;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        SectionManager = SectionManager.Instance;
        Player = GameObject.FindGameObjectWithTag("Player").GetComponent<Transform>();
    }

    // Update is called once per frame
    void Update()
    {
        if (!SectionManager.IsDone(RequiredSection))
            return;
        float dist = Vector3.Distance(Player.position, transform.position);
        if (dist <= OpenDistance)
        {
            GateAnimator.SetTrigger("Open");
        }

        if (LT.IsTriggered)
        {
            GateAnimator.SetTrigger("Close");
        }
    }
    public void Blocking()
    {
        Blocker.SetActive(true);
    }
    public void UnBlocking()
    {
        Blocker.SetActive(false);
    }
    public void ResetGate()
    {
        foreach (var p in GateAnimator.parameters)
        {
            if (p.type == AnimatorControllerParameterType.Trigger)
            {
                GateAnimator.ResetTrigger(p.name);
            }
        }

        GateAnimator.SetTrigger("Reset");
    }
}
