using Ami.BroAudio;
using System;
using System.Collections.Generic;
using UnityEngine;

//TODO: Handles the animation of the sonar Pulse & Sonar detection spikes
public class SonarEffectAnimator : MonoBehaviour
{
    [SerializeField] private CameraEvents cameraEvents;
    [SerializeField] private SoundConfig soundConfig;

    [SerializeField] private Material sonarMaterial;
    [SerializeField] private Material sonarOccludedMaterial;

    [SerializeField] private MeshRenderer sonarMeshRenderer;
    [SerializeField] private MeshRenderer sonarOccludedMeshRenderer;
    private MaterialPropertyBlock sonarPropertyBlock;

    [SerializeField] private Material sonarGridMaterial;
    [SerializeField] private Transform sonarRingTransform;
    [SerializeField] private Transform targetTransform;
    private SonarCollider sonarCollider;

    [SerializeField] private AnimationCurve expandCurve;
    [SerializeField] private AnimationCurve collapseCurve;

    [SerializeField] private float maxRange;
    [SerializeField] private float minRange;

    [SerializeField] private float timeToExpand;
    [SerializeField] private float timeToCollapse;
    [SerializeField] private float scanningCooldown;

    [SerializeField] private bool isPulse;

    [SerializeField] private Vector2 minSpike = new Vector2(0.25f, 0.6f);
    [SerializeField] private Vector2 maxSpike = new Vector2(0.45f, 2.6f);

    [ColorUsage(true, true)]
    [SerializeField] Color[] spikeColours = new Color[4];

    private float expandTimer;
    private float collapseTimer;
    private float cooldownTimer;

    private bool canScan = true;
    private bool isScanning = false;
    private bool finishedAnimation = false;

    private int detectedIndex = 0;

    private Vector4[] detections;
    private Dictionary<int, DetectionData> detectionDataDict;

    private float[] spikesColorIndex;

    public EventHandler cycleFinishEvent;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Start()
    {
        sonarCollider = GetComponentInChildren<SonarCollider>();
        detectionDataDict = new Dictionary<int, DetectionData>();

        int maxDetections = 8; //Set in the SonarController this is TEMP
        spikesColorIndex = new float[maxDetections];
        detections = new Vector4[maxDetections];

        sonarMaterial.SetColorArray("_SpikeColours", spikeColours);
        sonarOccludedMaterial.SetColorArray("_SpikeColours", spikeColours);

        sonarPropertyBlock = new MaterialPropertyBlock();

        cameraEvents.cameraZoomChangedEvent += CameraZoomChanged;
    }

    // Update is called once per frame
    private void Update()
    {
        Follow();

        //Update renderers
        sonarMeshRenderer.SetPropertyBlock(sonarPropertyBlock);
        sonarOccludedMeshRenderer.SetPropertyBlock(sonarPropertyBlock);

        if (isPulse)
        {
            if (!isScanning & canScan)
            {
                BroAudio.Play(soundConfig.sonarScanStart, transform.position);
                
                finishedAnimation = false;
                isScanning = true;
                canScan = false;
            }
        }

        if (isScanning)
        {
            Animate(Time.deltaTime);
        }

        if (cooldownTimer < scanningCooldown && isScanning && expandTimer >= timeToExpand)
        {
            cooldownTimer += Time.deltaTime;

            if (cooldownTimer > scanningCooldown)
            {
                ResetScan();
            }
        }
    }

    private void CameraZoomChanged(object sender, float zoom)
    {
        sonarPropertyBlock.SetFloat("_Zoom", zoom);
    }

    private void Follow()
    {
        sonarMaterial.SetVector("_Center", targetTransform.position);
        sonarPropertyBlock.SetVector("_Center", targetTransform.position);
        sonarGridMaterial.SetVector("_Grid_Center", targetTransform.position);
    }

    private void Animate(float deltaTime)
    {
        if(expandTimer < timeToExpand) 
        {
            expandTimer += deltaTime;

            float expandAlpha = (expandTimer - 0) / (timeToExpand - 0);
            float scannerRadius = Mathf.Lerp(0, maxRange, expandCurve.Evaluate(expandAlpha));

            sonarGridMaterial.SetFloat("_ScanEffectRadius", scannerRadius);
        }

        if (collapseTimer < timeToCollapse && expandTimer >= timeToExpand)
        {
            collapseTimer += deltaTime;

            float collapseAlpha = (collapseTimer - 0) / (timeToCollapse - 0);
            float scannerRadius = Mathf.Lerp(0, maxRange, collapseCurve.Evaluate(collapseAlpha));

            sonarGridMaterial.SetFloat("_ScanEffectRadius", maxRange - scannerRadius);

            //Expand Radar Collider
            sonarCollider.ColliderEnable(true);
            sonarCollider.UpdateCollider(scannerRadius);
        }
        else if(collapseTimer >= timeToCollapse)
        {
            sonarCollider.ColliderEnable(false);
            sonarCollider.UpdateCollider(0);

            sonarPropertyBlock.SetInt("_SpikeCount", detectedIndex);
            sonarPropertyBlock.SetVectorArray("_SpikeData", detections);
            sonarPropertyBlock.SetFloatArray("_SpikesColourIndex", spikesColorIndex);

            if (!finishedAnimation) 
            {
                BroAudio.Play(soundConfig.sonarScanEnd, transform.position);
                finishedAnimation = true;
            }
        }
    }

    private void ResetScan()
    {
        detectedIndex = 0;
        detectionDataDict.Clear();

        Array.Clear(detections, 0, detections.Length);
        Array.Clear(spikesColorIndex, 0, spikesColorIndex.Length);

        expandTimer = 0;
        collapseTimer = 0;
        cooldownTimer = 0;
        isScanning = false;
        canScan = true;

        cycleFinishEvent?.Invoke(this, EventArgs.Empty);
    }

    public void UpdateSpike(Vector2 position, float normalizedDistance, DetectionData data, int colorIndex) 
    {
        float spikeHeight = Mathf.Lerp(maxSpike.y, minSpike.y, normalizedDistance);
        float spikeWidth = Mathf.Lerp(maxSpike.x, minSpike.x, normalizedDistance);

        detections[detectedIndex] = new Vector4(data.position.x, data.position.z, spikeWidth, spikeHeight);
        detectionDataDict[detectedIndex] = data;
        spikesColorIndex[detectedIndex] = colorIndex;

        detectedIndex++;
    }
}
