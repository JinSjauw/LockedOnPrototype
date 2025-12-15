using UnityEngine;

public class SonarDetectable : MonoBehaviour
{
    
    [SerializeField] private Transform hologramPrefab;
    [SerializeField] private Transform anchorPoint;
    
    private ObjectPool pool;

    private void Awake()
    {
        pool = FindFirstObjectByType<ObjectPool>();
    }

    //Spawn box take orientation of the boddy into account

    public void SpawnHologram(string enemyID, Vector3 position) 
    {
        GameObject hologramObject = pool.GetObject(hologramPrefab.gameObject);
        hologramObject.transform.position = position;
        //hologramObject.transform.rotation = anchor.rotation;

        if(hologramObject.TryGetComponent(out RadarHologram radarHologram)) 
        {
            radarHologram.SetID(enemyID);
        }
    } 
}
