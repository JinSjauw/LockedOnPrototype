using UnityEngine;
using UnityEngine.AI;

public class EnemyController : MonoBehaviour
{
    [SerializeField] private float fieldOfSight;
    [SerializeField] private float firingRadius;
    [SerializeField] private float detectionRadius;
    [SerializeField] private float randomPositionRadius;
    [SerializeField] private Transform playerTarget;
    [SerializeField] private EnemyTargetingController targetingController;

    [Header("Temp Refs")]

    [Header("Navmesh Ref - TEMP")]
    [SerializeField] private NavMeshAgent agent;
    private NavMeshPath path;
    private Vector3 movePosition;
    private bool hasAttacked = false;

    [Header("Trajectory Ref - TEMP")]
    //[SerializeField] private LineRenderer lineRenderer;
    [SerializeField] private Transform previewMuzzleTransform;
    [SerializeField] private Transform muzzleTransform;
    [SerializeField] private Transform targetTransform;
    [SerializeField] private Transform previewCurveControlTransform;
    [SerializeField] private Transform curveControlTransform;

    [SerializeField] private int attemptsPerFrame = 2;
    [SerializeField] private int maxTotalAttempts = 8;
    [SerializeField] private float trajectoryAdjustmentStep = 1;

    private int attemptsTotal = 0;

    private CurveController previewCurveController;
    private CurveController curveController;
    private float curveHeight;

    [SerializeField] private float randomTargetRadius;
    private bool hasTarget;
    private Vector3 targetPosition;

    [SerializeField] private float lowAngleHeight;
    [SerializeField] private float neutralAngleHeight;
    [SerializeField] private float highAngleHeight;

    [Header("Projectile Variables - TEMP")]
    [SerializeField] private Transform hitDebug;
    [SerializeField] private Transform projectileCollection;
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private float projectileTravelTime;
    [SerializeField] private float projectileDamage;
    [SerializeField] private float roundPerMinute;

    private float targetingCooldown = 1.5f;
    private float targetingTimer;
    private bool canTarget = false;


    //private float firingDelay;
    private float firingTimer;
    private bool isReloading = false;
    private bool hasTrajectory = false;
    private bool isAwareOfPlayer = true;
    private ObjectPool pool;

    private void Awake()
    {
        path = new NavMeshPath();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        pool = FindFirstObjectByType<ObjectPool>();
        curveController = curveControlTransform.GetComponent<CurveController>();
        previewCurveController = previewCurveControlTransform.GetComponent<CurveController>();

    }

    // Update is called once per frame
    void Update()
    {
        //FireAtPlayer();
        AttackBehaviour();

        if (isReloading)
        {
            FiringCoolDown(Time.deltaTime);
        }

        if(targetingTimer > 0 && !canTarget) 
        {
            targetingTimer -= Time.deltaTime;
        }
        else 
        {
            canTarget = true;
        }
    }

    private void AttackBehaviour() 
    {
        //Determines next point to patrol to
        float maxAngle = 75;
        float minDistance = 2;
        float maxDistance = 8;

        if (hasAttacked) 
        {
            //Using the last previous known target position of the player
            Vector3 nextPosition = CalculateNewPath(targetPosition, maxAngle, minDistance, maxDistance);

            MoveToPosition(nextPosition);
        }
        

        if (!hasAttacked) 
        {
            FireAtPlayer();
        }
    }

    private void MoveToPosition(Vector3 position) 
    {
        //Moves to given position
        if (agent.CalculatePath(position, path)) 
        {
            agent.SetPath(path);
            hasAttacked = false;
        }
    }

    private Vector3 CalculateNewPath(Vector3 target, float maxAngle, float minDistance, float maxDistance) 
    {
        //Determines a new location to move to to attack the player.

        //Keep it simple - grunts
        //try and get LOS on the player.
        //So move closer if you cannot fire.

        //artillery units - slower and in need of grunts
        //follow the grunts and try to stay out of player LOS
        //Fire at the player's last known velocity (estimated future position)

        //Hunter unit. - can go solo a bit faster but a bit less firepower than normal artillery unit
        //Artillery that actively hunts player(not in a squad)
        //track the player
        //try and ambush player - (guess players estimated future position and setup in the area)

        //For now choose random new position. within attack range

        Vector3 start = transform.position;

        Vector3 directionToTarget = (target - start).normalized;

        // Random rotation within cone
        Quaternion randomRotation = Quaternion.Euler(
            Random.Range(-maxAngle, maxAngle),
            Random.Range(-maxAngle, maxAngle),
            0
        );

        Vector3 randomDirection = randomRotation * directionToTarget;
        float randomDistance = Random.Range(minDistance, maxDistance);

        return start + (randomDirection * randomDistance);

        //return Vector3.zero;
    }
        
    private void FireAtPlayer() 
    {
        if(Vector2.Distance(new Vector2(playerTarget.position.x, playerTarget.position.z), new Vector2(transform.position.x, transform.position.z)) < firingRadius) 
        {
            if (!isAwareOfPlayer || !canTarget) return;

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

                //Set preview target
                targetingController.SetPreviewTarget(targetPosition);
            }

            if (!Physics.Linecast(muzzleTransform.position, playerTarget.position, LayerMask.GetMask("Ground"))) 
            {
                //Direct Shot!
                if (hasTrajectory)
                {
                    if (!isReloading && targetingController.OnTarget && curveController.ReachedDesiredHeight)
                    {
                        Fire();
                        hasTarget = false;
                        hasTrajectory = false;
                        canTarget = false;
                        targetingTimer = targetingCooldown;
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
                        canTarget = false;
                        targetingTimer = targetingCooldown;
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

        Vector3 startPosition = previewMuzzleTransform.position;
        Vector3 endPosition = targetPosition;

        //Follow x and z of the curveControl transform
        //curveControlPosition.x = previewCurveControlTransform.position.x;
        //curveControlPosition.z = previewCurveControlTransform.position.z;

        if (attemptsTotal == 0) 
        {
            //curveControlPosition.y = startingHeight;
            //curveControlPosition = curveControlTransform.position;
            curveHeight = startingHeight;
            previewCurveController.SetHeight(startingHeight);
        }

        Vector3 oldPosition = startPosition;
        Vector3 newPosition;

        for (int i = 0; i < attemptsPerFrame && !hasTrajectory; i++) 
        {

            for (int j = 0; j < linePositions; j++)
            {
                newPosition = QuadraticCurve.EvaluateCurve(startPosition, endPosition, previewCurveControlTransform.position, (float)j / (float)linePositions);

                //Debug.Log(newPosition + " " + oldPosition + " " + j);
                Debug.DrawLine(oldPosition, newPosition, i % 2 == 0 ? Color.red : Color.blue, 2);
                if (Physics.Linecast(oldPosition, newPosition, out RaycastHit hit, LayerMask.GetMask("Ground")))
                {
                    
                    //Debug.Log("Trajectory Obstructed! By: " + hit.collider.name);
                    //curveController.DesiredCurveHeight += adjustmentStep;
                    hitDebug.position = hit.point;
                    curveHeight += trajectoryAdjustmentStep;
                    previewCurveController.SetHeight(curveHeight);

                    hasTrajectory = false;
                    break;
                }

                if (j + 1 >= linePositions)
                {
                    hasTrajectory = true;
                    break;
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
            targetingController.SetTarget(targetPosition);
            curveController.InterpolateTo = true;

            //Add extra margin for bullet girth;
            curveController.DesiredCurveHeight = curveHeight + trajectoryAdjustmentStep;

            //Debug.Log("Setting Desired Curve: " + " Real: " + curveController.DesiredCurveHeight + " Preview: " + previewCurveController.DesiredCurveHeight);

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
        hasAttacked = true;
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
