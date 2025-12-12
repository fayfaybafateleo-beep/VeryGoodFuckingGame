using UnityEngine;
using TMPro;

public class MoneyUi : MonoBehaviour
{
    public PlayerData PD;
    public TextMeshProUGUI Text;
    public UIParent UP;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (PD == null)
        {
            PD = UP.Player.GetComponent<PlayerData>();
        }
       
        Text.text = PD.Coins.ToString ()+ " $";
    }
}
