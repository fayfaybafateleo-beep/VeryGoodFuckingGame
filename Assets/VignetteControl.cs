using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class VignetteControl : MonoBehaviour
{
    [Header("Volume")]
    public Volume Volume;
    private Vignette vignette;

    [Header("Hit Settings")]
    public float HitIntensity = 0.5f;   
    public float RecoverSpeed = 2f; 

    [Header("Colors")]
    public Color DefaultColor;
    private float DefaultIntensity;
    public Color ShieldHitColor = Color.cyan;
    public Color HealthHitColor = Color.red;

    private float CurrentIntensity;
    private float TargetIntensity;
    private Color TargetColor;

    [Header("Last Dance Settings")]
    public bool LastDanceActive = false;         
    public float LastDanceMinIntensity = 0.4f;    
    public float LastDanceMaxIntensity = 0.9f;    
    public float LastDanceSpeed = 2f;             
    public Color LastDanceColor = Color.red;      

    private float LastDanceTime = 0f;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Vignette vignette_temp;
        if (Volume.profile.TryGet<Vignette>(out vignette_temp))
        {
            vignette = vignette_temp;

        }
        DefaultIntensity = vignette.intensity.value;
            DefaultColor = vignette.color.value;

            CurrentIntensity = DefaultIntensity;
            TargetIntensity = DefaultIntensity;
            TargetColor = DefaultColor;

     
    }

    // Update is called once per frame
    void Update()
    {
        
        Vignette vignette_temp;
        if (Volume.profile.TryGet<Vignette>(out vignette_temp))
        {
            vignette = vignette_temp;

        }

        if (LastDanceActive)
        {
            LastDanceTime += Time.deltaTime * LastDanceSpeed;

            float t = (Mathf.Sin(LastDanceTime) + 1f) * 0.5f;

            float pulseIntensity = Mathf.Lerp(LastDanceMinIntensity, LastDanceMaxIntensity, t);
            vignette.intensity.value = pulseIntensity;

            var color = Color.Lerp(DefaultColor, LastDanceColor, t);
            vignette.color.value = color;

            return; 
        }

        //Smooth color/intensity change
        CurrentIntensity = Mathf.MoveTowards(CurrentIntensity,TargetIntensity,RecoverSpeed * Time.deltaTime );
           vignette.intensity.value = CurrentIntensity;
           vignette.color.value = Color.Lerp(vignette.color.value,TargetColor,RecoverSpeed * Time.deltaTime);
    }

    public void OnShieldHit()
    {
        if (LastDanceActive) return;

        CurrentIntensity = HitIntensity;
        vignette.intensity.value = HitIntensity;

        vignette.color.value = ShieldHitColor;
        TargetIntensity = DefaultIntensity;
        TargetColor = DefaultColor;   
    }

    public void OnHealthHit()
    {
        if (LastDanceActive) return;

        CurrentIntensity = HitIntensity;
        vignette.intensity.value = HitIntensity;

        vignette.color.value = HealthHitColor;
        TargetIntensity = DefaultIntensity;
        TargetColor = DefaultColor;  
    }

    public void StartLastDance()
    {
        LastDanceActive = true;
        LastDanceTime = 0f;
    }

    public void StopLastDance()
    {
        LastDanceActive = false;

        TargetIntensity = DefaultIntensity;
        TargetColor = DefaultColor;
        CurrentIntensity = vignette.intensity.value;
        vignette.color.value = DefaultColor;
    }
}
