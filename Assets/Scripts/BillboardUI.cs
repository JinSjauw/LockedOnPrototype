using UnityEngine;

public class BillboardUI : MonoBehaviour
{
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private Transform anchor;

    [SerializeField] private float distanceToCamera = 0;
    [SerializeField] private bool hasAnchor;

    Transform origin;

    //TODO: Make the distanceToCamera smaller the closer the camera is.
    private void Awake()
    {
        if (!cameraTransform) 
        {
            cameraTransform = Camera.main.transform;
        }

        if (!hasAnchor) 
        {
            origin = transform;
        }
        else 
        {
            origin = anchor;
        }
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 directionToCamera = (cameraTransform.position - origin.position).normalized;

        transform.forward = -directionToCamera;

        if (hasAnchor) 
        {
            transform.position = anchor.position + directionToCamera * distanceToCamera;
        }
    }
}
