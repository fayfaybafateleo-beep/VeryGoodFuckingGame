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
    // skill1
    public int BrutalCost1 = 100;
    public int BrutalNeed1 = 100;

    // skill2
    public int BrutalCost2 = 150;
    public int BrutalNeed2 = 150;

    // skill3
    public int BrutalCost3 = 200;
    public int BrutalNeed3 = 200;

    public float SupplyCooldown = 3f;   

    public int DropsCount = 3;
    public int DropsCount2 = 8;
    public GameObject Player;

    [Header("Skill UI")]
    public Image SkillIcon1;
    public Image SkillIcon2;
    public Image SkillIcon3;

    public Color ReadyColor = Color.green;
    private Color OriginalColor1;
    private Color OriginalColor2;
    private Color OriginalColor3;

    [Header("Drops")]
    public GameObject AmmoDrops1;
    public GameObject AmmoDrops2;

    public GameObject HealthDrops;
    public GameObject AurmorDrops;

    private float SupplyCooldownTimer = 0f;

    [Header("BuffManager")]
    public BuffManager BM;

    void Start()
    {
        MaxBrutal = 999;

        Player = GameObject.FindGameObjectWithTag("Player");

        BM = GameObject.FindGameObjectWithTag("BuffManager").GetComponent<BuffManager>();

        if (SkillIcon1 != null) OriginalColor1 = SkillIcon1.color;
        if (SkillIcon2 != null) OriginalColor2 = SkillIcon2.color;
        if (SkillIcon3 != null) OriginalColor3 = SkillIcon3.color;
    }

    void Update()
    {
        if (CurrentBrutal > MaxBrutal)
        {
            CurrentBrutal = MaxBrutal;
        }

        BrutalBar.fillAmount = (float)CurrentBrutal / MaxBrutal;
        ScoreText.text = CurrentBrutal.ToString();

        if (SupplyCooldownTimer > 0f)
        {
            SupplyCooldownTimer -= Time.deltaTime;
        }

        UpdateSkillIconsColor();

        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            TryCastSkill(BrutalCost1, BrutalNeed1, () => Skill1Effect(AmmoDrops1, AmmoDrops2));
        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            TryCastSkill(BrutalCost2, BrutalNeed2, () => Skill2Effect(HealthDrops, AurmorDrops));
        }
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            TryCastSkill(BrutalCost3, BrutalNeed3, Skill3Effect);
        }
    }

    void TryCastSkill(int cost, int need, System.Action skillAction)
    {
        if (SupplyCooldownTimer > 0f)
        {
            return;
        }
        if (CurrentBrutal < need || CurrentBrutal < cost)
        {
            return;
        }
            

        skillAction?.Invoke();

        CurrentBrutal -= cost;
        if (CurrentBrutal < 0) CurrentBrutal = 0;

        SupplyCooldownTimer = SupplyCooldown;
    }

    // SkillPerformance

    void Skill1Effect(GameObject drop1, GameObject drop2)
    {
        SpawnDrops(drop1,drop2,DropsCount);
    }

    void Skill2Effect(GameObject drop1, GameObject drop2)
    {
        SpawnDrops(drop1, drop2, DropsCount2);
    }

    void Skill3Effect()
    {

        BM.StartDamageIncrease();
    }


    void SpawnDrops(GameObject drop1, GameObject drop2,int dropCount)
    {
        if (Player == null) return;

        for (int i = 0; i < dropCount; i++)
        {
            Vector3 offset = new Vector3(
                Random.Range(-0.5f, 0.5f),
                0f,
                Random.Range(-0.5f, 0.5f)
            );
            Vector3 spawnPos = Player.transform.position + offset;

            GameObject prefabToSpawn = (Random.value < 0.5f) ?drop1 : drop2;

            if (prefabToSpawn != null)
            {
                Instantiate(prefabToSpawn, spawnPos, Quaternion.identity);
            }
        }
    }


    void UpdateSkillIconsColor()
    {
        bool canSkill1 = SupplyCooldownTimer <= 0f && CurrentBrutal >= BrutalCost1 && CurrentBrutal >= BrutalNeed1;
        bool canSkill2 = SupplyCooldownTimer <= 0f && CurrentBrutal >= BrutalCost2 && CurrentBrutal >= BrutalNeed2;
        bool canSkill3 = SupplyCooldownTimer <= 0f && CurrentBrutal >= BrutalCost3 && CurrentBrutal >= BrutalNeed3;

        if (SkillIcon1 != null)SkillIcon1.color = canSkill1 ? ReadyColor : OriginalColor1;

        if (SkillIcon2 != null)SkillIcon2.color = canSkill2 ? ReadyColor : OriginalColor2;

        if (SkillIcon3 != null) SkillIcon3.color = canSkill3 ? ReadyColor : OriginalColor3;
    }

}
