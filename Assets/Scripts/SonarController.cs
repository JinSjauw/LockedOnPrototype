using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Random = UnityEngine.Random;

public class SonarController : MonoBehaviour
{
    [SerializeField] private UIEvents uiEvents;

    [SerializeField] private Transform sonarRingTransform;
    [SerializeField] private Transform targetTransform;
    private SonarCollider sonarCollider;

    [SerializeField] private KalmanPointDrawer trackerOne;
    [SerializeField] private KalmanPointDrawer trackerTwo;

    [SerializeField] private float maxRange;
    [SerializeField] private float minRange;

    private int detectedIndex = 0;
    [SerializeField] private int maxDetections = 8;

    [ColorUsage(true, true)]
    [SerializeField] Color[] spikeColours = new Color[4];

    private DetectionData[] detectionDataArray;

    private string[] lockedOnArray;
    private HashSet<string> lockedOnSet;
    private int lockedCount = 0;

    [SerializeField] private int maxLockOns = 2;

    private SonarEffectAnimator sonarEffect;

    #region Input

    private void OnLockOn(InputValue value) 
    {
        if (value.isPressed) 
        {
            OnLock();
        }
    }

    #endregion

    #region Unity
    private void Awake()
    {
        sonarCollider = GetComponentInChildren<SonarCollider>();
        detectionDataArray = new DetectionData[maxDetections];
        lockedOnArray = new string[maxLockOns];
        lockedOnSet = new HashSet<string>();

        sonarEffect = GetComponent<SonarEffectAnimator>();
    }

    private void Start()
    {
        sonarEffect.OnCycleFinish += ResetDetections;
        sonarCollider.OnSonarEnter += OnDetectionReceive;
        uiEvents.onSonarLockButtonEvent += LockFromUI;
        uiEvents.onSonarLockLossEvent += LockLossFromUI;
    }

    #endregion

    #region Functions

    //If ID matches one of the Locked On ID's give it a different color!
    private void OnDetectionReceive(object sender, DetectionData data)
    {
        if (detectedIndex >= maxDetections) return;

        // Calculate based on max and min range (Closer is taller peak)

        int colorIndex = 3;

        if (lockedOnSet.Contains(data.ID))
        {
            Vector2 randomOffset = Random.insideUnitCircle * 1.2f;
            Vector3 randomPosition = data.position + new Vector3(randomOffset.x, 0, randomOffset.y);

            float heightOffset = 1f;
            randomPosition.y += heightOffset;

            data.sonarDetectable.SpawnTracker(data.ID, randomPosition);
            int lockIndex = Array.IndexOf(lockedOnArray, data.ID);

            colorIndex = lockIndex;

            PrintPointCloud(lockIndex, data.position, -1, randomPosition);
        }

        Vector2 centerPosition = new Vector2(transform.position.x, transform.position.z);
        Vector2 targetPosition = new Vector2(data.position.x, data.position.z);

        float distance = Vector2.Distance(centerPosition, targetPosition);
        float distanceNormalized = (distance - minRange) / (maxRange - minRange);

        sonarEffect.UpdateSpike(new Vector2(data.position.x, data.position.z), distanceNormalized, data, colorIndex);

        detectionDataArray[detectedIndex] = data;

        detectedIndex++;
    }

    //TODO: Move to dedicated pointcloud drawing/control class
    private void PrintPointCloud(int lockedIndex, Vector3 position, int colorIndex = -1, Vector3 markPosition = new Vector3()) 
    {
        if (lockedIndex == 0)
        {
            trackerOne.PrintPointCloud(position, markPosition);

            if(colorIndex > -1) trackerOne.SetPointColor(spikeColours[colorIndex]);
        }
        else if (lockedIndex == 1)
        {
            trackerTwo.PrintPointCloud(position, markPosition);

            if (colorIndex > -1) trackerTwo.SetPointColor(spikeColours[colorIndex]);
        }
    }

    private void ClearPointCloud(int lockedIndex) 
    {
        if (lockedIndex == 0)
        {
            trackerOne.StopDrawing();
        }
        else if (lockedIndex == 1)
        {
            trackerTwo.StopDrawing();
        }
    }

    private void LockFromUI(object sender, int trackIndex)
    {
        OnLock(trackIndex);
    }

    private void LockLossFromUI(object sender, int trackIndex)
    {
        OnLockLoss(trackIndex);
    }

    private void OnLockLoss(int trackIndex)
    {
        if(lockedOnArray.Length > trackIndex && lockedCount > 0) 
        {
            string toRemove = lockedOnArray[trackIndex];
            Debug.Log("ToRemove: " + toRemove);
            lockedOnSet.Remove(toRemove);
            
            lockedCount--;

            ClearPointCloud(trackIndex);
        }
    }

    private void OnLock(int trackIndex = -1)
    {
        Debug.Log("Locking On! " + trackIndex);

        if (maxLockOns <= lockedOnSet.Count) 
        {
            Debug.Log("Lock On Capacity Full: " + lockedOnSet.Count);
            return;
        }

        if (lockedCount >= maxLockOns) return;

        if (trackIndex == 0 && trackerOne.LockedOn()) return;
        if (trackIndex == 1 && trackerTwo.LockedOn()) return;

        //Check the direction of the sonarRing
        Vector3 rawDirectionSonar = sonarRingTransform.forward;
        Vector2 sonarDirection = new Vector2(rawDirectionSonar.x, rawDirectionSonar.z).normalized;
        Vector3 sonarRingPosition = sonarRingTransform.position;

        float bestDot = -2f;
        int bestTargetIndex = -1;

        float minimalThreshold = .95f; 

        for (int i = 0; i < detectedIndex; i++) 
        {
            //Skip if already locked on
            if (lockedOnSet.Contains(detectionDataArray[i].ID)) continue;

            Vector3 rawDirection = detectionDataArray[i].position - sonarRingPosition;
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

        DetectionData bestTarget = detectionDataArray[bestTargetIndex];

        if (lockedOnSet.Contains(bestTarget.ID)) return;

        if (trackIndex > -1) 
        {
            lockedOnArray[trackIndex] = bestTarget.ID;
        }
        else 
        {
            lockedOnArray[lockedCount] = bestTarget.ID;
        }

        lockedOnSet.Add(bestTarget.ID);
        lockedCount++;

        //If within visual range skip!

        Vector2 randomOffset = Random.insideUnitCircle * 1.2f;
        Vector3 randomPosition = bestTarget.position + new Vector3(randomOffset.x, 0, randomOffset.y);

        bestTarget.sonarDetectable.SpawnTracker(bestTarget.ID, randomPosition);

        int lockIndex = Array.IndexOf(lockedOnArray, bestTarget.ID);

        SonarLockUIData lockData = new SonarLockUIData();

        lockData.trackName = bestTarget.ID;
        lockData.trackIndex = lockIndex;
        lockData.trackColor = spikeColours[lockIndex];

        PrintPointCloud(lockIndex, bestTarget.position, lockIndex);

        uiEvents.OnSonarLock(lockData);
    }

    private void ResetDetections(object sender, EventArgs e)
    {
        detectedIndex = 0;
    }

    #endregion
}
