using FOVMapping;
using System;
using UnityEngine;

[CreateAssetMenu(menuName = "Event Channels/FOVChannel")]
public class FOVEvents : ScriptableObject
{
    public EventHandler<FOVAgent> agentRegister;
    public EventHandler<FOVAgent> agentDeregister;

    public void OnAgentRegister(FOVAgent fovAgent) 
    {
        agentRegister?.Invoke(this, fovAgent);
    }

    public void OnAgentDeregister(FOVAgent fovAgent)
    {
        agentDeregister?.Invoke(this, fovAgent);
    }
}
