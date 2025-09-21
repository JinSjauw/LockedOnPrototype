using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;
public class VehicleController : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private Transform debugSphere;
    [SerializeField] private NavMeshAgent agent;

    private Vector3 mousePosition;
    private Transform playerVehicle;

    NavMeshPath path;

    #region Input Handling

    void OnLeftClick(InputValue value) 
    {
        if (value.isPressed) 
        {
            Move();
        }
    }

    #endregion

    #region Unity

    private void Awake()
    {
        playerVehicle = transform;
        path = new NavMeshPath();
    }

    private void Update()
    {
        mousePosition = Input.mousePosition;
    }

    #endregion

    #region Functions

    private void Move()
    {
        //Get point in world space from screen space.
        RaycastHit hit;
        Ray ray = Camera.main.ScreenPointToRay(mousePosition);

        //Use navmesh to pathfind
        if (Physics.Raycast(ray, out hit)) 
        {
            Vector3 worldPosition = hit.point;
            debugSphere.position = worldPosition;
            
            agent.CalculatePath(worldPosition, path);

            if(path.status == NavMeshPathStatus.PathComplete) 
            { 
                agent.SetPath(path);
            }
        }
    }


    #endregion
}
