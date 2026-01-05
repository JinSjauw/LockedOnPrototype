using UnityEngine;

public class SonarDetectable : MonoBehaviour
{
    [SerializeField] private Transform trackerPrefab;
    [SerializeField] private Transform anchorPoint;
    
    private ObjectPool pool;

    private void Awake()
    {
        pool = FindFirstObjectByType<ObjectPool>();
    }

    public void SpawnTracker(string enemyID, Vector3 position) 
    {
        GameObject trackerObject = pool.GetObject(trackerPrefab.gameObject);

        trackerObject.transform.position = position;
        //hologramObject.transform.rotation = anchor.rotation;

        if(trackerObject.TryGetComponent(out RadarHologram radarHologram)) 
        {
            radarHologram.SetID(enemyID);
        }
    } 
}
