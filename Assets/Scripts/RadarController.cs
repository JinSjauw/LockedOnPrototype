using FOVMapping;
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
    List<Projectile> visibleProjectiles;

    #region Unity
    private void Awake()
    {
        visibleEnemies = new Dictionary<ActiveSensingDetectable, DetectionData>();
        radarAreaCollider = GetComponent<SphereCollider>();

        radarAreaCollider.radius = visualRangeRadius;
    }

    private void Start()
    {
        //radarCollider.OnSonarEnter += AddDetection;
    }

    private void Update()
    {
        //Update visible enemies UI
    }

    private void LateUpdate()
    {
        //Update the enemy UI if needed
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Enemy"))
        {
            ActiveSensingDetectable activeSensingDetectable = other.GetComponent<ActiveSensingDetectable>();
            if (visibleEnemies.ContainsKey(activeSensingDetectable)) return;

            if (other.TryGetComponent(out FOVAgent fovAgent))
            {
                EnemyController enemy = fovAgent.GetComponentInParent<EnemyController>();

                if (enemy && fovAgent.IsVisible())
                {
                    activeSensingDetectable.EnableMark(true);

                    DetectionData data = new DetectionData(activeSensingDetectable, other.transform.position, enemy.GetID(), fovAgent.IsVisible());
                    ////Into a collection
                    ////Visible Enemies
                    visibleEnemies.Add(activeSensingDetectable, data);
                    activeSensingChannel.VisibleEnemies.Add(data.ID);
                }
            }
        }

        if(other.gameObject.layer == LayerMask.NameToLayer("Projectile")) 
        {
            if(TryGetComponent(out Projectile projectile))
            {
                visibleProjectiles.Add(projectile);
                //enable projectile UI
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Enemy"))
        {
            if (other.TryGetComponent(out FOVAgent fovAgent))
            {
                ActiveSensingDetectable activeSensingDetectable = other.GetComponent<ActiveSensingDetectable>();

                activeSensingDetectable.EnableMark(false);

                if(visibleEnemies.TryGetValue(activeSensingDetectable, out DetectionData data)) 
                {
                    activeSensingChannel.OnSonarExit(data.ID);
                    visibleEnemies.Remove(activeSensingDetectable);
                    activeSensingChannel.VisibleEnemies.Remove(data.ID);
                }
            }
        }

        if (other.gameObject.layer == LayerMask.NameToLayer("Projectile"))
        {
            if (TryGetComponent(out Projectile projectile))
            {
                visibleProjectiles.Remove(projectile);
            }
        }
    }

    #endregion

    #region Functions

  
    #endregion
}
