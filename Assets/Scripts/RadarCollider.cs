using FOVMapping;
using System;
using UnityEngine;

public class RadarCollider : MonoBehaviour
{
    public EventHandler<String> OnRadarEnter;
    private SphereCollider radarCollider;

    private void Awake()
    {
        radarCollider = GetComponent<SphereCollider>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Enemy")) 
        {
            //Debug.Log(other.gameObject.name);

            if(other.gameObject.TryGetComponent(out FOVAgent fovAgent)) 
            {
                EnemyController enemy = fovAgent.GetComponentInParent<EnemyController>();

                if (enemy) 
                {
                    OnRadarEnter?.Invoke(this, enemy.GetID());

                    if (fovAgent.IsUnderFOW())
                    {
                        other.gameObject.GetComponentInParent<RadarDetectable>().SpawnHologram(enemy.GetID());
                    }
                    else
                    {
                        other.gameObject.GetComponentInParent<OutlineController>().ChangeStencilID(1);
                    }
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
