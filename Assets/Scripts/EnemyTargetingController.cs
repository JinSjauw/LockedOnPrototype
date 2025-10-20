using UnityEngine;

public class EnemyTargetingController : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private Transform previewTarget;

    [SerializeField] private float changeRate = 5;

    private Vector3 targetPosition;
    private bool onTarget = false;

    public bool OnTarget { get => onTarget; }

    private void Awake()
    {
        targetPosition = target.position;
    }

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

    public void SetPreviewTarget(Vector3 position) 
    {
        previewTarget.position = position;
    }
}
