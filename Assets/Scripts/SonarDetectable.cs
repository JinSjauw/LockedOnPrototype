using UnityEngine;
using UnityEngine.AI;

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

        if(trackerObject.TryGetComponent(out SonarMark sonarMark)) 
        {
            sonarMark.SetID(enemyID);

            NavMeshAgent navMeshAgent = GetComponentInChildren<NavMeshAgent>();
            if (navMeshAgent != null) 
            {
                sonarMark.SetDirectionHint(navMeshAgent.desiredVelocity);
            }
        }
    } 
}
