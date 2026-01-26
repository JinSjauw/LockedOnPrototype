using UnityEngine;
using UnityEngine.InputSystem;

namespace ActiveSensing
{
    [RequireComponent(typeof(IndicatorController), typeof(SeismicSonarRingController))]
    public class SeismicSonarController : MonoBehaviour
    {
        private IndicatorController controller;
        private IndicatorSegment[] indicatorArray;

        private SeismicSonarRingController seismicSonarRing;

        #region Unity

        private void Awake()
        {
            controller = GetComponent<IndicatorController>();
            seismicSonarRing = GetComponent<SeismicSonarRingController>();

            indicatorArray = controller.InitIndicators();
        }

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            Services.Input.OnLockEvent += OnLock;
        }

        #endregion

        #region Input

        private void OnLock()
        {
            //Get angle of 
            IndicatorSegment selectedIndicator = seismicSonarRing.GetSelectedIndicator();
            
            if (selectedIndicator != null) 
            {
                Debug.Log("Locking on in: " + selectedIndicator.name);
            }
        }

        #endregion


        #region Private



        #endregion
    }
}
