using FOVMapping;
using System;
using UnityEngine;

public class SonarCollider : MonoBehaviour
{
    [SerializeField] private ActiveSensingChannel activeSensingEvents;

    private SphereCollider radarCollider;

    private void Awake()
    {
        radarCollider = GetComponent<SphereCollider>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Enemy")) 
        {
            if(other.TryGetComponent(out FOVAgent fovAgent)) 
            {
                EnemyController enemy = fovAgent.GetComponentInParent<EnemyController>();

                if (enemy) 
                {
                    DetectionData data = new DetectionData(other.GetComponent<ActiveSensingDetectable>(), other.transform.position, enemy.GetID(), fovAgent.IsVisible());
                    activeSensingEvents.OnSonarEnter(data);
                }
            }
        }
    }

    public void ColliderEnable(bool enabled)
    {
        radarCollider.enabled = enabled;
    }
    public void UpdateCollider(float radius)
    {
        radarCollider.radius = radius;
    }
}
