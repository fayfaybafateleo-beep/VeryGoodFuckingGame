using UnityEngine;

public class MeshSync : MonoBehaviour
{
    public MeshFilter SourceMeshFilter;
    private MeshFilter TargetMeshFilter;

    public EnemyBehaviour EB;

    [Header("SyncTransform")]
    public bool WantSync;
    public GameObject SyncSource;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        TargetMeshFilter = GetComponent<MeshFilter>();
    }

    // Update is called once per frame
    void Update()
    {
        if (SourceMeshFilter == null || EB.ES==EnemyBehaviour.EnemyState.Die)
        {
            Destroy(gameObject, 0);
        }
        else
        {
            TargetMeshFilter.sharedMesh = SourceMeshFilter.sharedMesh;
        }
        if (WantSync)
        {
            if(SourceMeshFilter != null)
            {
                this.transform.position = SyncSource.transform.position;
                this.transform.rotation = SyncSource.transform.rotation;
            }
        }
    }
}
