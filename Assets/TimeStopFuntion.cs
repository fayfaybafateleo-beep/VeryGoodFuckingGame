using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using System.Collections;
public class TimeStopFuntion : MonoBehaviour
{
    [Header("TimeSlow")]
    public float TimeSlowRATE = 0.2f;
    public float TimeSlowTIme = 0.3f;
    public bool IsTimeSlow;

    [Header("Lens Distortion")]
    public Volume PostVolume;                 
    public float DistortPeak = -0.35f;       
    public float DistortInTime = 0.06f;      
    public float DistortOutTime = 0.12f;      
    public float DistortScalePeak = 1.02f;    

    private LensDistortion lens;
    private Coroutine DistortCoroutine;

    private Coroutine TimeSlowCoroutine;

    [Header("Pause")]
    public bool IsPaused;                 
    public float SavedTimeScale = 1f;    

    void Start()
    {
        PostVolume = GameObject.FindGameObjectWithTag("GlobalVolume").GetComponent<Volume>();
        if (PostVolume != null && PostVolume.profile != null)
        {
            PostVolume.profile.TryGet(out lens);
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return))
        {
            UsePause();
        }
    }

    public void UsePause()
   {
        if (IsPaused==false)
        {
            // Pause
            SavedTimeScale = Time.timeScale;
            Time.timeScale = 0f;
            IsPaused = true;
        }
        else
        {
            // UnPause
            Time.timeScale = SavedTimeScale;
            IsPaused = false;
        }
    }

    public void PlayTimeSlow()
    {
        if (IsTimeSlow==false)
        {
            if (TimeSlowCoroutine != null)
            {
                StopCoroutine(TimeSlowCoroutine);
            }
            TimeSlowCoroutine = StartCoroutine(TimeSlowRoutine());
        }

        if (DistortCoroutine != null)
        {
           StopCoroutine(DistortCoroutine);
        }
        
        DistortCoroutine = StartCoroutine(LensDistortPunch());
    }

    IEnumerator TimeSlowRoutine()
    {
        IsTimeSlow = true;

        float originalScale = Time.timeScale;
        Time.timeScale = TimeSlowRATE;

        float timer = 0f;
        while (timer < TimeSlowTIme)
        {
            timer += Time.deltaTime;
            yield return null;
        }

        Time.timeScale = originalScale;
        IsTimeSlow = false;
    }


    IEnumerator LensDistortPunch()
    {
        float startIntensity = lens.intensity.value;
        float startScale = lens.scale.value;

      
        float t = 0f;
        // Start
        while (t < DistortInTime)
        {
            t += Time.unscaledDeltaTime; 
            float k = (DistortInTime <= 0f) ? 1f : Mathf.Clamp01(t / DistortInTime);

            lens.intensity.Override(Mathf.Lerp(startIntensity, DistortPeak, k));
            lens.scale.Override(Mathf.Lerp(startScale, DistortScalePeak, k));
            yield return null;
        }

        // Back
        t = 0f;
        while (t < DistortOutTime)
        {
            t += Time.unscaledDeltaTime;
            float k = (DistortOutTime <= 0f) ? 1f : Mathf.Clamp01(t / DistortOutTime);

            lens.intensity.Override(Mathf.Lerp(DistortPeak, 0f, k));
            lens.scale.Override(Mathf.Lerp(DistortScalePeak, 1f, k));
            yield return null;
        }

        lens.intensity.Override(0f);
        lens.scale.Override(1f);
    }

    public void CancelAllEffects()
    {
        if (TimeSlowCoroutine != null) StopCoroutine(TimeSlowCoroutine);
        if (DistortCoroutine != null) StopCoroutine(DistortCoroutine);

        IsTimeSlow = false;
        Time.timeScale = 1f;

        if (lens != null)
        {
            lens.intensity.Override(0f);
            lens.scale.Override(1f);
        }
    }

}
