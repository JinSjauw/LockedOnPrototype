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
    [SerializeField] private Transform curveControl;
    [SerializeField] private CurveController previewCurveController;

    [Header("Trajectory Variables")]
    [SerializeField] private float trajectoryChangeRate;
    [SerializeField] private float trajectoryChangeSmoothing;
    [SerializeField] private float maxTrajectoryChangeRate;
    [SerializeField] private float minApex;
    [SerializeField] private float maxApex;

    private Vector3 trajectoryHeightChange;
    private float currentTrajectoryChangeRate;

    [Header("Projectile Variables")]
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private float travelTime;
    [SerializeField] private float damage;

    [Header("Gun Variables")]
    [SerializeField] private float roundPerMinute;

    //private float firingDelay;
    private float firingTimer;
    private bool isReloading = false;

    #region Input Handling

    void OnFire(InputValue value) 
    {
        if (value.isPressed && !isReloading) 
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
    }

    private void Update()
    {
        AdjustTrajectoryApex(Time.deltaTime);

        if (isReloading) 
        {
            FiringCoolDown(Time.deltaTime);
        }
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
            projectileComponent.InitProjectile(muzzleTransform.position, targetTransform.position, curveControl.position, travelTime, damage, pool, true);
        }
        else 
        {
            Debug.LogError("Couldn't get Projectile component on: " + newProjectile.name);
        }

        isReloading = true;
    }

    private bool FiringCoolDown(float deltaTime)
    {
        float firingDelay = 1.0f / (roundPerMinute / 60);
        firingTimer += deltaTime;
        if (firingTimer >= firingDelay)
        {
            firingTimer = 0.0f;
            isReloading = false;
            return true;
        }

        return false;
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
            currentTrajectoryChangeRate = Mathf.Clamp(currentTrajectoryChangeRate, -maxTrajectoryChangeRate, maxTrajectoryChangeRate);

            Debug.Log(currentTrajectoryChangeRate);

            if(previewCurveController.CurrentCurveHeight + currentTrajectoryChangeRate >= maxApex) 
            {
                previewCurveController.DesiredCurveHeight = maxApex;
            }
            else if(previewCurveController.CurrentCurveHeight + currentTrajectoryChangeRate <= minApex) 
            {
                previewCurveController.DesiredCurveHeight = minApex;
            }
            else 
            {
                previewCurveController.DesiredCurveHeight += currentTrajectoryChangeRate;
            }
        }
        else 
        {
            currentTrajectoryChangeRate = 0;
        }

    }

    #endregion

}
