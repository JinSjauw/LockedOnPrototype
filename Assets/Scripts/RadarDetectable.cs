using UnityEngine;

public class RadarDetectable : MonoBehaviour
{
    
    [SerializeField] private Transform hologramPrefab;
    [SerializeField] private Transform anchorPoint;
    
    private ObjectPool pool;

    private void Awake()
    {
        pool = FindFirstObjectByType<ObjectPool>();
    }

    //Spawn box take orientation of the boddy into account

    public void SpawnHologram(string enemyID) 
    {
        GameObject hologramObject = pool.GetObject(hologramPrefab.gameObject);
        hologramObject.transform.position = anchorPoint.position;
        hologramObject.transform.rotation = anchorPoint.rotation;

        if(hologramObject.TryGetComponent(out RadarHologram radarHologram)) 
        {
            radarHologram.SetID(enemyID);
        }
    } 
}
