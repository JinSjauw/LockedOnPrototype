using UnityEngine;

namespace ActiveSensing
{
    public class SeismicSonarRingSnapper : MonoBehaviour
    {
        [SerializeField] private float snapSpeed;

        public void SnapToAngle(Transform sonarRingTransform, IndicatorSegment selectedIndicator) 
        {
            Quaternion targetRot = Quaternion.Euler(0, selectedIndicator.MidAngle, 0);
            sonarRingTransform.rotation = Quaternion.Slerp(sonarRingTransform.rotation, targetRot, Time.deltaTime * snapSpeed);
        }
    }
}
