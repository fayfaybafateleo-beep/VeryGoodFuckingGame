using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BrutalSystem : MonoBehaviour
{
    [Header("BrutalData")]
    public int MaxBrutal=999;
    public int CurrentBrutal=0;

    public Image BrutalBar;
    public TextMeshProUGUI ScoreText;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        MaxBrutal = 999;
    }

    // Update is called once per frame
    void Update()
    {
        if (CurrentBrutal > MaxBrutal)
        {
            CurrentBrutal = MaxBrutal;
        }

        BrutalBar.fillAmount = (float)CurrentBrutal / MaxBrutal;
        ScoreText.text = CurrentBrutal.ToString();

    }
}
