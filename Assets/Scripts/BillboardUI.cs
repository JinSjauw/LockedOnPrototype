using System;
using UnityEngine;

public class BillboardUI : MonoBehaviour
{
    [SerializeField] private CameraEvents cameraEvents;
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private Transform anchor;

    [SerializeField] private bool scaleWithZoom;
    [SerializeField] private float distanceToCamera = 0;
    [SerializeField] private float maxScale;
    [SerializeField] private float minScale;

    Transform origin;
    private bool hasAnchor;

    //TODO: Make the distanceToCamera smaller the closer the camera is.
    private void Awake()
    {
        if (!cameraTransform) 
        {
            cameraTransform = Camera.main.transform;
        }

        if (anchor == null) 
        {
            origin = transform;
        }
        else if(anchor != null)
        {
            origin = anchor;
            hasAnchor = true;
        }
    }

    private void Start()
    {
        if (scaleWithZoom && cameraEvents != null) 
        {
            cameraEvents.cameraZoomChangedEvent += ChangeScale;
        }
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 directionToCamera = (cameraTransform.position - origin.position).normalized;

        transform.forward = -directionToCamera;

        if (hasAnchor) 
        {
            transform.position = origin.position + directionToCamera * distanceToCamera;
        }
    }

    private void ChangeScale(object sender, float zoom)
    {
        transform.localScale = Vector3.one * Mathf.Lerp(minScale, maxScale, zoom); //the more zoom the smaller
    }
}
