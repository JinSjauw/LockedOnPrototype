using FOVMapping;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    [SerializeField] private FOVAgent fovAgent;
    private ObjectPool pool;

    private Vector3 origin;
    private Vector3 target;
    private Vector3 control;

    private float travelTime;
    private float timer = 0f;

    private float damage;

    private bool isInitialized = false;

    #region Unity

    private void Awake()
    {
        //fovAgent = GetComponent<FOVAgent>();
    }

    // Update is called once per frame
    void Update()
    {
        if (!isInitialized) return;

        MoveProjectile();
    }

    #endregion

    #region Functions

    public void InitProjectile(Vector3 origin, Vector3 target, Vector3 control, float travelTime, float damage, ObjectPool pool, bool playerProjectile = false)
    {
        this.origin = origin;
        this.target = target;
        this.control = control;
        this.travelTime = travelTime;
        this.damage = damage;
        this.pool = pool;

        isInitialized = true;

        if (playerProjectile && fovAgent != null) 
        {
            fovAgent.ForceInvisible = false;
            fovAgent.disappearInFOW = false;
            fovAgent.contributeToFOV = true;
            fovAgent.SetVisible(true);
        }
        else if(fovAgent != null) 
        {
            fovAgent.contributeToFOV= false;
            fovAgent.ForceInvisible = true;
            fovAgent.disappearInFOW = true;
            fovAgent.SetVisible(false);
        }
    }

    private void MoveProjectile() 
    {
        //Normalize time between 0 and 1

        timer += Time.deltaTime;

        if(timer <= travelTime) 
        {
            float curveAlpha = (timer - 0) / (travelTime - 0);

            Vector3 currentPosition = transform.position;

            //Follow Curvature
            Vector3 newPosition = QuadraticCurve.EvaluateCurve(origin, target, control, curveAlpha);
            
            //Check if there is collision //Check if we can still animate bullet in Update() but run collision separately. Maybe invisible bullet and if that gets hit it has the animated bullet impact there aswell. (not that many projectiles anyway)
            if(Physics.Linecast(currentPosition, newPosition, out RaycastHit hit)) 
            {
                Debug.Log("Premature Kabooshh!");

                if(hit.collider.TryGetComponent(out HealthComponent health)) 
                {
                    Debug.Log("Hit soft target!");
                    health.TakeDamage(damage);
                }

                ReturnObject();

                return;
            }

            //Calculate new forward
            Vector3 newForward = (newPosition - currentPosition).normalized;

            transform.forward = newForward;
            transform.position = newPosition;   
        }
        else 
        {
            //Debug.Log("Kabooshh!");

            //Explode and do an AOE?

            ReturnObject();
        }
    }

    private void ReturnObject() 
    {
        isInitialized = false;
        timer = 0;

        if (pool != null)
        {
            this.gameObject.SetActive(false);
            pool.ReturnGameObject(this.gameObject);
        }
    }

    #endregion
}
