using UnityEngine;

public class EnemyController : MonoBehaviour
{
    [SerializeField] private float fieldOfSight;
    [SerializeField] private float detectionRadius;
    [SerializeField] private Transform playerTarget;
    [SerializeField] private EnemyTargetingController targetingController;

    [Header("Temp Refs")]
    [Header("Trajectory Ref - TEMP")]
    [SerializeField] private Transform muzzleTransform;
    [SerializeField] private Transform targetTransform;
    [SerializeField] private Transform curveControlTransform;

    [Header("Projectile Variables - TEMP")]
    [SerializeField] private Transform projectileCollection;
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private float projectileTravelTime;
    [SerializeField] private float projectileDamage;
    [SerializeField] private float roundPerMinute;

    //private float firingDelay;
    private float firingTimer;
    private bool isReloading = false;

    private bool isAwareOfPlayer = true;
    private ObjectPool pool;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        pool = FindFirstObjectByType<ObjectPool>();
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

            if (!Physics.Linecast(transform.position, playerTarget.position, LayerMask.GetMask("Ground"))) 
            {
                //Direct Shot!

                if (!isReloading) Fire();

            }
            else
            {
                //Simulate firing trajectory until you find acceptable angle
                if (!isReloading) Fire();
            }
        }
    }

    private void FindTrajectory() 
    {
        //
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
