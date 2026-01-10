using System;
using UnityEngine;


//Handles communication between Sonar and Radar
[CreateAssetMenu(fileName = "ActiveSensingChannel", menuName = "Scriptable Objects/Active Sensing Channel")]
public class ActiveSensingChannel : ScriptableObject
{
    //Shared variables
    private float maxSonarRange;

    public float MaxSonarRange {  get { return maxSonarRange; } }

    public void SetSonarRange(float maxSonarRange) 
    {
        this.maxSonarRange = maxSonarRange;
    }

    //Events
    public EventHandler<DetectionData> sonarEnterEvent;

    public void OnSonarEnter(DetectionData data) 
    {
        sonarEnterEvent?.Invoke(this, data);
    }
}
