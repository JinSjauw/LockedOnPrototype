using UnityEngine;

public class RadarCollider : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Enemy")) 
        {
            Debug.Log(other.gameObject.name);

            other.gameObject.GetComponentInParent<OutlineController>().ChangeStencilID(1);
        }
    }
}
