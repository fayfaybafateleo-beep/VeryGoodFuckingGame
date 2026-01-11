using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class KillListing : MonoBehaviour
{
    public  TextMeshProUGUI WhatText;
    public GameObject TextBoject;
    [Header("Shaker")]
    public float Intensity = 3f;   
    public float Duration = 0.2f;  

    private Vector3 OriginalPos;
    private float Timer;

    public Animator Animator;
  
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Destroy(gameObject, 3f);

      

        OriginalPos = TextBoject.transform.localPosition;
        Timer = Duration;
    }

    // Update is called once per frame
    void Update()
    {
        if (Timer > 0f)
        {
            Timer -= Time.deltaTime;
            //Shanking
            TextBoject.transform.localPosition = OriginalPos + new Vector3( Random.Range(-Intensity, Intensity), Random.Range(-Intensity, Intensity),  0f );
        }
        else
        {
            //Recover
            TextBoject.transform.localPosition = OriginalPos;
        }

        if(transform.GetSiblingIndex() != 0)
        {
            Animator.SetBool("End", true);
        }

        if (!IsInsideScreen())
        {
            Destroy(gameObject);
        }
    }
    public void SetName (string what,int score, float size, Vector3 color)
    {
        WhatText.text = what + " +"+score.ToString() + " Brutal";
       
        this.transform.localScale = new Vector3(size, size, size);
        WhatText.color = new Color(color.x, color.y, color.z, 1f);
    }

    bool IsInsideScreen()
    {
        //OutOfScreenDetedtion
        RectTransform rect = GetComponent<RectTransform>();

        Vector3[] worldCorners = new Vector3[4];
        rect.GetWorldCorners(worldCorners);

        
        float width = Screen.width;
        float height = Screen.height;

        for (int i = 0; i < 4; i++)
        {
            Vector3 pos = RectTransformUtility.WorldToScreenPoint(null, worldCorners[i]);

            if (pos.x >= 0 && pos.x <= width && pos.y >= 0 && pos.y <= height)
            {
                return true;   
            }
        }

        return false;
    }
}
