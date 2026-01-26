using System;
using System.IO;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;

public class TargetingController : MonoBehaviour
{
    [SerializeField] private Transform targetTransfom;
    [SerializeField] private float groundOffset = 0.15f;

    private Vector3 mousePosition;
    private bool isDragging;

    #region Unity

    private void Start()
    {
        Services.Input.OnPlaceTargetEvent += MoveTarget;
        Services.Input.OnDragTargetEvent += DragTarget;
    }

    private void Update()
    {
        mousePosition = Mouse.current.position.ReadValue();

        if (isDragging)
        {
            MoveTarget();
        } 
    }

    #endregion

    #region Functions

    private void DragTarget(bool isDragging)
    {
        this.isDragging = isDragging;
    }

    private void MoveTarget() 
    {
        RaycastHit hit;
        Ray ray = Camera.main.ScreenPointToRay(mousePosition);

        //Use navmesh to pathfind
        if (Physics.Raycast(ray, out hit, float.MaxValue, LayerMask.GetMask("Ground")))
        {
            Vector3 worldPosition = hit.point + hit.normal * groundOffset;
            targetTransfom.position = worldPosition;

            targetTransfom.rotation = Quaternion.LookRotation(hit.normal, Vector3.up);
        }
    }

    #endregion
}
