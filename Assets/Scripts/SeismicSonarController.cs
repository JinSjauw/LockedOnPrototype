using UnityEngine;

namespace ActiveSensing
{
    [RequireComponent(typeof(IndicatorController), typeof(SeismicSonarRingController))]
    public class SeismicSonarController : MonoBehaviour
    {
        private IndicatorController controller;
        private IndicatorSegment[] indicatorArray;

        private SeismicSonarRingController seismicSonarRing;
        private SeismicSonarHUD seismicSonarHUD;

        #region Unity

        private void Awake()
        {
            controller = GetComponent<IndicatorController>();
            seismicSonarRing = GetComponent<SeismicSonarRingController>();
            seismicSonarHUD = GetComponent<SeismicSonarHUD>();

            indicatorArray = controller.InitIndicators();
        }

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            Services.Input.OnLockEvent += OnLock;
        }

        void OnDestroy()  // or OnDestroy()
        {
            Services.Input.OnLockEvent -= OnLock;
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

                if (!seismicSonarHUD.NarrowBandOpen) 
                { 
                    seismicSonarHUD.OpenNarrowBand(selectedIndicator.name); 
                }
                else if (seismicSonarHUD.NarrowBandOpen)
                {
                    seismicSonarHUD.CloseNarrowBand();
                }
            }


        }

        #endregion


        #region Private



        #endregion
    }
}
