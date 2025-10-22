using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UIElements;

public class EnemyController : MonoBehaviour
{
    [SerializeField] private float fieldOfSight;
    [SerializeField] private float firingRadius;
    [SerializeField] private float detectionRadius;

    [SerializeField] private Transform playerTarget;
    [SerializeField] private EnemyTargetingController targetingController;
    [SerializeField] private EnemyTrajectoryController trajectoryController;

    [Header("Temp Refs")]

    [Header("Navmesh Ref - TEMP")]
    [SerializeField] private NavMeshAgent agent;
    private NavMeshPath path;

    private Vector3 pathOrigin;
    private Vector3 pathDestination;

    [SerializeField] private bool hasAttacked = false;
    [SerializeField] private bool hasPath = false;
    [SerializeField] private bool inPosition = true;
    [SerializeField] private bool canReposition = true;

    //Firing Type Logic
    [SerializeField] private EnemyFireType fireType;
    private bool canFire;
    private float inPositionAlpha = 1;


    [SerializeField] private float randomTargetRadius;

    [Header("Trajectory Variables - TEMP")]
    [SerializeField] private Transform muzzleTransform;
    [SerializeField] private CurveController curveController;
    [SerializeField] private Transform targetTransform;

    [SerializeField] private float lowAngleHeight;
    [SerializeField] private float neutralAngleHeight;
    [SerializeField] private float highAngleHeight;

    private Vector3 targetPosition;

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
    }

    // Update is called once per frame
    void Update()
    {
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

        //If it needs a new path calculate it.

        //Determine when enemy can shoot
        //After reaching position?
        //During a moment when repositioning/moving?
        //Unconstrained (whenever has target and has player in range)

        //Check conditions to set canFire to true
        //Check enum states to determine what to check for;

        if(fireType == EnemyFireType.AFTER_IN_POSITION) 
        {
            inPositionAlpha = 1;
            canFire = inPosition;
        }

        if (fireType == EnemyFireType.DURING_MOVE_ONCE)
        {
            inPositionAlpha = 0.5f;
            canFire = inPosition;
        }

        if(fireType == EnemyFireType.UNCONSTRAINED) 
        {
            canFire = true;
        }
        
        //Movement logic
        if (!hasPath)
        {
            //Using the last previous known target position of the player

            if(fireType == EnemyFireType.AFTER_IN_POSITION && hasAttacked) 
            {
                CalculateNewPath();
            }
            else if(fireType != EnemyFireType.AFTER_IN_POSITION)
            {
                CalculateNewPath();
            }
        }

        if (hasPath)
        {
            MoveToPosition(inPositionAlpha);
        }

        //Attacking Logic
        if (canFire)
        {
            FireAtPlayer();
        }
    }

    private void MoveToPosition(float positionReached = 1.0f) 
    {
        //Calculate how far along agent is
        if (!inPosition)
        {
            //agent.SetPath(path);

            //Normalize distance 
            float totalDistance = Vector3.Distance(pathOrigin, pathDestination);
            float distanceTraveled = Vector3.Distance(pathOrigin, agent.transform.position);

            float roughProgress = Mathf.Clamp01(distanceTraveled / totalDistance);

            if (roughProgress >= positionReached)
            {
                inPosition = true;
            }
        }

        if (agent.remainingDistance <= agent.stoppingDistance + .1f)
        {
            Debug.Log("Reached Destination");
            hasPath = false;
            inPosition = true;
        }
    }

    private void CalculateNewPath() 
    {
        //Temp vars
        float maxAngle = 75;
        float minDistance = 2;
        float maxDistance = 8;

        Vector3 nextPosition = CalculateTargetPosition(targetPosition, maxAngle, minDistance, maxDistance);
        if (agent.CalculatePath(nextPosition, path) && !hasPath)
        {
            agent.SetPath(path);
            pathOrigin = agent.transform.position;
            pathDestination = path.corners[path.corners.Length - 1];

            //Set all relevant flags
            hasPath = true;
            inPosition = false;
            hasAttacked = false;
        }
    }

    private Vector3 CalculateTargetPosition(Vector3 target, float maxAngle, float minDistance, float maxDistance) 
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

        float distanceToTarget = Vector3.Distance(target, start);

        //If distance is to small, make forward movement smaller and maybe even have it go back a little bit
        float maintainMinDistance = 6;

        //TEMP - have it scale proportionally 
        if(distanceToTarget < maintainMinDistance) 
        {
            minDistance = -(maxDistance / 2);
            maxDistance = -1;
        }

        // Random rotation within cone
        Quaternion randomRotation = Quaternion.Euler(
            Random.Range(-maxAngle, maxAngle),
            Random.Range(-maxAngle, maxAngle),
            0
        );

        Vector3 randomDirection = randomRotation * directionToTarget;
        float randomDistance = Random.Range(minDistance, maxDistance);

        return start + (randomDirection * randomDistance);
    }

    private void FireAtPlayer()
    {
        if (Vector2.Distance(new Vector2(playerTarget.position.x, playerTarget.position.z), new Vector2(transform.position.x, transform.position.z)) < firingRadius)
        {
            if (!isAwareOfPlayer || !canTarget) return;

            //Take a snapshot and set a targetPosition
            //Take random position in radius of the player
            //Lock this target position in and don't take new target until this has been handled completely
            //Select targetPosition
            if (!trajectoryController.HasTarget && !isReloading)
            {
                //Debug.Log("Selected new Target");
                Vector2 randomFactor = Random.insideUnitCircle * randomTargetRadius;
                targetPosition = playerTarget.position + new Vector3(randomFactor.x, 0, randomFactor.y);

                //Set preview target
                trajectoryController.SetTarget(targetPosition);
            }

            if (!Physics.Linecast(muzzleTransform.position, playerTarget.position, LayerMask.GetMask("Ground")))
            {
                //Direct Shot!
                if (trajectoryController.HasTrajectory)
                {
                    if (!isReloading && targetingController.OnTarget && curveController.ReachedDesiredHeight)
                    {
                        Fire();
                        ResetFire();
                    }
                }
                else
                {
                    Debug.Log("Find trajectory when having direct sight");
                    trajectoryController.FindTrajectory(lowAngleHeight);
                }
            }
            else
            {
                if (trajectoryController.HasTrajectory)
                {
                    if (!isReloading && targetingController.OnTarget && curveController.ReachedDesiredHeight)
                    {
                        Fire();
                        ResetFire();

                    }
                }
                else
                {
                    Debug.Log("Find trajectory when having no direct sight");
                    trajectoryController.FindTrajectory(neutralAngleHeight);
                }
            }
        }
    }

    private void ResetFire() 
    {
        trajectoryController.ResetTrajectory();
        targetingTimer = targetingCooldown;
        canTarget = false;

        isReloading = true;
        hasAttacked = true;
        canFire = false;
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
            projectileComponent.InitProjectile(muzzleTransform.position, targetTransform.position, curveController.transform.position, projectileTravelTime, projectileDamage, pool);
        }
        else
        {
            Debug.LogError("Couldn't get Projectile component on: " + newProjectile.name);
        }

        //isReloading = true;
        //hasAttacked = true;
        //canFire = false;
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
