using Ami.BroAudio;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class GunController : MonoBehaviour
{
    [Header("Component Refs")]
    [SerializeField] private ObjectPool pool;
    [SerializeField] private Transform projectileCollection;

    [SerializeField] private UIEvents uiEvents;
    [SerializeField] private SoundConfig soundConfig;

    [Header("UI Refs")]
    [SerializeField] private TextMeshProUGUI travelTimeUI;

    [Header("Trajectory Refs")]
    [SerializeField] private Transform muzzleTransform;
    [SerializeField] private Transform targetTransform;
    [SerializeField] private Transform curveControl;
    [SerializeField] private CurveController realCurveController;
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
    [SerializeField] private ProjectileVariables projectileVariables;
    [SerializeField] private float travelTime;
    [SerializeField] private float damage;

    [Header("Gun Variables")]
    [SerializeField] private float roundPerMinute;
    [SerializeField] private int totalAmmo = 12;

    private float firingDelay;
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
        //Debug.Log(trajectoryHeightChange);
    }

    #endregion

    #region Unity

    private void Awake()
    {
        pool =  FindFirstObjectByType<ObjectPool>();
    }

    private void Start()
    {
        uiEvents.OnMainGunAmmoChanged(totalAmmo);
    }

    private void Update()
    {
        AdjustTrajectoryApex(Time.deltaTime);

        travelTimeUI.text = CalculateTravelTime().ToString(".0");
        firingDelay = 1.0f / (roundPerMinute / 60);

        if (isReloading) 
        {
            FiringCoolDown(Time.deltaTime);
        }
    }

    #endregion

    #region Functions

    private void FireProjectile()
    {
        if (totalAmmo <= 0) return;

        BroAudio.Play(soundConfig.artillerySound, muzzleTransform.position);

        GameObject newProjectile = pool.GetObject(projectilePrefab);
        newProjectile.transform.position = muzzleTransform.position;
        newProjectile.transform.forward = muzzleTransform.forward;
        newProjectile.transform.parent = projectileCollection;
        
        if (newProjectile.TryGetComponent(out Projectile projectileComponent))
        {
            projectileComponent.InitProjectile(muzzleTransform.position, targetTransform.position, curveControl.position, CalculateTravelTime(), damage, pool, soundConfig, true);

            totalAmmo--;

            uiEvents.OnReloadMainGun(firingDelay);
            uiEvents.OnMainGunAmmoChanged(totalAmmo);
        }
        else 
        {
            Debug.LogError("Couldn't get Projectile component on: " + newProjectile.name);
        }

        isReloading = true;
    }

    private float CalculateTravelTime() 
    {
        float distance = Vector3.Distance(muzzleTransform.position, targetTransform.position);
        float result = projectileVariables.GetTravelTime(distance, realCurveController.DesiredCurveHeight);

        return result;
    }

    private bool FiringCoolDown(float deltaTime)
    {
        
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

            //Debug.Log(currentTrajectoryChangeRate);

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
