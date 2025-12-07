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
    public bool IsShieldBroke;

    [Header("Invincible")]
    public float HurtInvincibleTime = 1f;
    public float ShieldBreakInvincibleTime = 1.5f;

    public bool IsInvincible = false;
    public float InvincibleTimer = 0f;

    public VignetteControl VC;

    public CinemachineImpulseSource ImpulseSource;

    public Animator DeadCurtainAnimator;


    [Header("Brutal System")]
    public BrutalSystem BS;          
    public float BrutalDrainPerSecond = 50f; 
    public bool IsUsingBrutalToSurvive = false;
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
        DeadCurtainAnimator = GameObject.FindGameObjectWithTag("DeadCurtain").GetComponent<Animator>();

        BS = GameObject.FindGameObjectWithTag("Brutal").GetComponent<BrutalSystem>();
    }

    // Update is called once per frame
    void Update()
    {
        if (CurrentArmuor > MaxArmuor)
        {
            CurrentArmuor = MaxArmuor;
        }

        if (CurrentHealth > MaxHealth)
        {
            CurrentHealth = MaxHealth;
        }

        if (CurrentArmuor <= 0)
        {
            CurrentArmuor = 0;
            IsShieldBroke = true;
        }
        else
        {
            IsShieldBroke = false;
        }

        if (CurrentHealth <= 0)
        {
            HandleDeathOrBrutal();
        }
        else
        {
            if (IsUsingBrutalToSurvive)
            {
                IsUsingBrutalToSurvive = false;
                VC.StopLastDance();
              
            }
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
    public void GainHealth(float multipulier)
    {
        CurrentHealth += Mathf.RoundToInt(5 * multipulier);
    }
    public void GainArmour(float multipulier)
    {
        CurrentArmuor += Mathf.RoundToInt(5 * multipulier);
    }

    void HandleDeathOrBrutal()
    {
        if (PS == PlayerState.Die) return;

        if (IsUsingBrutalToSurvive==false)
        {
            IsUsingBrutalToSurvive = true;
            VC.StartLastDance();
        }
        //BrutalLastDance
        if (BS != null && BS.CurrentBrutal > 0)
        {
            IsUsingBrutalToSurvive = true;

         
            float drain = BrutalDrainPerSecond * Time.deltaTime;
            int drainInt = Mathf.CeilToInt(drain);

            if (drainInt > BS.CurrentBrutal)
                drainInt = BS.CurrentBrutal;

           BS.CurrentBrutal -= drainInt;

            if (BS.CurrentBrutal <= 0 && CurrentHealth <= 0)
            {
                PS = PlayerState.Die;
                VC.StopLastDance();
                DeadCurtainAnimator.SetTrigger("Die");
            }
        }
        else
        {
            
            PS = PlayerState.Die;
            VC.StopLastDance();
            DeadCurtainAnimator.SetTrigger("Die");
        }
    }
}
