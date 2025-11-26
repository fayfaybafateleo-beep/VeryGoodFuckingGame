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

    [Header("ExitGate")]
    public bool IsExit;
    public GameLevelControl CLC;

    [Header("StartGate")]
    public bool IsStart;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        SectionManager = SectionManager.Instance;
        Player = GameObject.FindGameObjectWithTag("Player").GetComponent<Transform>();
    }

    // Update is called once per frame
    void Update()
    {
        float dist = Vector3.Distance(Player.position, transform.position);
        //StartGate
        if(IsStart && CLC.LS == GameLevelControl.LevelState.NotActive && CLC.IsSeleted)
        {
            GateAnimator.SetTrigger("Open");
            GateAnimator.Play("Open");
        }
        if(IsStart && CLC.LS != GameLevelControl.LevelState.NotActive || IsStart&& CLC.IsSeleted==false)
        {
            GateAnimator.SetTrigger("Close");
        }
        //EndGate

        if (IsExit && CLC.LS==GameLevelControl.LevelState.End)
        {
            if (dist <= OpenDistance)
            {
                GateAnimator.SetTrigger("Open");
            }

            if (LT.IsTriggered)
            {
                GateAnimator.SetTrigger("Close");
            }
        }
        //SectorGate

       if (!SectionManager.IsDone(RequiredSection) || IsExit || IsStart )
            return;
       if( LT != null)
       {
            if (dist <= OpenDistance)
            {
                GateAnimator.SetTrigger("Open");
            }

            if (LT.IsTriggered)
            {
                GateAnimator.SetTrigger("Close");
            }
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
