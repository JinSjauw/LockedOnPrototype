using System;
using System.Collections;
using UnityEngine;

public class TurretController : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private Transform turretTransform;
    [SerializeField] private Transform barrelTransform;
    [SerializeField] private Transform targetTransform;
    [SerializeField] private Transform barrelTargetTransform;

    private void Update()
    {
        RotateGun();
    }

    private void RotateGun()
    {
        ////decreases from 360 degress when we are rotating up
        turretTransform.LookAt(targetTransform);
        turretTransform.eulerAngles = new Vector3(0f, turretTransform.rotation.eulerAngles.y, 0f);
        barrelTransform.LookAt(barrelTargetTransform);
    }
}
