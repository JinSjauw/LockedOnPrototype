using UnityEngine;

public class EnemyTargetingController : MonoBehaviour
{
    [SerializeField] private Transform target;

    public void SetTarget(Vector3 position)
    {
        target.position = position;
    }
}
