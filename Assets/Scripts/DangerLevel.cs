using UnityEngine;

public class DangerLevel : MonoBehaviour
{
    [Header("DangerLevel")]
    public static int GameDangerLevel;

    [Header("DeveloperTool")]
    public bool Enable=false;
    public KeyCode Key=KeyCode.L;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        GameDangerLevel = 0;
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(Key)&& Enable)
        {
            GameDangerLevel += 1;
        }
    }
}
