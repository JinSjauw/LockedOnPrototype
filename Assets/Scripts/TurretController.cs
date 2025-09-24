using System;
using System.Collections;
using UnityEngine;

public class TurretController : MonoBehaviour
{
    [Header("Temporary Refs")]
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] public Transform projectileCollectionParent;

    [Header("Components")]
    [SerializeField] private Transform turretTransform;
    [SerializeField] private Transform barrelTransform;
    [SerializeField] private Transform targetTransform;
    [SerializeField] private Transform barrelTargetTransform;

    [Header("Variables")]
    [SerializeField] private float muzzleVelocity = 20;

    void Start()
    {
        StartCoroutine(FireBullet());
    }

    private void Update()
    {
        RotateGun();
    }

    private IEnumerator FireBullet()
    {
        while (true)
        {
            //Create a new bullet
            GameObject newBullet = Instantiate(projectilePrefab, barrelTransform.position, barrelTransform.rotation) as GameObject;

            if(newBullet.TryGetComponent(out Projectile projectile)) 
            {
                //Base travel time on range from origin to target.
                float travelTime = 2;

                projectile.InitProjectile(barrelTransform.position, targetTransform.position, barrelTargetTransform.position, travelTime);
            }

            //Parent it to get a less messy workspace
            newBullet.transform.parent = projectileCollectionParent;

            yield return new WaitForSeconds(2f);
        }
    }

    private void RotateGun()
    {
        ////decreases from 360 degress when we are rotating up
        turretTransform.LookAt(targetTransform);
        turretTransform.eulerAngles = new Vector3(0f, turretTransform.rotation.eulerAngles.y, 0f);
        barrelTransform.LookAt(barrelTargetTransform);
    }
}
