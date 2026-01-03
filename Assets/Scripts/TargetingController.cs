using System.IO;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;

public class TargetingController : MonoBehaviour
{
    [SerializeField] private Transform targetTransfom;

    private Vector3 mousePosition;
    private bool isDragging;

    #region Input Handling

    void OnRightClick(InputValue value) 
    {
        if (value.isPressed) 
        {
            //Change Targeting
            MoveTarget();
            //if it is held down have the target follow the mousePosition
            isDragging = true;
        }
        else
        {
            isDragging = false;
        }
    }
    #endregion

    #region Unity

    private void Update()
    {
        mousePosition = Input.mousePosition;
        if (isDragging)
        {
            MoveTarget();
        } 
    }

    #endregion

    #region Functions

    private void MoveTarget() 
    {
        RaycastHit hit;
        Ray ray = Camera.main.ScreenPointToRay(mousePosition);

        //Use navmesh to pathfind
        if (Physics.Raycast(ray, out hit, float.MaxValue, LayerMask.GetMask("Ground")))
        {
            Vector3 worldPosition = hit.point;
            targetTransfom.position = worldPosition;
        }
    }

    #endregion
}
