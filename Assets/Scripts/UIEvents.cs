using UnityEngine;
using System;
using UnityEngine.Events;

[CreateAssetMenu(fileName = "UI Events", menuName = "Event Channels/UI Events")]
public class UIEvents : ScriptableObject
{
    public EventHandler<float> reloadMainGunEvent;
    public EventHandler<int> mainGunAmmoChangedEvent;

    public EventHandler<float> reloadInterceptGunEvent;
    public EventHandler<int> interceptGunAmmoChangedEvent;

    public EventHandler<SonarLockUIData> sonarLockEvent;
    public EventHandler<int> sonarLockLossEvent;
    public EventHandler<int> sonarLockButtonEvent;

    public EventHandler<int> sonarSuperLockLossEvent;
    public EventHandler<int> sonarSuperLockButtonEvent;

    public UnityAction<int, int> newWaveEvent;
    public EventHandler<int> enemyAmountChangedEvent;

    public EventHandler<float> healthChangedEvent;

    public void OnReloadMainGun(float reloadTime) 
    {
        reloadMainGunEvent?.Invoke(this, reloadTime);
    }

    public void OnMainGunAmmoChanged(int ammoAmount) 
    {
        mainGunAmmoChangedEvent?.Invoke(this, ammoAmount);
    }

    public void OnReloadInterceptGun(float reloadTime) 
    {
        reloadInterceptGunEvent?.Invoke(this, reloadTime);
    }

    public void OnInterceptGunAmmoChanged(int ammoAmount) 
    {
        interceptGunAmmoChangedEvent?.Invoke(this, ammoAmount);
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

    public void OnSonarSuperLockLoss(int trackIndex)
    {
        sonarSuperLockLossEvent?.Invoke(this, trackIndex);
    }

    public void OnSonarSuperLockButton(int trackIndex)
    {
        sonarSuperLockButtonEvent?.Invoke(this, trackIndex);
    }

    public void OnNewWaveEvent(int waveNumber, int maxEnemies) 
    {
        newWaveEvent?.Invoke(waveNumber, maxEnemies);
    }

    public void OnEnemyAmountChangedEvent(int amount) 
    {
        enemyAmountChangedEvent?.Invoke(this, amount);
    }

    public void OnHealthChanged(float health) 
    {
        healthChangedEvent?.Invoke(this, health);
    }
}

//Data struct for SonarLockUIData
public struct SonarLockUIData 
{
    public int trackIndex;
    public string trackName;
    public Color trackColor;
}
