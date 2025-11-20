using UnityEngine;

public class MeshSync : MonoBehaviour
{
    public MeshFilter SourceMeshFilter;
    private MeshFilter TargetMeshFilter;

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
        if (SourceMeshFilter == null)
        {
            Destroy(gameObject, 0);
        }
        else
        {
            TargetMeshFilter.sharedMesh = SourceMeshFilter.sharedMesh;
        }
        if (WantSync)
        {
            this.transform.position = SyncSource.transform.position;
            this.transform.rotation = SyncSource.transform.rotation;
        }
    }
}
