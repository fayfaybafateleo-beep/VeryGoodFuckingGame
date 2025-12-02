using UnityEngine;
using UnityEngine.UI;

public class HealthUI : MonoBehaviour
{
    public PlayerHealth PH;
    public Image HealthBar;
    public Image ShieldBar;
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
    }
}
