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
    
    [SerializeField] private int attemptsPerFrame = 2;
    [SerializeField] private int maxTotalAttempts = 8;
    [SerializeField] private float trajectoryAdjustmentStep = 1;

    private int attemptsTotal = 0;

    private CurveController curveController;
    private float targetCurveHeight;

    [SerializeField] private float randomTargetRadius;
    private bool hasTarget;
    private Vector3 targetPosition;

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
    private bool hasTrajectory = false;
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
        //if(!isReloading && targetingController.OnTarget && curveController.ReachedDesiredHeight) 
        //{
        //    canFire = true;
        //}

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

            //Take a snapshot and set a targetPosition
            //Take random position in radius of the player
            //Lock this target position in and don't take new target until this has been handled completely
            //Select targetPosition
            if (!hasTarget && !isReloading) 
            {
                Debug.Log("Selected new Target");
                Vector2 randomFactor = Random.insideUnitCircle * randomTargetRadius;
                targetPosition = playerTarget.position + new Vector3(randomFactor.x, 0, randomFactor.y);
                hasTarget = true;

                targetingController.SetTarget(targetPosition);
            }

            if (!Physics.Linecast(muzzleTransform.position, playerTarget.position, LayerMask.GetMask("Ground"))) 
            {
                //Direct Shot!
                ////curveController.DesiredCurveHeight = lowAngleHeight;
                if (hasTrajectory)
                {
                    if (!isReloading && targetingController.OnTarget && curveController.ReachedDesiredHeight)
                    {
                        Fire();
                        hasTarget = false;
                        hasTrajectory = false;
                    } 
                }
                else 
                {
                    Debug.Log("Find trajectory when having direct sight");
                    FindTrajectory(lowAngleHeight);
                }
            }
            else
            {
                if (hasTrajectory)
                {
                    if (!isReloading && targetingController.OnTarget && curveController.ReachedDesiredHeight) 
                    {
                        Fire();
                        hasTarget = false;
                        hasTrajectory = false;
                    }
                }
                else
                {
                    Debug.Log("Find trajectory when having no direct sight");
                    FindTrajectory(neutralAngleHeight);
                }
            }
        }
    }

    //Find correct CurveControlHeight
    private void FindTrajectory(float startingHeight) 
    {
        //Maybe do this a few times per frame. for optimization

        //int maxAttempts = 4;
        //float adjustmentStep = 1.25f;

        int linePositions = 8;

        Vector3 startPosition = muzzleTransform.position;
        Vector3 endPosition = targetPosition;
        Vector3 controlPosition = curveControlTransform.position;

        if(attemptsTotal == 0) 
        {
            controlPosition.y = startingHeight;
        }

        Vector3 oldPosition = startPosition;
        Vector3 newPosition;

        for (int i = 0; i < attemptsPerFrame && !hasTrajectory; i++) 
        {

            for (int j = 0; j < linePositions; j++)
            {
                newPosition = QuadraticCurve.EvaluateCurve(startPosition, endPosition, controlPosition, (float)j / (float)linePositions);

                //Debug.Log(newPosition + " " + oldPosition + " " + j);

                if (Physics.Linecast(oldPosition, newPosition, out RaycastHit hit, LayerMask.GetMask("Ground")))
                {
                    //Debug.Log("Trajectory Obstructed! By: " + hit.collider.name);
                    //curveController.DesiredCurveHeight += adjustmentStep;
                    controlPosition.y += trajectoryAdjustmentStep;

                    hasTrajectory = false;
                    break;
                }

                if(j == linePositions - 1) 
                {
                    hasTrajectory = true;
                    //Debug.Log("Trajectory Not Obstructed! " + controlPosition.y + " " + j);
                    //curveController.DesiredCurveHeight = controlPosition.y;
                    //return controlPosition.y;
                }

                oldPosition = newPosition;
                //curveController.DesiredCurveHeight = controlPosition.y;
            }
        }

        attemptsTotal++;

        if (hasTrajectory) 
        {
            Debug.Log("Found Trajectory");
            //Return controlPosition.y;
            //Set desiredControlPosition.Y
            curveController.InterpolateTo = true;
            curveController.DesiredCurveHeight = controlPosition.y;
            attemptsTotal = 0;
        }

        if(attemptsTotal >= maxTotalAttempts && !hasTrajectory) 
        {
            attemptsTotal = 0;
            hasTarget = false;

            Debug.Log("Failed to find trajectory...");
            //Fire();
            //Or just fires?
        }

        //return -100;
    }

    private void Fire() 
    {
        Debug.Log("Fire!");

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
