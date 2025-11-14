using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using Random = UnityEngine.Random;

public class KillCount : MonoBehaviour
{
    [Header("UIStuff")]
    public TextMeshProUGUI KillCountText;
    public GameObject TextObject;
    public GameObject CDBarL;
    public GameObject CDBarR;
    public Image CDBarLImage;
    public Image CDBarRImage;

    [Header("Shaker")]
    public float Intensity = 3f;
    public float Duration = 0.2f;   // 抖动持续时间

    private Vector3 OriginalPos;
    private float ShakeTimer = 0f;  

    [Header("CD")]
    public float CountDownTime=1.5f;
    public float CountDownTimer;
    public float CountDownTimeFinal;
    public float CountDownMultiPulier;

    [Header("Data")]
    public int KillCounter;


    Color Color;
    public static KillCount Instance;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Instance = this;
        OriginalPos = TextObject.transform.localPosition;
    }

    // Update is called once per frame
    void Update()
    {

        if (KillCounter <= 0)
        {
            TextObject.SetActive(false);

        }
        else
        {
            TextObject.SetActive(true);
            CountDownTimer -= Time.deltaTime;
            KillCountText.text = "X " + KillCounter.ToString();
        }
        if (CountDownTimer <= 0 )
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

        //KillCountTextColor
        if (KillCounter >= 30)
        {
            Color = new Color(1f, 0.84f, 0f, 0.7f);
        }
        else if (KillCounter >= 20)
        {
            Color = new Color(1f, 0f, 0f, 0.7f);
        }
        else if (KillCounter >= 10)
        {
            Color = new Color(0f, 0.8980392f, 0.3076688f, 0.7f);
        }
        else
        {
            Color = new Color(1f, 1f, 1f, 0.7f);
        }

        KillCountText.color = Color;

        //FillBar
        CDBarLImage.fillAmount = CountDownTimer / CountDownTime;
        CDBarRImage.fillAmount = CountDownTimer / CountDownTime;
        //ShakeFunction
        if (ShakeTimer > 0f)
        {
            float intensity=Intensity* CountDownMultiPulier* CountDownMultiPulier;
            if (intensity >= 10) intensity = 10;
            ShakeTimer -= Time.deltaTime;

            TextObject.transform.localPosition = OriginalPos + new Vector3(
                Random.Range(-intensity, intensity),
                Random.Range(-intensity, intensity),
                0f
            );
        }
        else
        {
            TextObject.transform.localPosition = OriginalPos;
        }

    }
    public void AddKill()
    {
        KillCounter += 1;

        CountDownMultiPulier = 1f + (KillCounter / 10f) * 0.1f;

        CountDownTimeFinal = CountDownTime * CountDownMultiPulier;

        CountDownTimer = CountDownTimeFinal;
        AddShake();

    }
    public void AddShake()
    {
        ShakeTimer = Duration;
    }
}
