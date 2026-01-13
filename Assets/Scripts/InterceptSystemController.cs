using Ami.BroAudio;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class InterceptSystemController : MonoBehaviour
{
    [SerializeField] private UIEvents uiEvents;
    [SerializeField] private ActiveSensingChannel activeSensingChannel;
    [SerializeField] private float timeToLock;
    [SerializeField] private float beepInterval;

    [SerializeField] private int ammoAmount;
    [SerializeField] private float timeToRecharge;
    private float rechargeTimer;
    private bool canFire;

    [SerializeField] private SoundConfig soundConfig;

    private Camera mainCamera;
    private Vector2 mousePosition;

    private bool hasTarget;
    private bool targetLocked;
    private float lockOnTimer;
    private float beepIntervalTimer;
    private int beepCount;

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
        beepIntervalTimer = beepInterval;
        uiEvents.OnInterceptGunAmmoChanged(ammoAmount);
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
            else if(currentTarget == hitProjectile && !targetLocked)
            {
                LockOn(currentTarget);
            }
        }
        else 
        {
            ResetIntercept();
        }

        if(!canFire && rechargeTimer < timeToRecharge) 
        {
            rechargeTimer += Time.deltaTime;

            float rechargeAlpha = (rechargeTimer - 0) / (timeToRecharge - 0);
            uiEvents.OnReloadInterceptGun(rechargeAlpha);
        }
        else if(!canFire && rechargeTimer > timeToRecharge) 
        {
            rechargeTimer = 0;
            canFire = true;
        }
    }

    private void LockOn(Projectile target)
    {
        //Run Timer
        if (lockOnTimer < timeToLock)
        {
            lockOnTimer += Time.deltaTime;

            if(beepIntervalTimer < beepInterval) 
            {
                beepIntervalTimer += Time.deltaTime;
            }
            else 
            {
                beepIntervalTimer = 0;

                if(beepCount == 0) 
                {
                    target.EnableLockMarks(0);
                }

                beepCount++;

                BroAudio.Play(soundConfig.interceptBeepSound);
            }
        }
        else if(!targetLocked)
        {
            targetLocked = true;
            lockedTarget = target;

            target.EnableLockMarks(1);

            BroAudio.Play(soundConfig.interceptLockSound);
        }
    }

    private void FireIntercept()
    {
        if (targetLocked && ammoAmount > 0) 
        {
            BroAudio.Play(soundConfig.autoCannonSound, transform.position);

            ammoAmount--;
            canFire = false;
            uiEvents.OnInterceptGunAmmoChanged(ammoAmount);

            lockedTarget.ExplodeProjectile();
            ResetIntercept();
        }
    }

    private void ResetIntercept() 
    {
        BroAudio.Stop(soundConfig.interceptBeepSound, 0.1f);
        BroAudio.Stop(soundConfig.interceptLockSound, 0.1f);

        beepIntervalTimer = beepInterval;

        beepCount = 0;

        lockedTarget = null;
        targetLocked = false;
        lockOnTimer = 0;
        hasTarget = false;
    }
}
