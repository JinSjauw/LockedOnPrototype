using FOVMapping;
using System;
using UnityEngine;

[CreateAssetMenu(menuName = "Event Channels/FOVChannel")]
public class FOVEvents : ScriptableObject
{
    public EventHandler<FOVAgent> agentRegisterEvent;
    public EventHandler<FOVAgent> agentDeregisterEvent;

    public EventHandler<FOVAgent> agentEnterFOWEvent;
    public EventHandler<FOVAgent> agentExitFOWEvent;

    public void OnAgentRegister(FOVAgent fovAgent) 
    {
        agentRegisterEvent?.Invoke(this, fovAgent);
    }

    public void OnAgentDeregister(FOVAgent fovAgent)
    {
        agentDeregisterEvent?.Invoke(this, fovAgent);
    }

    public void OnAgentEnterFOW(FOVAgent fovAgent) 
    {
        agentEnterFOWEvent?.Invoke(this, fovAgent);
    }
    
    public void OnAgentExitFOW(FOVAgent fovAgent) 
    {
        agentExitFOWEvent?.Invoke(this, fovAgent);
    }
}
