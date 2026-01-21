using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SphereCollider))]
public class RadarController : MonoBehaviour
{
    [SerializeField] private CameraEvents cameraEvents;
    [SerializeField] private ActiveSensingChannel activeSensingChannel;
    [SerializeField] private float visualRangeRadius; //TEMP Should be same as fov set in player FOVAgent component

    private SphereCollider radarAreaCollider;

    Dictionary<ActiveSensingDetectable, DetectionData> visibleEnemies;

    #region Unity
    private void Awake()
    {
        visibleEnemies = new Dictionary<ActiveSensingDetectable, DetectionData>();
        radarAreaCollider = GetComponent<SphereCollider>();

        radarAreaCollider.radius = visualRangeRadius;
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Enemy"))
        {
            ActiveSensingDetectable activeSensingDetectable = other.GetComponent<ActiveSensingDetectable>();
            if (visibleEnemies.ContainsKey(activeSensingDetectable)) return;
            {
                EnemyController enemy = other.GetComponentInParent<EnemyController>();

                //if (enemy && fovAgent.IsVisible()) 
                if(enemy && Physics.Linecast(transform.position, other.transform.position, LayerMask.GetMask("Ground")))
                {
                    activeSensingDetectable.EnableMark(true);

                    DetectionData data = new DetectionData(activeSensingDetectable, other.transform.position, enemy.GetID(), false);
                    ////Into a collection
                    ////Visible Enemies
                    visibleEnemies.Add(activeSensingDetectable, data);
                    activeSensingChannel.VisibleEnemies.Add(data.ID);
                }
            }
        }

        if (other.gameObject.layer == LayerMask.NameToLayer("Projectile"))
        {
            if (other.TryGetComponent(out Projectile projectile))
            {
                //If terrain is blocking direct lines no projectile mark.
                if (Physics.Linecast(transform.position, projectile.transform.position, LayerMask.GetMask("Ground"))) 
                {
                    //Debug.Log("Line Obstructed! " + projectile.name);
                    return;
                }
                projectile.EnableMark(true);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Enemy"))
        {
            ActiveSensingDetectable activeSensingDetectable = other.GetComponent<ActiveSensingDetectable>();

            activeSensingDetectable.EnableMark(false);

            if(visibleEnemies.TryGetValue(activeSensingDetectable, out DetectionData data)) 
            {
                activeSensingChannel.OnRadarExit(data.ID);
                visibleEnemies.Remove(activeSensingDetectable);
                activeSensingChannel.VisibleEnemies.Remove(data.ID);
            }
        }
    }

    #endregion

    #region Functions

  
    #endregion
}
