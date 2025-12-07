using StarterAssets;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UIParent : MonoBehaviour
{
    public GameObject Player;
    public PlayerHealth PH;
    public StarterAssetsInputs StarterInputs;

    [Header("DeadSound")]
    public AudioSource AudioSource;
    public AudioClip Diesound;
    bool OneShot;
    bool unlockedOnDeath;   // 确保只在死亡第一次时解锁一次

    private void Awake()
    {
        Player = GameObject.FindGameObjectWithTag("Player");
        PH = Player.GetComponent<PlayerHealth>();
        StarterInputs = Player.GetComponent<StarterAssetsInputs>();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            StarterInputs.UnlockCursor();
        }
        if (Input.GetKeyDown(KeyCode.J))
        {
            StarterInputs.LockCursor();
        }
        if (PH == null) return;

        if (PH.PS == PlayerHealth.PlayerState.Die && !unlockedOnDeath)
        {
            if (StarterInputs != null)
            {
                StarterInputs.UnlockCursor();
            }

            PlayDeadSound();
            unlockedOnDeath = true;
        }
    }

    public void ResetScene()
    {
        Scene current = SceneManager.GetActiveScene();
        SceneManager.LoadScene(current.name);
    }

    public void PlayDeadSound()
    {
        if (OneShot) return;
        if (AudioSource != null && Diesound != null)
        {
            AudioSource.PlayOneShot(Diesound);
        }
        OneShot = true;
    }
}
