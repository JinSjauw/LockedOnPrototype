using UnityEngine;

namespace ActiveSensing
{
    public class SeismicSonarRingRotator : MonoBehaviour
    {
        [SerializeField] private float sensitivity = 4f;

        public void AdjustSonarRing(Transform sonarRingTransform, Vector3 mousePosition)
        {
            Ray ray = Camera.main.ScreenPointToRay(mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit, float.MaxValue, LayerMask.GetMask("SonarUI")))
            {
                Vector3 point = hit.point;
                Vector3 dir = point - sonarRingTransform.position;
                dir.y = 0;                      // keep rotation flat
                if (dir.sqrMagnitude > 0.001f)  // avoid jitter when mouse on top
                {
                    Quaternion target = Quaternion.LookRotation(dir);
                    sonarRingTransform.rotation = target; // instant snap
                }
            }
        }

        public void RotateByDelta(Transform sonarRingTransform, Vector2 delta) 
        {
            float speed = delta.x + -delta.y;
            sonarRingTransform.Rotate(0, speed * sensitivity * Time.deltaTime, 0, Space.Self);
        }
    }
}
