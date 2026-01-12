using NUnit.Framework;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class InterceptSystemController : MonoBehaviour
{
    [SerializeField] private ActiveSensingChannel activeSensingChannel;
    [SerializeField] private float timeToLock;

    private Camera mainCamera;
    private Vector2 mousePosition;

    private bool hasTarget;
    private bool targetLocked;
    [SerializeField] private float lockOnTimer;
    private Projectile currentTarget;
    private Projectile lockedTarget;

    //private List<Projectile> lockOnTargets;

    private void OnFireIntercept(InputValue value) 
    {
        if (value.isPressed) 
        {
            FireIntercept();
        }
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        mainCamera = Camera.main;
        //lockOnTargets = activeSensingChannel.VisibleProjectiles;
    }

    // Update is called once per frame
    void Update()
    {
        //Check all visible projectiles?
        //Subscribe to lockedOnEvent?
        mousePosition = Mouse.current.position.ReadValue();
        Ray ray = mainCamera.ScreenPointToRay(mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hit,  Mathf.Infinity, LayerMask.GetMask("UITarget")))
        {
            Projectile hitProjectile = hit.collider.GetComponentInParent<Projectile>();

            if (!hitProjectile) return;

            if (!hasTarget) 
            {
                currentTarget = hitProjectile;
                hasTarget = true;
            }
            else if(currentTarget == hitProjectile)
            {
                LockOn(currentTarget);
            }
        }
        else 
        {
            ResetIntercept();
        }
    }

    private void LockOn(Projectile target)
    {
        //Run Timer
        if (lockOnTimer < timeToLock)
        {
            lockOnTimer += Time.deltaTime;
        }
        else
        {
            targetLocked = true;
            lockedTarget = target;
        }
    }

    private void FireIntercept()
    {
        if (targetLocked) 
        {
            lockedTarget.ExplodeProjectile();
            ResetIntercept();
        }
    }

    private void ResetIntercept() 
    {
        lockedTarget = null;
        targetLocked = false;
        lockOnTimer = 0;
        hasTarget = false;
    }
}
