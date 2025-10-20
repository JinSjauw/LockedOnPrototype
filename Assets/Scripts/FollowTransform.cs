using UnityEngine;

public class FollowTransform : MonoBehaviour
{
    [SerializeField] private Transform followTarget;

    // Update is called once per frame
    void Update()
    {
        transform.position = followTarget.position;
    }
}
