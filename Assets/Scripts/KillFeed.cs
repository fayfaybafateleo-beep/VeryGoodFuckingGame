using UnityEngine;
using UnityEngine.UI;

public class KillFeed : MonoBehaviour
{
    public static KillFeed Instance;
    public GameObject KillList;

    public int maxItems = 5;
    public RectTransform RT;

    public BrutalSystem BS;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Instance = this;

        RT.sizeDelta = new Vector2(400, 300);

        BS = GameObject.FindGameObjectWithTag("Brutal").GetComponent<BrutalSystem>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void AddKillLIst(string what, int score,float size,Vector3 color)
    {
        GameObject text = Instantiate(KillList, transform);
        text.transform.SetSiblingIndex(0);
        KillListing ks = text.GetComponent<KillListing>();
        ks.SetName(what, score,size,color);
        BS.CurrentBrutal += score;

        if (score<=10)
        {
            ks.Intensity = 5;
        }
        if(score>10 && score <= 20)
        {
            ks.Intensity = 7;
        }
        if ( score > 20)
        {
            ks.Intensity = 10;
        }
    }
}
