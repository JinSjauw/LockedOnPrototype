using System;
using System.Collections.Generic;
using Unity.VisualScripting;
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
    private float distanceFromTarget;

    private bool legIsMoving = false;
    private bool canUpdate = true;
    private bool isGrounded = false;

    [SerializeField] private int legID;

    public EventHandler<int> OnMoveFinished;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        UpdateLegTarget(legTransform.position, currentLegTransform);
    }

    // Update is called once per frame
    void Update()
    {
        isGrounded = CheckGrounded();

        if (!legIsMoving) 
        {
            FixLeg();
        }
        else 
        {
            AnimateLeg();
        }

        //UpdateLegTarget(legTransform.position, legTargetTransform);

        if (!canUpdate) return;

        distanceFromTarget = Vector3.Distance(legTransform.position, legTargetTransform.position);

        if(distanceFromTarget >= maxDistanceFromTarget && !legIsMoving) 
        {
            oldPosition = currentLegTransform.position;
            UpdateLegTarget(legTargetTransform.position, currentLegTransform);
            legIsMoving = true;
        }
    }

    private bool CheckGrounded() 
    {
        return Physics.Raycast(legTransform.position, legTransform.up, legTransform.position.y + legHeightOffset + 0.05f);
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

            OnMoveFinished?.Invoke(this, legID);

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

    public void SetCanUpdate(bool state) 
    {
        canUpdate = state;
    }

    public int GetID() 
    {
        return legID;
    }
}
