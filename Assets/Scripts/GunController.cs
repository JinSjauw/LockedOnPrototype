using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;

public class GunController : MonoBehaviour
{
    [Header("Component Refs")]
    [SerializeField] private ObjectPool pool;
    [SerializeField] private Transform projectileCollection;

    [Header("Trajectory Refs")]
    [SerializeField] private Transform muzzleTransform;
    [SerializeField] private Transform targetTransform;
    [SerializeField] private Transform curveControlTransform;

    [Header("Projectile Variables")]
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private float travelTime;

    #region Input Handling

    void OnFire(InputValue value) 
    {
        if (value.isPressed) 
        {
            FireProjectile();
        }
    }

    #endregion

    #region Unity

    private void Awake()
    {
        pool =  FindFirstObjectByType<ObjectPool>();
    }

    #endregion

    #region Functions

    private void FireProjectile()
    {
        GameObject newProjectile = pool.GetObject(projectilePrefab);
        newProjectile.transform.position = muzzleTransform.position;
        newProjectile.transform.forward = muzzleTransform.forward;
        newProjectile.transform.parent = projectileCollection;

        if (newProjectile.TryGetComponent(out Projectile projectileComponent))
        {
            projectileComponent.InitProjectile(muzzleTransform.position, targetTransform.position, curveControlTransform.position, travelTime, pool);
        }
        else 
        {
            Debug.LogError("Couldn't get Projectile component on: " + newProjectile.name);
        }
    }


    #endregion

}
