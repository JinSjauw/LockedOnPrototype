using System;
using UnityEngine;

public class CollisionChecker : MonoBehaviour
{
    public EventHandler<Collider> CollisionEvent;

    private void OnTriggerEnter(Collider other)
    {
        CollisionEvent?.Invoke(this, other);
    }
}
