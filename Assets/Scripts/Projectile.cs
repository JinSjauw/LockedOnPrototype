using UnityEngine;

public class Projectile : MonoBehaviour
{
    private Vector3 origin;
    private Vector3 target;
    private Vector3 control;

    private float travelTime;
    private float timer = 0f;

    #region Unity

    // Update is called once per frame
    void Update()
    {
        MoveProjectile();
    }

    #endregion

    #region Functions

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
            if(Physics.Linecast(currentPosition, newPosition)) 
            {
                Debug.Log("Premature Kabooshh!");
                Destroy(gameObject);
                return;
            }

            //Calculate new forward
            Vector3 newForward = (newPosition - currentPosition).normalized;

            transform.forward = newForward;
            transform.position = newPosition;   
        }
        else 
        {
            Debug.Log("Kabooshh!");
            Destroy(gameObject);
        }
    }

    public void InitProjectile(Vector3 origin, Vector3 target, Vector3 control, float travelTime) 
    {
        this.origin = origin;
        this.target = target;
        this.control = control;
        this.travelTime = travelTime;
    }

    #endregion
}
