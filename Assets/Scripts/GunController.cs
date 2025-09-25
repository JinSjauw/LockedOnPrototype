using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;

public class GunController : MonoBehaviour
{
    [Header("Component Refs")]
    [SerializeField] private ObjectPool pool;
    [SerializeField] private Transform projectileCollection;

    [Header("Trajectory Refs")]
    [SerializeField] private Transform muzzleTransform;
    [SerializeField] private Transform targetTransform;
    [SerializeField] private Transform curveControlTransform;

    [Header("Trajectory Variables")]
    [SerializeField] private float trajectoryChangeRate;
    [SerializeField] private float trajectoryChangeSmoothing;
    [SerializeField] private float minApex;
    [SerializeField] private float maxApex;

    private CurveController curveController;
    private Vector3 trajectoryHeightChange;
    private float currentTrajectoryChangeRate;

    [Header("Projectile Variables")]
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private float travelTime;



    #region Input Handling

    void OnFire(InputValue value) 
    {
        if (value.isPressed) 
        {
            FireProjectile();
        }
    }
    
    void OnAdjustTrajectory(InputValue value)
    {
        trajectoryHeightChange = value.Get<Vector2>();
        Debug.Log(trajectoryHeightChange);
    }

    #endregion

    #region Unity

    private void Awake()
    {
        pool =  FindFirstObjectByType<ObjectPool>();

        if (curveControlTransform.TryGetComponent(out CurveController curveController))
        {
            this.curveController = curveController;
        }
        else 
        {
            Debug.Log("CurveController Component not found");
        }
    }

    private void Update()
    {
        AdjustTrajectoryApex(Time.deltaTime);
    }

    #endregion

    #region Functions

    private void FireProjectile()
    {
        GameObject newProjectile = pool.GetObject(projectilePrefab);
        newProjectile.transform.position = muzzleTransform.position;
        newProjectile.transform.forward = muzzleTransform.forward;
        newProjectile.transform.parent = projectileCollection;

        if (newProjectile.TryGetComponent(out Projectile projectileComponent))
        {
            projectileComponent.InitProjectile(muzzleTransform.position, targetTransform.position, curveControlTransform.position, travelTime, pool);
        }
        else 
        {
            Debug.LogError("Couldn't get Projectile component on: " + newProjectile.name);
        }
    }

    private void AdjustTrajectoryApex(float deltaTime) 
    {
        float targetChangeRate = 0;

        //Needs some more tweaking.

        //How would it need to feel?
        //Quickly but smoothly set target trajectory
        //Have the real Trajectory move to it.

        if(Mathf.Abs(trajectoryHeightChange.y) >= 0.01f) 
        {
            targetChangeRate = trajectoryChangeRate * trajectoryHeightChange.y;

            currentTrajectoryChangeRate = Mathf.Lerp(currentTrajectoryChangeRate, targetChangeRate, trajectoryChangeSmoothing * deltaTime);
            Debug.Log(currentTrajectoryChangeRate);

            curveController.DesiredCurveHeight += currentTrajectoryChangeRate;
        }

    }

    #endregion

}
