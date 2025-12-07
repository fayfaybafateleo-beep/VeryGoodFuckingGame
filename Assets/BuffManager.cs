using UnityEngine;

public class BuffManager : MonoBehaviour
{
    [Header("DmaageIncrease")]
    public float DamageIncreaseRate;
    public bool IsDamageIncreaseActive;

    public float CountDown;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        CountDown -= Time.deltaTime;
        if (CountDown <= 0)
        {
            CountDown = 0;
            IsDamageIncreaseActive = false;
        }
        if (IsDamageIncreaseActive)
        {
            DamageIncreaseRate = 1;
        }
        else
        {
            DamageIncreaseRate = 0;
        }
    }
    public void StartDamageIncrease()
    {
        CountDown = 10;
        IsDamageIncreaseActive = true;
    }
}
