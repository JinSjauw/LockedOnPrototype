using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;


//Handles communication between Sonar and Radar
[CreateAssetMenu(fileName = "ActiveSensingChannel", menuName = "Scriptable Objects/Active Sensing Channel")]
public class ActiveSensingChannel : ScriptableObject
{
    //Shared variables

    private int detectedIndex;
    public int DetectedIndex 
    {
        get { return detectedIndex; } 
        set { detectedIndex = value; }
    }

    [SerializeField] private int lockedOnCount;
    public int LockedOnCount 
    {
        get { return lockedOnCount; } 
        set { lockedOnCount = Mathf.Clamp(value, 0, lockOnArray.Length);}
    }

    private float maxSonarRange;
    public float MaxSonarRange {  get { return maxSonarRange; } }
    
    public void SetSonarRange(float maxSonarRange) 
    {
        this.maxSonarRange = maxSonarRange;
    }

    //Collections
    private Dictionary<string, DetectionData> detectionDataDict;
    public int DataEntries { get { return detectionDataDict.Count; } }

    [SerializeField] private string[] lockOnArray;

    public string[] LockOnArray { get { return lockOnArray; } }

    private List<string> visibleEnemiesList;
    public List<string> VisibleEnemies { get { return visibleEnemiesList; } }

    private List<Projectile> visibleProjectilesList;

    public List<Projectile> VisibleProjectiles {  get { return visibleProjectilesList; } }

    //Collection Management
    public void InitializeCollections(int maxLockOns) 
    {
        detectionDataDict = new Dictionary<string, DetectionData>();
        lockOnArray = new string[maxLockOns];

        visibleEnemiesList = new List<string>();
        visibleProjectilesList = new List<Projectile>();

        for (int i = 0; i < lockOnArray.Length; i++) 
        {
            lockOnArray[i] = "EMPTY";
        }
        
        lockedOnCount = 0;
    }

    public bool LockOnContains(string ID) 
    {
        return lockOnArray.Contains(ID);
    }

    public int LockOnIndex(string ID) 
    {
        return Array.IndexOf(lockOnArray, ID);
    }

    public DetectionData GetDetectionData(string ID) 
    {
        if (!detectionDataDict.ContainsKey(ID)) 
        {
            //Debug.LogError("NO SUCH ENTRY");
            return null;
        }

        return detectionDataDict[ID];
    }

    public DetectionData DataAtIndex(int index) 
    {
        if(detectionDataDict.Count <= index) 
        {
            //Debug.LogError("INDEX LARGER THAN DETECTION DICT!");
            return null;
        }

        return detectionDataDict.ElementAt(index).Value;
    }

    public void AddData(DetectionData dataToAdd) 
    {
        detectionDataDict.Add(dataToAdd.ID, dataToAdd);
    }

    public void ClearDetectionData()
    {
        detectionDataDict.Clear();
    }

    //Events
    public EventHandler<DetectionData> sonarEnterEvent;
    public EventHandler<DetectionData> radarEnterEvent;
    public EventHandler<string> radarExitEvent;

    public UnityAction<int, float> signalLockedEvent;


    public void OnSonarEnter(DetectionData data) 
    {
        sonarEnterEvent?.Invoke(this, data);
    }

    public void OnRadarExit(string ID) 
    {
        radarExitEvent?.Invoke(this, ID);
    }

    public void OnSignalLocked(int trackerIndex, float score)
    {
        signalLockedEvent?.Invoke(trackerIndex, score);
    }
}
