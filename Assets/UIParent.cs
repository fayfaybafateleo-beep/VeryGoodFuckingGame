using UnityEngine;

public class UIParent : MonoBehaviour
{
    public GameObject Player;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Awake()
    {
        Player = GameObject.FindGameObjectWithTag("Player");
    }

    void Start()
    {
       
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
