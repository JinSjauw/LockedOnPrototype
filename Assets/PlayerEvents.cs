using System;
using UnityEngine;

[CreateAssetMenu(fileName = "PlayerEvents", menuName = "Scriptable Objects/PlayerEvents")]
public class PlayerEvents : ScriptableObject
{
    public EventHandler<float> playerSpeedChangedEvent;
    public EventHandler<bool> playerStationaryEvent;

    public EventHandler playerDeathEvent;
    public EventHandler<bool> playerHealthChangedEvent;
    public EventHandler<bool> playerSpawnedEvent;

    public void OnPlayerSpeedChanged(float speed) 
    {
        playerSpeedChangedEvent?.Invoke(this, speed);
    }

    public void OnPlayerStationary(bool state) 
    {
        playerStationaryEvent?.Invoke(this, state);
    }
}
