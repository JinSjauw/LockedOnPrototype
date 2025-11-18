using System.Collections.Generic;
using UnityEngine;

public class LegController : MonoBehaviour
{
    //[SerializeField] List<Transform> transforms;

    [SerializeField] private Transform legTarget;
    [SerializeField] private Transform currentLegTransform;
    [SerializeField] private Transform legTransform;
    [SerializeField] private AnimationCurve legHeightCurve;

    [SerializeField] private float legHeightOffset = 0.35f;
    [SerializeField] private float maxDistanceFromTarget = 0.14f;

    private float distanceFromTarget;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        UpdateLegTarget(legTransform.position);
    }

    // Update is called once per frame
    void Update()
    {
        FixLeg();

        distanceFromTarget = Vector3.Distance(legTransform.position, legTarget.position);

        if(distanceFromTarget >= maxDistanceFromTarget) 
        {
            UpdateLegTarget(legTarget.position);
        }
    }

    private void FixLeg() 
    {
        legTransform.position = currentLegTransform.position;
        legTransform.up = currentLegTransform.up;
    }

    private void UpdateLegTarget(Vector3 updatePosition) 
    {
        if(Physics.Raycast(updatePosition, Vector3.down, out RaycastHit hit, 10, LayerMask.GetMask("Ground"))) 
        {
            currentLegTransform.position = hit.point + new Vector3(0, legHeightOffset, 0);
            currentLegTransform.up = -hit.normal;

            legTarget.position = currentLegTransform.position;
            legTarget.up = currentLegTransform.up;
        }
    }
}
