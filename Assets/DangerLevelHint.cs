using TMPro;
using UnityEngine;

public class DangerLevelHint : MonoBehaviour
{
    [Header("Text")]
    public TextMeshPro Text;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Text.text = "Danger Level: " + DangerLevel.GameDangerLevel.ToString();
    }
}
