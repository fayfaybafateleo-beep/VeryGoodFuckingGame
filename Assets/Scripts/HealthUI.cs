using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HealthUI : MonoBehaviour
{
    public PlayerHealth PH;
    public Image HealthBar;
    public Image ShieldBar;
    public GameObject BrokenBar;
    public TextMeshProUGUI HealthCount;
    public TextMeshProUGUI ShieldCount;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        PH = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerHealth>();
    }

    // Update is called once per frame
    void Update()
    {
        HealthBar.fillAmount = (float)PH.CurrentHealth / (float)PH.MaxHealth;
        ShieldBar.fillAmount = (float)PH.CurrentArmuor / (float)PH.MaxArmuor;
        ShieldCount.text = PH.CurrentArmuor.ToString();
        HealthCount.text = PH.CurrentHealth.ToString();

        if (PH.IsShieldBroke)
        {
            BrokenBar.SetActive(true);
        }
        else
        {
            BrokenBar.SetActive(false);
        }
    }
}
