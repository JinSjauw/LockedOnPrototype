using Ami.BroAudio;
using System;
using UnityEngine;
using UnityEngine.InputSystem;
using Random = UnityEngine.Random;

public class SonarController : MonoBehaviour
{
    //TEMP
    [SerializeField] private GameObject stationaryIndicator;

    [SerializeField] private UIEvents uiEvents;
    [SerializeField] private ActiveSensingChannel activeSensingChannel;
    [SerializeField] private PlayerEvents playerEvents;
    [SerializeField] private SoundConfig soundConfig;

    [SerializeField] private Transform sonarRingTransform;
    [SerializeField] private Transform targetTransform;

    [SerializeField] private KalmanPointDrawer[] trackers;

    [SerializeField] private float maxRange;
    [SerializeField] private float minRange;

    [SerializeField] private int maxDetections = 8;

    [ColorUsage(true, true)]
    [SerializeField] Color[] spikeColours = new Color[4];

    [SerializeField] private int maxLockOns = 2;

    private SonarEffectAnimator sonarEffect;
    private bool[] trackFocused;
    private float[] trackScore;
    private bool isStationary;

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
        sonarEffect = GetComponent<SonarEffectAnimator>();

        activeSensingChannel.SetSonarRange(maxRange);
        activeSensingChannel.InitializeCollections(maxLockOns);
        trackFocused = new bool[maxLockOns];
        trackScore = new float[maxLockOns];
    }

    private void Start()
    {
        sonarEffect.cycleFinishEvent += ResetDetections;
        activeSensingChannel.sonarEnterEvent += OnDetectionReceive;
        activeSensingChannel.radarExitEvent += OnRadarExit;
        activeSensingChannel.signalLockedEvent += OnSignalLocked;
        uiEvents.sonarLockButtonEvent += OnLockFromUI;
        uiEvents.sonarLockLossEvent += OnLockLossFromUI;
        uiEvents.sonarSuperLockLossEvent += OnLockLossFromUI;
        playerEvents.playerStationaryEvent += OnPlayerStationary;
    }

    private void OnDisable()
    {
        sonarEffect.cycleFinishEvent -= ResetDetections;
        activeSensingChannel.sonarEnterEvent -= OnDetectionReceive;
        activeSensingChannel.radarExitEvent -= OnRadarExit;
        activeSensingChannel.signalLockedEvent -= OnSignalLocked;
        uiEvents.sonarLockButtonEvent -= OnLockFromUI;
        uiEvents.sonarLockLossEvent -= OnLockLossFromUI;
        playerEvents.playerStationaryEvent -= OnPlayerStationary;
    }

    #endregion

    #region Functions

    //If ID matches one of the Locked On ID's give it a different color!
    private void OnDetectionReceive(object sender, DetectionData data)
    {
        if (activeSensingChannel.DetectedIndex >= maxDetections) return;

        // Calculate based on max and min range (Closer is taller peak)

        int colorIndex = 3;

        if (activeSensingChannel.LockOnContains(data.ID))
        {
            Vector2 randomOffset = Random.insideUnitCircle * 1.2f;
            Vector3 randomPosition = data.position + new Vector3(randomOffset.x, 0, randomOffset.y);

            float heightOffset = 1f;
            randomPosition.y += heightOffset;
            
            int lockIndex = activeSensingChannel.LockOnIndex(data.ID);

            colorIndex = lockIndex;

            if (!activeSensingChannel.VisibleEnemies.Contains(data.ID))
            {
                data.sonarDetectable.SpawnTracker(data.ID, randomPosition, spikeColours[colorIndex]);
                PrintPointCloud(lockIndex, data.position);
            }
            else
            {
                ClearPointCloud(lockIndex);
            }
        }

        Vector2 centerPosition = new Vector2(transform.position.x, transform.position.z);
        Vector2 targetPosition = new Vector2(data.position.x, data.position.z);

        float distance = Vector2.Distance(centerPosition, targetPosition);
        float distanceNormalized = (distance - minRange) / (maxRange - minRange);

        sonarEffect.UpdateSpike(new Vector2(data.position.x, data.position.z), distanceNormalized, data, colorIndex);

        //detectionDataArray[detectedIndex] = data;
        activeSensingChannel.AddData(data);

        activeSensingChannel.DetectedIndex++;
    }

    private void OnRadarExit(object sender, string ID)
    {
        if (activeSensingChannel.LockOnContains(ID))
        {
            int lockIndex = activeSensingChannel.LockOnIndex(ID);

            DetectionData data = activeSensingChannel.GetDetectionData(ID);

            if (data == null) 
            {
                Debug.LogError("Data is null for: " + ID);
                return;
            }

            Vector2 randomOffset = Random.insideUnitCircle * 1.2f;
            Vector3 randomPosition = data.position + new Vector3(randomOffset.x, 0, randomOffset.y);

            float heightOffset = 1f;
            randomPosition.y += heightOffset;

            data.sonarDetectable.SpawnTracker(data.ID, randomPosition, spikeColours[lockIndex]);
            PrintPointCloud(lockIndex, data.position);
            
        }
    }

    private void PrintPointCloud(int lockedIndex, Vector3 position, int colorIndex = -1, Vector3 markPosition = new Vector3()) 
    {
        if (colorIndex > -1) trackers[lockedIndex].SetPointColor(spikeColours[colorIndex]);

        trackers[lockedIndex].PrintPointCloud(position, markPosition);
    }

    private void ClearPointCloud(int lockedIndex) 
    {
        trackers[lockedIndex].StopDrawing();
    }

    private void OnLockFromUI(object sender, int trackIndex)
    {
        OnLock(trackIndex);
    }

    private void OnLockLossFromUI(object sender, int trackIndex)
    {
        Debug.Log("Losing Lock! " + trackIndex);
        OnLockLoss(trackIndex);
    }

    private void OnLockLoss(int trackIndex)
    {
        if(activeSensingChannel.LockedOnCount > 0) 
        {
            activeSensingChannel.LockOnArray[trackIndex] = "EMPTY";
            activeSensingChannel.LockedOnCount--;
        }

        OnSignalLocked(trackIndex, 0); //Reset focusedSignal flag
        ClearPointCloud(trackIndex);
    }

    private void OnLock(int trackIndex = -1)
    {
        Debug.Log("Locking On! " + trackIndex);

        string[] lockedOnList = activeSensingChannel.LockOnArray;

        if (maxLockOns <= activeSensingChannel.LockedOnCount) 
        {
            Debug.Log("Lock On Capacity Full: " + activeSensingChannel.LockedOnCount);
            return;
        }

        if(trackIndex > -1 && trackers[trackIndex].IsDrawing()) 
        {
             return;
        }

        //Check the direction of the sonarRing
        Vector3 rawDirectionSonar = sonarRingTransform.forward;
        Vector2 sonarDirection = new Vector2(rawDirectionSonar.x, rawDirectionSonar.z).normalized;
        Vector3 sonarRingPosition = sonarRingTransform.position;

        float bestDot = -2f;
        int bestTargetIndex = -1;

        float minimalThreshold = .95f; 

        for (int i = 0; i < activeSensingChannel.DataEntries; i++) 
        {
            DetectionData data = activeSensingChannel.DataAtIndex(i);

            //Skip if already locked on
            if (data == null) 
            {
                Debug.LogError("Data is null at index: " + i);

                continue;
            }

            if (activeSensingChannel.LockOnContains(data.ID)) continue;

            Vector3 rawDirection = data.position - sonarRingPosition;
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

        DetectionData bestTarget = activeSensingChannel.DataAtIndex(bestTargetIndex);

        if (activeSensingChannel.LockOnContains(bestTarget.ID)) return;

        if (trackIndex > -1) 
        {
            lockedOnList[trackIndex] = bestTarget.ID;
        }
        else 
        {
            int availableIndex = Array.FindIndex(lockedOnList, i => i == "EMPTY");
            lockedOnList[availableIndex] = bestTarget.ID;
        }

        activeSensingChannel.LockedOnCount++;

        //If within visual range skip!

        Vector2 randomOffset = Random.insideUnitCircle * 1.2f;
        Vector3 randomPosition = bestTarget.position + new Vector3(randomOffset.x, 0, randomOffset.y);

        int lockIndex = activeSensingChannel.LockOnIndex(bestTarget.ID);

        Color spikeColor = spikeColours[lockIndex];

        if (!activeSensingChannel.VisibleEnemies.Contains(bestTarget.ID))
        {
            bestTarget.sonarDetectable.SpawnTracker(bestTarget.ID, randomPosition, spikeColor);
            PrintPointCloud(lockIndex, bestTarget.position, lockIndex);
        }
        else 
        {
            ClearPointCloud(lockIndex);
        }

        SonarLockUIData lockData = new SonarLockUIData();

        lockData.trackName = bestTarget.ID;
        lockData.trackIndex = lockIndex;
        lockData.trackColor = spikeColor;

        uiEvents.OnSonarLock(lockData);
        BroAudio.Play(soundConfig.lockOnSound);
    }

    private void OnPlayerStationary(object sender, bool stationary)
    {
        stationaryIndicator.SetActive(stationary);

        isStationary = stationary;

        UpdatePointDataType(0);
        UpdatePointDataType(1);
    }

    private void OnSignalLocked(int trackIndex, float score)
    {
        //trackers[0].SetPointParams();
        if(score > 0) 
        {
            trackFocused[trackIndex] = true;
            trackScore[trackIndex] = score;
        }
        else 
        {
            trackFocused[trackIndex] = false;
            trackScore[trackIndex] = -1; //no score
        }

        UpdatePointDataType(trackIndex);
    }

    private void UpdatePointDataType(int trackIndex) 
    {
        PointCloudType typeToSet = PointCloudType.DEFAULT;

        if (isStationary)
        {
            if (trackFocused[trackIndex])
            {
                typeToSet = PointCloudType.FOCUSED_STATIONARY;
            }
            else
            {
                typeToSet = PointCloudType.DEFAULT_STATIONARY;
            }
        }
        else
        {
            if (trackFocused[trackIndex])
            {
                typeToSet = PointCloudType.FOCUSED_MOVING;
            }
            else
            {
                typeToSet = PointCloudType.DEFAULT_MOVING;
            }
        }

        trackers[trackIndex].SetPointParams(typeToSet, trackScore[trackIndex]);
    }

    private void ResetDetections(object sender, EventArgs e)
    {
        activeSensingChannel.DetectedIndex = 0;
        activeSensingChannel.ClearDetectionData();
    }

    #endregion
}
