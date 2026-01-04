using UnityEngine;
using UnityEngine.SceneManagement;

public class SwapScene : MonoBehaviour
{
    [Header("SceneCode")]
    public int SceneNum;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
   public void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Player")
        {
            SceneChange();
        }
    }
    public void SceneChange()
    {
        SceneManager.LoadScene(SceneNum);
    }
}
