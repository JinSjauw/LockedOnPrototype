using UnityEngine;
using System;

[CreateAssetMenu(fileName = "UI Events", menuName = "Event Channels/UI Events")]
public class UIEvents : ScriptableObject
{
    public EventHandler<float> reloadEvent;
    public EventHandler<int> ammoChangedEvent;

    public EventHandler<SonarLockUIData> sonarLockEvent;
    public EventHandler<int> sonarLockLossEvent;

    public EventHandler<int> sonarLockButtonEvent;

    public void OnReload(float reloadTime) 
    {
        reloadEvent?.Invoke(this, reloadTime);
    }

    public void OnAmmoLoss(int ammoAmount) 
    {
        ammoChangedEvent?.Invoke(this, ammoAmount);
    }

    public void OnSonarLock(SonarLockUIData lockData) 
    {
        sonarLockEvent?.Invoke(this, lockData);
    }

    public void OnSonarLockLoss(int trackIndex)
    {
        sonarLockLossEvent?.Invoke(this, trackIndex);
    }

    public void OnSonarLockButton(int trackIndex) 
    {
        sonarLockButtonEvent?.Invoke(this, trackIndex);
    }
}

//Data struct for SonarLockUIData
public struct SonarLockUIData 
{
    public int trackIndex;
    public string trackName;
    public Color trackColor;
}
