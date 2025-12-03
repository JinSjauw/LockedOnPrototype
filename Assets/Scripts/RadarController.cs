using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class RadarController : MonoBehaviour
{
    [SerializeField] private Material scannerMaterial;
    [SerializeField] private Transform targetTransform;
    private SonarCollider radarCollider;

    [SerializeField] private AnimationCurve expansionCurve;
    [SerializeField] private AnimationCurve fadeCurve;

    [SerializeField] private float maxRange;
    [SerializeField] private float timeToEnd;
    [SerializeField] private float timeToFade;
    [SerializeField] private float scanningCooldown;
    
    [SerializeField] private bool canPulse;

    private float expansionTimer;
    private float fadeTimer;
    private float cooldownTimer;

    private bool canScan = true;
    private bool isScanning = false;

    List<String> detectedID;

    #region Input

    private void OnRadarScan(InputValue value) 
    {
        if (value.isPressed) 
        {
            if (!canScan || canPulse) return;

            //Follow();
            isScanning = true;
            canScan = false;
        }
    }

    #endregion

    #region Unity
    private void Awake()
    {
        detectedID = new List<String>();
        radarCollider = GetComponentInChildren<SonarCollider>();
    }

    private void Start()
    {
        //radarCollider.OnSonarEnter += AddDetection;
    }

    private void AddDetection(object sender, string ID)
    {
        if (detectedID.Contains(ID)) return;

        Debug.Log("Detected & Added： " + ID);
        detectedID.Add(ID);
    }

    private void Update()
    {
        Follow();

        if (canPulse) 
        {
            if(!isScanning & canScan) 
            {
                isScanning = true;
                canScan = false;
            }
        }

        if (isScanning) 
        {
            Expand(Time.deltaTime);
            //Fade(Time.deltaTime);
        }

        if (cooldownTimer < scanningCooldown && isScanning) 
        {
            cooldownTimer += Time.deltaTime;

            if(cooldownTimer > scanningCooldown) 
            {
                ResetScan();
            }
        }
    }

    #endregion

    #region Functions

    private void Follow() 
    {
        //scannerMaterial.SetVector("_Terrain_Scan_Origin", targetTransform.position);
        scannerMaterial.SetVector("_Grid_Center", targetTransform.position);

        //Radar collider follow
        radarCollider.transform.position = targetTransform.position;

    }

    private void Expand(float deltaTime) 
    {
        if(expansionTimer < timeToEnd) 
        {
            expansionTimer += deltaTime;

            float expansionTimer01 = (expansionTimer - 0) / (timeToEnd - 0);
            float scannerRadius = Mathf.Lerp(0, maxRange, expansionCurve.Evaluate(expansionTimer01));

            //scannerMaterial.SetFloat("_Terrain_Scan_Range", scannerRadius);
            scannerMaterial.SetFloat("_ScanEffectRadius", scannerRadius);

            //Expand Radar Collider
            radarCollider.ColliderEnable(true);
            radarCollider.UpdateCollider(scannerRadius);
        }
        else 
        {
            radarCollider.ColliderEnable(false);
            radarCollider.UpdateCollider(0);
        }
    }

    private void Fade(float deltaTime) 
    {
        if (fadeTimer < timeToFade)
        {
            fadeTimer += deltaTime;

            float fadeTimer01 = (fadeTimer - 0) / (timeToFade - 0);
            float scannerVisibility = Mathf.Lerp(1, 0, fadeCurve.Evaluate(fadeTimer01));

            scannerMaterial.SetFloat("_Global_Visibility", scannerVisibility);
        }
    }

    private void ResetScan()
    {
        detectedID.Clear();

        expansionTimer = 0;
        fadeTimer = 0;
        cooldownTimer = 0;
        isScanning = false;
        canScan = true;
    }

    #endregion
}
