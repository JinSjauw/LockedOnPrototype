using UnityEngine;

public class EnemyController : MonoBehaviour
{
    [SerializeField] private float fieldOfSight;
    [SerializeField] private float detectionRadius;
    [SerializeField] private Transform playerTarget;
    [SerializeField] private EnemyTargetingController targetingController;

    [Header("Temp Refs")]
    [Header("Trajectory Ref - TEMP")]
    [SerializeField] private LineRenderer lineRenderer;
    [SerializeField] private Transform muzzleTransform;
    [SerializeField] private Transform targetTransform;
    [SerializeField] private Transform curveControlTransform;
    private CurveController curveController;

    [SerializeField] private float lowAngleHeight;
    [SerializeField] private float neutralAngleHeight;
    [SerializeField] private float highAngleHeight;

    [Header("Projectile Variables - TEMP")]
    [SerializeField] private Transform projectileCollection;
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private float projectileTravelTime;
    [SerializeField] private float projectileDamage;
    [SerializeField] private float roundPerMinute;

    //private float firingDelay;
    private float firingTimer;
    private bool isReloading = false;
    private bool hasTrajectory = true;
    private bool canFire = false;
    private bool isAwareOfPlayer = true;
    private ObjectPool pool;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        pool = FindFirstObjectByType<ObjectPool>();
        curveController = curveControlTransform.GetComponent<CurveController>();
    }

    // Update is called once per frame
    void Update()
    {
        DetectPlayer();
        
        if (isReloading)
        {
            FiringCoolDown(Time.deltaTime);
        }
    }

    private void DetectPlayer() 
    {
        if(Vector2.Distance(new Vector2(playerTarget.position.x, playerTarget.position.z), new Vector2(transform.position.x, transform.position.z)) < detectionRadius) 
        {
            if (!isAwareOfPlayer) return;
            targetingController.SetTarget(playerTarget.position);

            if (!Physics.Linecast(muzzleTransform.position, playerTarget.position, LayerMask.GetMask("Ground"))) 
            {
                //Direct Shot!
                ////curveController.DesiredCurveHeight = lowAngleHeight;
                //if (FindTrajectory(lowAngleHeight)) 
                //{
                //    if (!isReloading) Fire();
                //}
            }
            else
            {
                //if (FindTrajectory(neutralAngleHeight))
                //{
                //    if (!isReloading) Fire();
                //}
            }
        }
        else 
        {
            //curveController.DesiredCurveHeight = neutralAngleHeight;
            //Neutral turret rotation; //Set target infront;
        }
    }

    private float FindTrajectory(float startingHeight) 
    {
        //Maybe do this a few times per frame. for optimization

        int maxAttempts = 4;
        float adjustmentStep = 1.25f;

        int linePositions = 8;

        Vector3 startPosition = muzzleTransform.position;
        Vector3 endPosition = targetTransform.position;
        Vector3 controlPosition = curveControlTransform.position;
        controlPosition.y = startingHeight;

        Vector3 oldPosition = startPosition;
        Vector3 newPosition;

        for (int i = 0; i < maxAttempts; i++) 
        {

            for (int j = 0; j < linePositions; j++)
            {
                newPosition = QuadraticCurve.EvaluateCurve(startPosition, endPosition, controlPosition, (float)j / (float)linePositions);

                //Debug.Log(newPosition + " " + oldPosition + " " + j);

                if (Physics.Linecast(oldPosition, newPosition, out RaycastHit hit, LayerMask.GetMask("Ground")))
                {
                    //Debug.Log("Trajectory Obstructed! By: " + hit.collider.name);
                    //curveController.DesiredCurveHeight += adjustmentStep;
                    controlPosition.y += adjustmentStep;

                    hasTrajectory = false;
                    break;
                }

                if(j == linePositions - 1) 
                {
                    hasTrajectory = true;
                    //Debug.Log("Trajectory Not Obstructed! " + controlPosition.y + " " + j);
                    //curveController.DesiredCurveHeight = controlPosition.y;
                    return controlPosition.y;
                }

                oldPosition = newPosition;
                //curveController.DesiredCurveHeight = controlPosition.y;
            }
        }

        return -100;
    }

    private void Fire() 
    {
        GameObject newProjectile = pool.GetObject(projectilePrefab);
        newProjectile.transform.position = muzzleTransform.position;
        newProjectile.transform.forward = muzzleTransform.forward;
        newProjectile.transform.parent = projectileCollection;

        if (newProjectile.TryGetComponent(out Projectile projectileComponent))
        {
            projectileComponent.InitProjectile(muzzleTransform.position, targetTransform.position, curveControlTransform.position, projectileTravelTime, projectileDamage, pool);
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
}
