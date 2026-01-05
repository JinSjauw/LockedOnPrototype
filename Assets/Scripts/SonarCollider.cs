using FOVMapping;
using System;
using UnityEngine;

public class SonarCollider : MonoBehaviour
{
    public EventHandler<DetectionData> OnSonarEnter;
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
                    DetectionData data = new DetectionData(other.gameObject.GetComponentInParent<SonarDetectable>(), other.transform.position, enemy.GetID(), fovAgent.IsUnderFOW());
                    OnSonarEnter?.Invoke(this, data);
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
