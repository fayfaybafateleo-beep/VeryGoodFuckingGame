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
            TextBoject.transform.localPosition = OriginalPos + new Vector3(
                Random.Range(-Intensity, Intensity),
                Random.Range(-Intensity, Intensity),
                0f
            );
        }
        else
        {
            // 抖完回到原位
            TextBoject.transform.localPosition = OriginalPos;
        }
    }
    public void SetName (string what,int score, float size, Vector3 color)
    {
        WhatText.text = what + " +"+score.ToString() + " Brutal";
        this.transform.localScale = new Vector3(size, size, size);
        WhatText.color = new Color(color.x, color.y, color.z, 1f);
    }
}
