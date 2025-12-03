using FOVMapping;
using System;
using UnityEngine;

public class SonarCollider : MonoBehaviour
{
    //public EventHandler<String> OnSonarEnter;
    //public EventHandler<Vector3> OnSonarEnter;
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
                    //OnSonarEnter?.Invoke(this, other.transform.position);
                    //OnSonarEnter?.Invoke(this, enemy.GetID());

                    DetectionData data = new DetectionData(other.gameObject.GetComponentInParent<SonarDetectable>(), other.transform.position, enemy.GetID());
                    OnSonarEnter?.Invoke(this, data);

                    //if (fovAgent.IsUnderFOW())
                    //{
                    //    other.gameObject.GetComponentInParent<SonarDetectable>().SpawnHologram(enemy.GetID());
                    //}
                    //else
                    //{
                    //    other.gameObject.GetComponentInParent<OutlineController>().ChangeStencilID(1);
                    //}
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
