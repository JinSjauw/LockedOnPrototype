using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class SonarController : MonoBehaviour
{
    [SerializeField] private Material sonarMaterial;
    [SerializeField] private Transform sonarRingTransform;
    [SerializeField] private Transform targetTransform;
    private SonarCollider sonarCollider;

    [SerializeField] private AnimationCurve expansionCurve;
    [SerializeField] private AnimationCurve fadeCurve;

    [SerializeField] private float maxRange;
    [SerializeField] private float timeToEnd;
    [SerializeField] private float timeToFade;
    [SerializeField] private float scanningCooldown;

    [SerializeField] private bool isPulse;

    private float expansionTimer;
    private float fadeTimer;
    private float cooldownTimer;

    private bool canScan = true;
    private bool isScanning = false;

    private int detectedIndex = 0;
    [SerializeField] private int maxDetections = 8;

    private Vector4[] detections;
    private Dictionary<int, DetectionData> detectionDataDict;
    private List<string> lockedOnList;

    [SerializeField] private int maxLockOns = 2;

    [ColorUsage(true, true)]
    [SerializeField] Color[] bumpColours = new Color[4];


    private bool moveSonarRing;

    #region Input

    private void OnRadarScan(InputValue value)
    {
        if (value.isPressed)
        {
            if (!canScan || isPulse) return;

            //Follow();
            isScanning = true;
            canScan = false;
        }
    }

    private void OnAdjustSonarRing(InputValue value) 
    {
        if (value.isPressed) 
        {
            moveSonarRing = true;
            AdjustSonarRing();
        }
        else 
        {
            moveSonarRing = false;
        }
    }

    private void OnLockOn(InputValue value) 
    {
        if (value.isPressed) 
        {
            //Function
            //Maybe store directions?
            //Check which direction is the closest and within threshold (dot product)?
            //Locked On Objects will produce holograms

            OnLockOn();
        }
    }

    #endregion

    #region Unity
    private void Awake()
    {
        //detectedPosition = new List<Vector3>();
        sonarCollider = GetComponentInChildren<SonarCollider>();
        detectionDataDict = new Dictionary<int, DetectionData>();
        lockedOnList = new List<string>();

        detections = new Vector4[maxDetections];
        sonarMaterial.SetColorArray("_BumpColours", bumpColours);
    }

    private void Start()
    {
        sonarCollider.OnSonarEnter += OnDetectionReceive;
    }

    //If ID matches one of the Locked On ID's give it a different color!
    //Create struct to send ID & Position DATA
    private void OnDetectionReceive(object sender, DetectionData data)
    {
        //detectedPosition.Add(position);

        if (detectedIndex >= maxDetections) return;

        // Calculate based on max and min range (Closer is taller peak)

        int colorIndex = 3;

        if (lockedOnList.Contains(data.ID)) 
        {
            data.sonarDetectable.SpawnHologram(data.ID);
            colorIndex = 1; //different colors for locked 1 and locked 2?
        }

        Debug.Log(colorIndex);

        // Vector4 (posX, posZ, colorIndex, heightValue)
        detections[detectedIndex] = new Vector4(data.position.x, data.position.z, colorIndex, 1); 
        detectionDataDict[detectedIndex] = data;

        detectedIndex++;
    }

    private void Update()
    {
        Follow();
        //AdjustSonarRing();

        if (isPulse)
        {
            if (!isScanning & canScan)
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

            if (cooldownTimer > scanningCooldown)
            {
                ResetScan();
            }
        }
    }

    #endregion

    #region Functions

    private void AdjustSonarRing() 
    {
        if (moveSonarRing) 
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if(Physics.Raycast(ray, out RaycastHit hit, float.MaxValue, LayerMask.GetMask("Ground")))
            {
                Vector3 point = hit.point;
                Vector3 dir = point - sonarRingTransform.position;
                dir.y = 0;                      // keep rotation flat
                if (dir.sqrMagnitude > 0.001f)  // avoid jitter when mouse on top
                {
                    Quaternion target = Quaternion.LookRotation(dir);
                    sonarRingTransform.rotation = target; // instant snap
                 // transform.rotation = Quaternion.Slerp(transform.rotation, target, 15f * Time.deltaTime); // smooth
                }
            }
        }
    }

    private void OnLockOn()
    {
        if (maxLockOns <= lockedOnList.Count) 
        {
            Debug.Log("Lock On Capacity Full");
            return;
        }
            

        //Check the direction of the sonarRing
        Vector3 rawDirectionSonar = sonarRingTransform.forward;
        Vector2 sonarDirection = new Vector2(rawDirectionSonar.x, rawDirectionSonar.z).normalized;
        Vector3 sonarRingPosition = sonarRingTransform.position;

        float bestDot = -2f;
        int bestTargetIndex = -1;

        float minimalThreshold = .93f; 

        for (int i = 0; i < detectionDataDict.Count; i++) 
        {
            //Skip if already locked on
            if (lockedOnList.Contains(detectionDataDict[i].ID)) continue;

            Vector3 rawDirection = detectionDataDict[i].position - sonarRingPosition;
            Vector2 testDirection = new Vector2(rawDirection.x, rawDirection.z).normalized;

            float testValue = Vector2.Dot(sonarDirection, testDirection);

            if (testValue < minimalThreshold)
            {
                Debug.Log("Angle diff too large! " + testValue);
                continue;
            }

            if(testValue > bestDot) 
            {
                bestDot = testValue;
                bestTargetIndex = i;
            }
        }

        if (bestTargetIndex < 0) return;

        DetectionData bestTarget = detectionDataDict[bestTargetIndex];

        lockedOnList.Add(bestTarget.ID);

        Debug.Log("Locked On: " + bestTarget.ID);

        bestTarget.sonarDetectable.SpawnHologram(bestTarget.ID);
    }

    private void Follow()
    {
        sonarMaterial.SetVector("_Center", targetTransform.position);
        //scannerMaterial.SetVector("_Grid_Center", targetTransform.position);

        //Radar collider follow
        //sonarCollider.transform.position = targetTransform.position;
    }

    private void Expand(float deltaTime)
    {
        if (expansionTimer < timeToEnd)
        {
            expansionTimer += deltaTime;

            float expansionTimer01 = (expansionTimer - 0) / (timeToEnd - 0);
            float scannerRadius = Mathf.Lerp(0, maxRange, expansionCurve.Evaluate(expansionTimer01));

            //scannerMaterial.SetFloat("_Terrain_Scan_Range", scannerRadius);
            //scannerMaterial.SetFloat("_ScanEffectRadius", scannerRadius);

            //Expand Radar Collider
            sonarCollider.ColliderEnable(true);
            sonarCollider.UpdateCollider(scannerRadius);
        }
        else
        {
            sonarCollider.ColliderEnable(false);
            sonarCollider.UpdateCollider(0);

            sonarMaterial.SetInt("_BumpCount", detectedIndex);
            sonarMaterial.SetVectorArray("_BumpData", detections);
            //sonarMaterial.SetVectorArray("_BumpHeights", heights);
            sonarMaterial.SetFloat("_BumpWidths", 0.22f);
        }
    }

    private void Fade(float deltaTime)
    {
        if (fadeTimer < timeToFade)
        {
            fadeTimer += deltaTime;

            float fadeTimer01 = (fadeTimer - 0) / (timeToFade - 0);
            float scannerVisibility = Mathf.Lerp(1, 0, fadeCurve.Evaluate(fadeTimer01));

            //scannerMaterial.SetFloat("_Global_Visibility", scannerVisibility);
        }
    }

    private void ResetScan()
    {
        detectedIndex = 0;
        detectionDataDict.Clear();
        Array.Clear(detections, 0, detections.Length);
        //Array.Clear(heights, 0, heights.Length);

        expansionTimer = 0;
        fadeTimer = 0;
        cooldownTimer = 0;
        isScanning = false;
        canScan = true;
    }

    #endregion
}
