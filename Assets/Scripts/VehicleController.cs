using UnityEngine;
using UnityEngine.AI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
public class VehicleController : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private Transform indicator;
    [SerializeField] private NavMeshAgent agent;
    [SerializeField] private PlayerEvents playerEvents;

    [SerializeField] private float groundOffset;

    private Vector3 mousePosition;

    NavMeshPath path;

    private bool leftShiftDown;
    private bool pointerOverUI;

    #region Input Handling

    void OnLeftClick(InputValue value) 
    {
        if (value.isPressed && !leftShiftDown && !pointerOverUI) 
        {
            Move();
        }
    }

    private void OnLeftShift(InputValue value)
    {
        leftShiftDown = value.isPressed;
    }

    private void OnStopMoving(InputValue value) 
    {
        StopMoving();
    }

    #endregion

    #region Unity

    private void Awake()
    {
        path = new NavMeshPath();
    }

    private void Update()
    {
        mousePosition = Mouse.current.position.ReadValue();
    }

    private void LateUpdate()
    {
        if (!EventSystem.current.IsPointerOverGameObject())
        {
            pointerOverUI = false;
        }
        else 
        {
            pointerOverUI= true;
        }

        if (!agent.hasPath) 
        {
            StopMoving();
        }
    }

    #endregion

    #region Functions

    private void Move()
    {
        agent.isStopped = false;
        playerEvents.OnPlayerStationary(false);
        //Get point in world space from screen space.
        RaycastHit hit;
        Ray ray = Camera.main.ScreenPointToRay(mousePosition);

        //Use navmesh to pathfind
        if (Physics.Raycast(ray, out hit, float.MaxValue, LayerMask.GetMask("Ground"))) 
        {
            Vector3 worldPosition = hit.point + hit.normal * groundOffset;
            indicator.position = worldPosition;

            indicator.transform.rotation = Quaternion.LookRotation(hit.normal, Vector3.up);

            agent.CalculatePath(worldPosition, path);

            if(path.status == NavMeshPathStatus.PathComplete) 
            { 
                agent.SetPath(path);
            }
        }
    }

    private void StopMoving() 
    {
        agent.isStopped = true;
        playerEvents.OnPlayerStationary(true);
    }


    #endregion
}
