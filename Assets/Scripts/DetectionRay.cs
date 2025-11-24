using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DetectionRay : MonoBehaviour
{
    [Header("RayCastInfo")]
    public float MaxDistance = 100f;
    public float Dist;
    public LayerMask EnemyLayer;
    public Camera Cam;
    public float AimRadius = 0.3f;   // 可以在 Inspector 里调，比如 0.2 ~ 0.5 试试

    [Header("Execution")]
    public GameObject ExecutionText;
    public float ExecutionDis;

    [Header("Shaker")]
    public float Intensity = 3f;
    public float Duration = 0.2f;

    private Vector3 OriginalPos;
    private float Timer;

    [Header("Fade Settings")]
    public float FadeDuration = 0.5f;
    private float FadeTimer;
    public CanvasGroup CG;

    [Header("EnemyHealthSystem")]
    public GameObject EnemyDetailGroup;
    public GameObject EnemyExecuteHint;
    public Image EnemyHealthBar;
    public TextMeshProUGUI ThoughnessAmount;
    public TextMeshProUGUI ThoughnessCompare;
    public TextMeshProUGUI EnemyName;

    [Header("WeaponData")]
    public MagazineCounterUI MGUI;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        OriginalPos = ExecutionText.transform.localPosition;
    }

    // Update is called once per frame
    void Update()
    {
        DetectEnemy();
        //ExecutionTextShake
        float caculate= ((1 - (Dist / 10)) * 2);
        float caculate2 = (1 - 2*(Dist / 10));
        float intensty = Intensity * caculate;
    
        Timer -= Time.deltaTime;
        ExecutionText.transform.localScale = new Vector3(0.2f + caculate2, 0.2f + caculate2, 0.2f + caculate2);
            //Shanking
            ExecutionText.transform.localPosition = OriginalPos + new Vector3(
                Random.Range(-Intensity, Intensity),
                Random.Range(-Intensity, Intensity),
                0f
            );

        //Fadeout
        if (ExecutionText.activeSelf)
        {
            if (FadeTimer > 0)
            {
                FadeTimer -= Time.deltaTime;
                CG.alpha = 1f;  
            }
            else
            {
                CG.alpha -= Time.deltaTime / FadeDuration;

                if (CG.alpha <= 0f)
                {
                    CG.alpha = 0f;
                    ExecutionText.SetActive(false);
                    ExecutionText.transform.localPosition = OriginalPos;  
                }
            }
        }

    }

    void DetectEnemy()
    {
 
        Ray ray = Cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        Debug.DrawRay(ray.origin, ray.direction * MaxDistance, Color.red, 1f);

        if (Physics.SphereCast(ray, AimRadius, out RaycastHit hit, MaxDistance, EnemyLayer))
        {
            EnemyHealth eh = hit.collider.GetComponentInParent<EnemyHealth>();
            EnemyBehaviour eb  = hit.collider.GetComponentInParent<EnemyBehaviour>();
            if (eh != null)
            {
                EnemyDetailGroup.SetActive(true);

                EnemyHealthBar.fillAmount = eh.Health / eh.MaxHealth;
                ThoughnessAmount.text = eh.Thougthness.ToString();

                EnemyName.text = eb.Name;

                if (eb.ES == EnemyBehaviour.EnemyState.Shock)
                {
                    EnemyExecuteHint.SetActive(true);
                }
                else
                {
                    EnemyExecuteHint.SetActive(false);
                }

                if (MGUI.CurrentAP > eh.Thougthness)
                {
                    ThoughnessCompare.text = "<";
                    ThoughnessCompare.color= Color.green;
                }
                if (MGUI.CurrentAP < eh.Thougthness)
                {
                    ThoughnessCompare.text = ">";
                    ThoughnessCompare.color = Color.red;
                }
                if (MGUI.CurrentAP == eh.Thougthness)
                {
                    ThoughnessCompare.text = "|";
                    ThoughnessCompare.color = Color.yellow;
                }
            }
            if(eh == null)
            {
                EnemyDetailGroup.SetActive(false);
            }
            if(eb!=null && eb.ES == EnemyBehaviour.EnemyState.Shock)
            {
                Dist = Vector3.Distance(ray.origin, hit.point);

                if (Dist <= ExecutionDis)
                {
                    ExecutionText.SetActive(true);
                    FadeTimer = FadeDuration;  // 重置淡出计时器，让它保持显示
                }
                else
                {
                    ExecutionText.transform.localPosition = OriginalPos;
                }


            }
            else
            {
                ExecutionText.transform.localPosition = OriginalPos;
            }

        }
        else
        {
            ExecutionText.transform.localPosition = OriginalPos;
        }
    }
}
