using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class KillCount : MonoBehaviour
{
    [Header("UIStuff")]
    public TextMeshProUGUI KillCountText;
    public GameObject TextOject;
    public GameObject CDBarL;
    public GameObject CDBarR;
    public Image CDBarLImage;
    public Image CDBarRImage;

    [Header("Shaker")]
    public float Intensity = 3f;
    public float Duration = 0.2f;

    [Header("CD")]
    public float CountDownTime=1.5f;
    public float CountDownTimer;
    public float CountDownMultiPulier;

    [Header("Data")]
    public int KillCounter;
    
    private Vector3 OriginalPos;
    private float Timer;

    public static KillCount Instance;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Instance = this;
    }

    // Update is called once per frame
    void Update()
    {
        float countDownTime = CountDownTime * CountDownMultiPulier;

        CountDownMultiPulier = 1f + (KillCounter / 10) * 0.1f;

        if (KillCounter <= 0)
        {
            TextOject.SetActive(false);
        }
        else
        {
            TextOject.SetActive(true);
            CountDownTimer -= Time.deltaTime;
            KillCountText.text = "X " + KillCounter.ToString();
        }

        if (CountDownTimer <=0)
        {
            KillCounter = 0;
            CountDownTimer = 0;
            CDBarL.SetActive(false);
            CDBarR.SetActive(false);
        }

        else
        {
            CDBarL.SetActive(true);
            CDBarR.SetActive(true);
        }
        //FillBar
        CDBarLImage.fillAmount = CountDownTimer / CountDownTime;
        CDBarRImage.fillAmount = CountDownTimer / CountDownTime;
       
        
    }
    public void AddKill()
    {
        KillCounter += 1;
        CountDownTimer = CountDownTime;
    }
}
