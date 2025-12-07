using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class BrutalSystem : MonoBehaviour
{
    [Header("BrutalData")]
    public int MaxBrutal = 999;
    public int CurrentBrutal = 0;

    public Image BrutalBar;
    public TextMeshProUGUI ScoreText;

    [Header("Skill Settings")]
    public int BrutalCost = 100;        
    public int BrutalNeed = 100;        
    public float SupplyCooldown = 3f;     
    public int DropsCount = 3;
    public GameObject Player;

    [Header("Skill UI")]
    public Image SupplyIcon;              
    public Color ReadyColor = Color.green; 
    private Color OriginalColor;    


    public GameObject AmmoDrops1;
    public GameObject AmmoDrops2;

    private float SupplyCooldownTimer = 0f;

    void Start()
    {
        MaxBrutal = 999;
        Player = GameObject.FindGameObjectWithTag("Player");
    }

    void Update()
    {
        if (CurrentBrutal > MaxBrutal)
        {
            CurrentBrutal = MaxBrutal;
        }

        UpdateSupplyIconColor();

        BrutalBar.fillAmount = (float)CurrentBrutal / MaxBrutal;
        ScoreText.text = CurrentBrutal.ToString();

        if (SupplyCooldownTimer > 0f)
        {
            SupplyCooldownTimer -= Time.deltaTime;
        }

        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            Supply();
        }
    }

    public void Supply()
    {
        if (SupplyCooldownTimer > 0f)
        {
            return;
        }

        if (CurrentBrutal < BrutalNeed || CurrentBrutal < BrutalCost)
        {
            return;
        }

        SpawnDrops();

        CurrentBrutal -= BrutalCost;
        if (CurrentBrutal < 0) CurrentBrutal = 0;

        SupplyCooldownTimer = SupplyCooldown;
    }

    void SpawnDrops()
    {
        
        for (int i = 0; i < DropsCount; i++)
        {
            Vector3 offset = new Vector3(
                Random.Range(-0.5f, 0.5f),
                0f,
                Random.Range(-0.5f, 0.5f)
            );
            Vector3 spawnPos = Player.transform.position + offset;

            GameObject prefabToSpawn = (Random.value < 0.5f) ? AmmoDrops1 : AmmoDrops2;

            if (prefabToSpawn != null)
            {
                Instantiate(prefabToSpawn, spawnPos, Quaternion.identity);
            }
        }
    }

    void UpdateSupplyIconColor()
    {
        if (SupplyIcon == null) return;

        bool canUseSupply = SupplyCooldownTimer <= 0f && CurrentBrutal >= BrutalCost && CurrentBrutal >= BrutalNeed;

        SupplyIcon.color = canUseSupply ? ReadyColor : OriginalColor;
    }

}
