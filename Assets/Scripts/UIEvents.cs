using UnityEngine;
using System;

[CreateAssetMenu(fileName = "UI Events", menuName = "Event Channels/UI Events")]
public class UIEvents : ScriptableObject
{
    public EventHandler<float> onReloadEvent;
    public EventHandler<int> onAmmoChangedEvent;

    public EventHandler<SonarLockUIData> onSonarLockEvent;
    public EventHandler<int> onSonarLockLossEvent;

    public EventHandler<int> onSonarLockButtonEvent;

    public void OnReload(float reloadTime) 
    {
        onReloadEvent?.Invoke(this, reloadTime);
    }

    public void OnAmmoLoss(int ammoAmount) 
    {
        onAmmoChangedEvent?.Invoke(this, ammoAmount);
    }

    public void OnSonarLock(SonarLockUIData lockData) 
    {
        onSonarLockEvent?.Invoke(this, lockData);
    }

    public void OnSonarLockLoss(int trackIndex)
    {
        onSonarLockLossEvent?.Invoke(this, trackIndex);
    }

    public void OnSonarLockButton(int trackIndex) 
    {
        onSonarLockButtonEvent?.Invoke(this, trackIndex);
    }
}

//Data struct for SonarLockUIData
public struct SonarLockUIData 
{
    public int trackIndex;
    public string trackName;
    public Color trackColor;
}
