using Ami.BroAudio;
using BTreeEditor;
using FischlWorks_FogWar;
using System;
using System.Collections.Generic;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

[RequireComponent(typeof(EnemyBlackBoard))]
public class EnemyController : MonoBehaviour
{
    //Handled when spawned in dynamically
    [SerializeField] private string enemySignID;

    [SerializeField] private float fieldOfSight;
    [SerializeField] private float firingRadius;
    [SerializeField] private float detectionRadius;
    [SerializeField] private float afterFiringDelay;
    private float afterFiringTimer;

    [Header("Pathfinding Variables")]
    [SerializeField] private float maxConeAngle;
    [SerializeField] private float minDistance;
    [SerializeField] private float maxDistance;
    [SerializeField] private float maintainDistance;
    [SerializeField] private float minimumPadding = 0.15f;


    [SerializeField] private Transform playerTarget;
    [SerializeField] private EnemyTargetingController targetingController;
    [SerializeField] private EnemyTrajectoryController trajectoryController;
    [SerializeField] private SoundConfig soundConfig;

    [Header("Temp Refs")]

    [Header("Navmesh Ref - TEMP")]
    [SerializeField] private NavMeshAgent agent;
    private NavMeshPath path;

    private Vector3 pathOrigin;
    private Vector3 pathDestination;

    [SerializeField] private bool canMove = false;
    [SerializeField] private bool hasPath = false;
    [SerializeField] private bool inPosition = true;

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
    [SerializeField] private ProjectileVariables projectileVariables;
    [SerializeField] private Transform hitDebug;
    [SerializeField] private Transform projectileCollection;
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private float projectileTravelTime;
    [SerializeField] private float projectileDamage;
    [SerializeField] private float roundPerMinute;

    private float targetingCooldown = 1.5f;
    private float targetingTimer;
    private bool canTarget = false;

    private float firingTimer;
    private bool isReloading = false;
    private bool isAwareOfPlayer = true;

    private ObjectPool pool;
    private csFogWar fogWar;

    public EventHandler<EnemyController> OnDestructionEvent;

    public void InitializeEnemy(Transform playerTransform, Vector3 spawnPosition, string ID)
    {
        playerTarget = playerTransform;
        transform.position = spawnPosition;
        enemySignID = ID;

        this.enabled = true;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        path = new NavMeshPath();
        pool = FindFirstObjectByType<ObjectPool>();
    }

    // Update is called once per frame
    void Update()
    {
        //CalculateTargetPosition(playerTarget.transform.position, maxConeAngle, minDistance, maxDistance);
        //return;

        if (playerTarget == null) return;

        //AttackBehaviour();

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

        if(canMove && afterFiringTimer < afterFiringDelay) 
        {
            //Debug.Log(afterFiringTimer);
            afterFiringTimer += Time.deltaTime;
            return;
        }
        else if(afterFiringTimer >= afterFiringDelay) 
        {
            afterFiringTimer = 0;
        }

        if (fireType == EnemyFireType.AFTER_IN_POSITION)
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
            inPositionAlpha = 1;
            canFire = true;
        }
        
        //Movement logic
        if (!hasPath)
        {
            //Using the last previous known target position of the player

            if(fireType == EnemyFireType.AFTER_IN_POSITION && canMove) 
            {
                CalculateNewPathToTarget();
            }
            else if(fireType != EnemyFireType.AFTER_IN_POSITION)
            {
                CalculateNewPathToTarget();
            }
        }
        else if (hasPath)
        {
            //MoveToPosition();
        }

        //Attacking Logic
        if (canFire)
        {
            //Debug.Log("Entering Fire Logic: " + gameObject.name);
            FireAtPlayer();
        }
    }

    public bool DetectedEnemy() 
    {
        float distanceToTarget = Vector2.Distance(new Vector2(playerTarget.position.x, playerTarget.position.z), new Vector2(agent.transform.position.x, agent.transform.position.z));

        if (distanceToTarget <= firingRadius)
        {
            //Player Detected!
            Debug.Log("Detected Player!");

            agent.isStopped = true;
            return true;
        }

        return false;
    }

    public void MoveToDestination(ref NodeState state) 
    {
        float distanceToTarget = Vector2.Distance(new Vector2(playerTarget.position.x, playerTarget.position.z), new Vector2(agent.transform.position.x, agent.transform.position.z));

        if (distanceToTarget <= firingRadius)
        {
            //Player Detected!
            Debug.Log("Detected Player while Moving!");
            state = NodeState.FAILURE;

            agent.isStopped = true;
        }
        else 
        {
            //agent.SetPath(path);
            Debug.Log("Moving!");
            //Normalize distance 
            float totalDistance = Vector3.Distance(pathOrigin, pathDestination);
            float distanceTraveled = Vector3.Distance(pathOrigin, agent.transform.position);

            float roughProgress = Mathf.Clamp01(distanceTraveled / totalDistance);

            if (agent.remainingDistance <= agent.stoppingDistance + .1f)
            {

                agent.isStopped = true;

                state = NodeState.SUCCESS;
            }
            else
            {
                state = NodeState.RUNNING;
            }
        }
    }

    //[BTreeMethod(MethodID.SELECT_NEXT_PATROL_POINT)]
    public bool SetPatrolPoint(BlackBoard blackBoard)
    {
        Debug.Log("Setting New Point!");
        //Get new patrol point
        List<Transform> patrolPoints = (List<Transform>)blackBoard.GetField(BlackBoardFields.PATROL_POINTS_TRANSFORMS_LIST);

        int randomIndex = Random.Range(0, patrolPoints.Count - 1);

        Vector3 nextPosition = patrolPoints[randomIndex].position;
        if (agent.CalculatePath(nextPosition, path))
        {
            agent.isStopped = false;
            agent.SetPath(path);
            pathOrigin = agent.transform.position;
            pathDestination = path.corners[path.corners.Length - 1];

            //Set all relevant flags
            hasPath = true;
            canMove = false;

            return true;
        }

        return false;
    }

    private void CalculateNewPathToTarget() 
    {
        inPosition = false;

        Vector3 nextPosition = CalculateTargetPosition(targetPosition, maxConeAngle, minDistance, maxDistance);
        if (agent.CalculatePath(nextPosition, path) && !hasPath)
        {
            agent.SetPath(path);
            pathOrigin = agent.transform.position;
            pathDestination = path.corners[path.corners.Length - 1];

            //Set all relevant flags
            hasPath = true;
            canMove = false;
        }
    }

    private Vector3 CalculateTargetPosition(Vector3 target, float maxAngle, float minDistance, float maxDistance) 
    {
        //For now choose random new position. within attack range

        Vector3 origin = agent.transform.position;

        Vector3 directionToTarget = (target - origin).normalized;

        float distanceToTarget = Vector3.Distance(target, origin);

        //If distance is to small, make forward movement smaller and maybe even have it go back a little bit
        
        //Normalize the value.
        float distanceAlpha = (distanceToTarget - maintainDistance) / (detectionRadius - maintainDistance);

        if(fireType != EnemyFireType.UNCONSTRAINED) 
        {
            distanceAlpha = distanceAlpha * 2 - 1;
        }

        float distancePadding = distanceAlpha > 0 ? minimumPadding : -minimumPadding;

        // Random rotation within cone
        Quaternion randomRotation = Quaternion.Euler(
            Random.Range(-maxAngle, maxAngle),
            Random.Range(-maxAngle, maxAngle),
            0
        );

        Vector3 randomDirection = randomRotation * directionToTarget;

        float randomDistance = Random.Range(minDistance * (distanceAlpha + distancePadding), maxDistance * (distanceAlpha + distancePadding));

        //Debug.Log("Name: " + gameObject.name + "Distance: " + distanceToTarget + " Distance Alpha " + distanceAlpha + " Random Distance: " + randomDistance + " Result: " + origin + (randomDirection * randomDistance));
        return origin + (randomDirection.normalized * randomDistance);
    }

    private void FireAtPlayer()
    {
        float distanceToTarget = Vector2.Distance(new Vector2(playerTarget.position.x, playerTarget.position.z), new Vector2(agent.transform.position.x, agent.transform.position.z));

        if (distanceToTarget <= firingRadius)
        {
            //Debug.Log("Entering Fire Logic 1: " + gameObject.name);

            if (!isAwareOfPlayer || !canTarget) return;

            bool directLineOfSight = !Physics.Linecast(muzzleTransform.position, playerTarget.position, LayerMask.GetMask("Ground"));

            //Take a snapshot and set a targetPosition
            //Take random position in radius of the player
            //Lock this target position in and don't take new target until this has been handled completely
            //Select targetPosition
            if (!trajectoryController.HasTarget && !isReloading)
            {
                //Debug.Log("Selected new Target");
                Vector2 randomFactor = Random.insideUnitCircle * randomTargetRadius;

                Vector3 overshootDirection = playerTarget.position - agent.transform.position;

                float overshootFactor = directLineOfSight ? 2.2f : .75f;

                Vector3 randomPosition = playerTarget.position + (overshootDirection.normalized * overshootFactor ) + new Vector3(randomFactor.x, 0, randomFactor.y);

                //Make target stick to ground
                if (Physics.Raycast(randomPosition, Vector3.down, out RaycastHit hit, 100, LayerMask.GetMask("Ground"))) 
                {
                    randomPosition = hit.point;
                }

                targetPosition = randomPosition;

                //Set preview target
                trajectoryController.SetTarget(targetPosition);
            }

            if (trajectoryController.HasFailed) 
            {
                canMove = true;
                trajectoryController.ResetTries();
            }
            else if (directLineOfSight)
            {
                //Direct Shot!
                if (trajectoryController.HasTrajectory)
                {
                    if (!isReloading && targetingController.OnTarget && curveController.ReachedDesiredHeight)
                    {
                        Fire();
                    }
                }
                else
                {
                    //Debug.Log("Find trajectory when having direct sight");
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
                    }
                }
                else
                {
                    //Debug.Log("Find trajectory when having no direct sight");
                    trajectoryController.FindTrajectory(neutralAngleHeight);
                }
            }
        }
        else if (distanceToTarget <= detectionRadius && distanceToTarget > firingRadius)
        {
            //Move if in detection range
            targetPosition = playerTarget.position;
            canMove = true;
        }

        //Reset so agent continues
        //ResetFire();
    }
    private void ResetFire() 
    {
        trajectoryController.ResetTrajectory();
        targetingTimer = targetingCooldown;
        canTarget = false;

        isReloading = true;
        canMove = true;
        canFire = false;
    }
    private void Fire() 
    {
        //Debug.Log("Fire!");

        GameObject newProjectile = pool.GetObject(projectilePrefab);
        newProjectile.transform.position = muzzleTransform.position;
        newProjectile.transform.forward = muzzleTransform.forward;
        newProjectile.transform.parent = projectileCollection;

        if (newProjectile.TryGetComponent(out Projectile projectileComponent))
        {
            projectileComponent.InitProjectile(muzzleTransform.position, targetTransform.position, curveController.transform.position, CalculateTravelTime(), projectileDamage, pool, soundConfig, fogWar, false);
        }
        else
        {
            Debug.LogError("Couldn't get Projectile component on: " + newProjectile.name);
        }

        SoundID soundToPlay = soundConfig.railgunSound;

        if(fireType == EnemyFireType.AFTER_IN_POSITION) 
        {
            soundToPlay = soundConfig.artillerySound;
        }

        BroAudio.Play(soundToPlay, muzzleTransform.position);

        ResetFire();
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
    private float CalculateTravelTime()
    {
        float distance = Vector3.Distance(muzzleTransform.position, targetTransform.position);
        float result = projectileVariables.GetTravelTime(distance, curveController.DesiredCurveHeight);

        return result;
    }

    public string GetID() 
    {
        return enemySignID;
    }

    public void Die()
    {
        BroAudio.Play(soundConfig.destructionSound, agent.transform.position);
        //Play explosion VFX here
        //Blow up turret?
        //Body falls down?
        agent.isStopped = true;
        //this.enabled = false;
        OnDestructionEvent?.Invoke(this, this);
        //OnDestructionEvent = null;

        //TEMP Disable the enemyController
        //FUTURE spawn corpse
        this.enabled = false;

        //pool.ReturnGameObject(gameObject);
    }
}
