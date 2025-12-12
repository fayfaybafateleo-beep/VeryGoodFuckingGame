using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleShutDown : MonoBehaviour
{
    public List<GameObject> ParticleList;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        HitBoxPart hb = GetComponentInParent<HitBoxPart>();
        if (hb.enabled == false)
        {
            StopLoopAndDestroy();
        }
    }
    public void StopLoopAndDestroy()
    {
        foreach (GameObject obj in ParticleList)
        {
            if (obj == null) continue;

            ParticleSystem ps = obj.GetComponent<ParticleSystem>();
            if (ps == null) continue;

            var main = ps.main;
            main.loop = false;

            ps.Stop(false, ParticleSystemStopBehavior.StopEmitting);

            StartCoroutine(DestroyAfterDone(ps));
        }
    }
    private IEnumerator DestroyAfterDone(ParticleSystem ps)
    {
        while (ps != null && ps.IsAlive(true))
            yield return null;

        if (ps != null)
            Destroy(ps.gameObject);
    }
}
