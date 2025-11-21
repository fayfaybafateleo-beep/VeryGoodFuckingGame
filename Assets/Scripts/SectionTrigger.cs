using UnityEngine;

public class SectionTrigger : MonoBehaviour
{
    [Header("SectionPart")]
    public SectionManager.SectionType Section;

    [Header("Triggers")]
    public bool IsStartTrigger = true;
    public bool TriggerOnce = true;
    private bool HasTriggered = false;

    [Header("SectionManager")]
    public SectionManager SectionManager;


    private void OnTriggerEnter(Collider other)
    {
        if (HasTriggered && TriggerOnce ) return;
        if(other.gameObject.tag!="Player")return;
        if (IsStartTrigger)
        {
            SectionManager.SetActive(Section, true);
            SectionManager.SetDone(Section, false);
        }
        else
        {
            SectionManager.SetActive(Section, false);
            SectionManager.SetDone(Section, true);
        }

        HasTriggered = true;
    }
}
