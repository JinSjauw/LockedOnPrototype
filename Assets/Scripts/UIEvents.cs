using UnityEngine;
using System;

[CreateAssetMenu(fileName = "UI Events", menuName = "Event Channels/UI Events")]
public class UIEvents : ScriptableObject
{
    public EventHandler<float> onReloadEvent;
    public EventHandler<int> onAmmoChangedEvent;

    public void OnReload(float reloadTime) 
    {
        onReloadEvent?.Invoke(this, reloadTime);
    }

    public void OnAmmoLoss(int ammoAmount) 
    {
        onAmmoChangedEvent?.Invoke(this, ammoAmount);
    }
}
