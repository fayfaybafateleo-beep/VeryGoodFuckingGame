using UnityEngine;

public class DangerLevel : MonoBehaviour
{
    [Header("DangerLevel")]
    public static int GameDangerLevel;

 
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        GameDangerLevel = 10;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
