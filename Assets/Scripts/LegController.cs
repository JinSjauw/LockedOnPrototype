using System.Collections.Generic;
using UnityEngine;

public class LegController : MonoBehaviour
{
    
    //Need to update the height for target for uneven terrain.
    [SerializeField] private Transform legTargetTransform;

    [SerializeField] private Transform currentLegTransform;
    [SerializeField] private Transform legTransform;
    [SerializeField] private AnimationCurve legHeightCurve;

    [SerializeField] private float legHeightOffset = 0.35f;
    [SerializeField] private float legHeightApex = 0.35f;

    [SerializeField] private float maxDistanceFromTarget = 0.14f;

    [SerializeField] private float legMoveTime;
    private float legMoveTimer;
    Vector3 oldPosition;
    private bool legIsMoving = false;

    private float distanceFromTarget;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        UpdateLegTarget(legTransform.position, currentLegTransform);
    }

    // Update is called once per frame
    void Update()
    {
        if (!legIsMoving) 
        {
            FixLeg();
        }
        else 
        {
            AnimateLeg();
        }

        //UpdateLegTarget(legTransform.position, legTargetTransform);

        distanceFromTarget = Vector3.Distance(legTransform.position, legTargetTransform.position);

        if(distanceFromTarget >= maxDistanceFromTarget && !legIsMoving) 
        {
            oldPosition = currentLegTransform.position;
            UpdateLegTarget(legTargetTransform.position, currentLegTransform);
            legIsMoving = true;
        }
    }

    private void FixLeg() 
    {
        legTransform.position = currentLegTransform.position;
        legTransform.up = currentLegTransform.up;
    }

    private void AnimateLeg() 
    {
        //Lerp XZ to target position
        //Use animation curve for y height + heightOffset
        
        //Timer
        if(legMoveTimer < legMoveTime) 
        {
            legMoveTimer += Time.deltaTime;
        }
        else 
        {
            legMoveTimer = 0;
            legIsMoving = false;
            oldPosition = currentLegTransform.position;
        }

        float moveAlpha = (legMoveTimer - 0) / (legMoveTime - 0);

        Vector3 newPosition = Vector3.Lerp(oldPosition, legTargetTransform.position, moveAlpha);
        
        newPosition.y += (legHeightApex * legHeightCurve.Evaluate(moveAlpha));

        currentLegTransform.position = newPosition;

        FixLeg();
    }

    private void UpdateLegTarget(Vector3 updatePosition, Transform updateTransform) 
    {
        if(Physics.Raycast(updatePosition, Vector3.down, out RaycastHit hit, 10, LayerMask.GetMask("Ground"))) 
        {
            updateTransform.position = hit.point + new Vector3(0, legHeightOffset, 0);
            updateTransform.up = -hit.normal;

            //legTargetPosition.position = currentLegTransform.position;
            //legTargetPosition.up = currentLegTransform.up;
        }
    }
}
