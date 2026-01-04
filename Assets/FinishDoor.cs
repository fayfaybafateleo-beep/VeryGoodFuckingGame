using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class FinishDoor : MonoBehaviour
{
    [Header("InputSettings")]
    public KeyCode Key = KeyCode.B;
    public bool InTrigger;
    public bool Purchased;

    [Header("Player")]
    public PlayerData PD;
    public int CurrentCash;
    public int TargetCash;

    [Header("Text")]
    public TextMeshPro MoneyText;

    [Header("Door")]
    public Animator Animator;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        PD = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerData>();
    }

    // Update is called once per frame
    void Update()
    {
        CurrentCash = PD.Coins;

        if (Input.GetKeyDown(Key) && InTrigger && Purchased==false)
        {
            if (CurrentCash >= TargetCash )
            {
                Animator.SetTrigger("Open");
                PD.Coins -= TargetCash;
                Purchased = true;
                
            }

        }

        MoneyText.text = CurrentCash.ToString() + " / " + TargetCash.ToString();

    
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            InTrigger = true;
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            InTrigger = false;
        }
    }
}
