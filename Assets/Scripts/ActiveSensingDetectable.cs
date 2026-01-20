using UnityEngine;
using UnityEngine.AI;

//Handles enemy UI Elements
public class ActiveSensingDetectable : MonoBehaviour
{
    [SerializeField] private Transform trackerPrefab;
    [SerializeField] private Transform anchorPoint;
    [SerializeField] private GameObject markTransform;

    private ObjectPool pool;

    private void Awake()
    {
        pool = FindFirstObjectByType<ObjectPool>();
    }

    public void EnableMark(bool state)
    {
        markTransform.SetActive(state);
    }

    public void SpawnTracker(string enemyID, Vector3 position, Color color) 
    {
        GameObject trackerObject = pool.GetObject(trackerPrefab.gameObject);

        float heightOffset = 4.2f;
        position.y += heightOffset;

        trackerObject.transform.position = position;
        //hologramObject.transform.rotation = anchor.rotation;

        if(trackerObject.TryGetComponent(out SonarMark sonarMark)) 
        {
            sonarMark.SetID(enemyID);
            sonarMark.SetColor(color);

            NavMeshAgent navMeshAgent = GetComponentInChildren<NavMeshAgent>();
            if (navMeshAgent != null) 
            {
                Vector3 velocity = navMeshAgent.velocity;
                float maxSpeed = navMeshAgent.speed;

                if (navMeshAgent.velocity == Vector3.zero) 
                {
                    velocity = navMeshAgent.transform.forward;
                    maxSpeed = -1;
                }

                sonarMark.SetMovementHint(velocity, maxSpeed);
            }
        }
    } 
}
