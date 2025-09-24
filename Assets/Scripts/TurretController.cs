using System;
using System.Collections;
using UnityEngine;

public class TurretController : MonoBehaviour
{
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] public Transform projectileCollectionParent;

    [SerializeField] private Transform turretTransform;
    [SerializeField] private Transform barrelTransform;
    [SerializeField] private Transform impactTransform;

    [SerializeField] private float muzzleVelocity = 20;

    static float h = 2;

    private LineRenderer lineRenderer;

    void Awake()
    {
        //Can use a less precise h to speed up calculations
        //Or a more precise to get a more accurate result
        //But lower is not always better because of rounding errors
        h = Time.fixedDeltaTime * 1f;

        lineRenderer = GetComponent<LineRenderer>();
    }

    void Start()
    {
        StartCoroutine(FireBullet());
    }

    private void FixedUpdate()
    {
        //RotateGun();
        //SimulateTrajectory(impactTransform.position, true);
    }

    private IEnumerator FireBullet()
    {
        while (true)
        {
            //Create a new bullet
            GameObject newBullet = Instantiate(projectilePrefab, barrelTransform.position, barrelTransform.rotation) as GameObject;

            //Parent it to get a less messy workspace
            newBullet.transform.parent = projectileCollectionParent;

            //Add velocity to the bullet with a rigidbody
            newBullet.GetComponent<Projectile>().currentVelocity = muzzleVelocity * barrelTransform.forward;

            yield return new WaitForSeconds(2f);
        }
    }

    //private void RotateGun() 
    //{
    //    float highAngle = 0f;
    //    float lowAngle = 0f;

    //    bool hasAngle = Ballistics.CalculateAngleToHitTarget(out highAngle, out lowAngle, muzzleVelocity, barrelTransform.position, impactTransform.position);

    //    if (!hasAngle) return;

    //    //If there is direct line of sight use lowAngle

    //    //Artillery
    //    float angle = (float)highAngle;
    //    //Regular gun
    //    //float angle = (float)lowAngle;
    //    //Debug.Log(angle);

    //    //If we are within range

    //    //The equation we use assumes that if we are rotating the gun up from the
    //    //pointing "forward" position, the angle increase from 0, but our gun's angles
    //    //decreases from 360 degress when we are rotating up
    //    turretTransform.LookAt(impactTransform);
    //    turretTransform.eulerAngles = new Vector3(0f, turretTransform.rotation.eulerAngles.y, 0f);

    //    barrelTransform.localEulerAngles = new Vector3(360f - angle, 0f, 0f);
    //}

    ////Display the trajectory path with a line renderer
    //private Vector3 SimulateTrajectory(Vector3 targetPosition, bool draw = false)
    //{
    //    //How long did it take to hit the target?
    //    float timeToHitTarget = Ballistics.CalculateTimeToHitTarget(muzzleVelocity, barrelTransform, targetPosition, h);
    //    Debug.Log(timeToHitTarget);
    //    //How many segments we will have
    //    int maxIndex = Mathf.RoundToInt(timeToHitTarget / h);

    //    if (draw) 
    //    { 
    //        lineRenderer.positionCount = maxIndex; 
    //    }

    //    //Start values

    //    Vector3 currentVelocity = barrelTransform.forward * muzzleVelocity;
    //    Vector3 currentPosition = barrelTransform.position;
        
    //    Vector3 newPosition = Vector3.zero;
    //    Vector3 newVelocity = Vector3.zero;

    //    //Build the trajectory line
    //    for (int index = 0; index < maxIndex; index++)
    //    {
    //        if (draw) 
    //        {
    //            lineRenderer.SetPosition(index, currentPosition);
    //        }

    //        //Calculate the new position of the bullet
    //        Ballistics.CurrentIntegrationMethod(h, currentPosition, currentVelocity, out newPosition, out newVelocity);

    //        currentPosition = newPosition;
    //        currentVelocity = newVelocity;
    //    }

    //    return currentPosition;
    //}
}
