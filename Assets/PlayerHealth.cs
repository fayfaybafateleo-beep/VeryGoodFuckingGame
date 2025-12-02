using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Rendering;

public class PlayerHealth : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
  

    [Header("Health&Armor")]
    public int MaxHealth;
    public int MaxArmuor;
    public int CurrentHealth;
    public int CurrentArmuor;

    [Header("Invincible")]
    public float HurtInvincibleTime = 1f;
    public float ShieldBreakInvincibleTime = 1.5f;


    public bool IsInvincible = false;
    public float InvincibleTimer = 0f;

    public VignetteControl VC;

    public CinemachineImpulseSource ImpulseSource;

    public enum PlayerState
    {
     Alive,
     Die
    }
    public PlayerState PS;
    void Start()
    {
        CurrentHealth = MaxHealth;
        CurrentArmuor = MaxArmuor;
        PS = PlayerState.Alive;

       VC = GameObject.FindGameObjectWithTag("GlobalVolume").GetComponent<VignetteControl>();
    }

    // Update is called once per frame
    void Update()
    {
        if (CurrentHealth <= 0)
        {
            PS = PlayerState.Die;
        }

        if (IsInvincible)
        {
            InvincibleTimer -= Time.deltaTime;
            if (InvincibleTimer <= 0f)
            {
                IsInvincible = false;
            }
        }


    }
    public void TakeDamage(int damage)
    {
        if (PS == PlayerState.Die || IsInvincible) return;
        if (damage <= 0) return;

        int remainingDamage = damage;

     //Aumor first
        if (CurrentArmuor > 0)
        {
            int armorBefore = CurrentArmuor;
            CurrentArmuor -= remainingDamage;

            
            if (CurrentArmuor <= 0)
            {
                remainingDamage = -CurrentArmuor;
                CurrentArmuor = 0;

                StartInvincible(ShieldBreakInvincibleTime);
                VC.OnShieldHit();
                ImpulseSource.GenerateImpulse(1.2f);

            }
            else
            {
                remainingDamage = 0;
            
                StartInvincible(HurtInvincibleTime);

                  VC.OnShieldHit();
                ImpulseSource.GenerateImpulse();
            }
        }

        if (remainingDamage > 0)
        {
            CurrentHealth -= remainingDamage;
            StartInvincible(HurtInvincibleTime);
            ImpulseSource.GenerateImpulse();
            VC.OnHealthHit();
        }

        //Limitation
        CurrentHealth = Mathf.Clamp(CurrentHealth, 0, MaxHealth);
        CurrentArmuor = Mathf.Clamp(CurrentArmuor, 0, MaxArmuor);
    }

    
    void StartInvincible(float time)
    {
        IsInvincible = true;
        InvincibleTimer = time;
    }
}
