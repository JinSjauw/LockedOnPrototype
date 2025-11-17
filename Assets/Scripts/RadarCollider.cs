using FOVMapping;
using UnityEngine;

public class RadarCollider : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Enemy")) 
        {
            Debug.Log(other.gameObject.name);

            if(other.gameObject.TryGetComponent(out FOVAgent fovAgent)) 
            {
                if (fovAgent.IsUnderFOW()) 
                {
                    other.gameObject.GetComponentInParent<RadarDetectable>().SpawnHologram();
                }
                else 
                {
                    other.gameObject.GetComponentInParent<OutlineController>().ChangeStencilID(1);
                }
            }

        }
    }
}
