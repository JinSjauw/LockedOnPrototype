using UnityEngine;

public class EnemyTargetingController : MonoBehaviour
{
    [SerializeField] private Transform target;

    private Vector3 targetPosition;
    private float changeRate = 5;
    private bool onTarget = false;

    public bool OnTarget { get => onTarget; }

    //Interpolate realPos to targetPos?
    private void Update()
    {
        target.position = Vector3.MoveTowards(target.position, targetPosition, changeRate * Time.deltaTime);

        if(Vector3.Distance(target.position, targetPosition) < 0.01f) 
        {
            onTarget = true;
        }
    }

    public void SetTarget(Vector3 position)
    {
        targetPosition = position;
        onTarget = false;
    }
}
