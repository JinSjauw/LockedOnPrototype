using UnityEngine;

namespace ActiveSensing
{
    [RequireComponent(typeof(IndicatorController), typeof(SeismicSonarRingController))]
    public class SeismicSonarController : MonoBehaviour
    {
        private IndicatorController indicatorController;
        private IndicatorSegment[] indicatorArray;

        private SeismicSonarRingController seismicSonarRing;
        private SeismicSonarHUD seismicSonarHUD;
        private SeismicSonarDisplay seismicSonarDisplay;

        #region Unity

        private void Awake()
        {
            indicatorController = GetComponent<IndicatorController>();
            seismicSonarRing = GetComponent<SeismicSonarRingController>();
            seismicSonarHUD = GetComponent<SeismicSonarHUD>();
            seismicSonarDisplay = GetComponent<SeismicSonarDisplay>();

            indicatorArray = indicatorController.InitIndicators();
            seismicSonarRing.SetIndicatorArray(indicatorArray);
            seismicSonarDisplay.SetIndicatorArray(indicatorArray);
        }

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            Services.Input.OnLockEvent += OnLock;
        }

        private void Update()
        {
            if (seismicSonarHUD.NarrowBandOpen)
            {
                seismicSonarDisplay.LoadDisplay(seismicSonarRing.GetCurrentAngle());
            }

        }

        void OnDestroy()
        {
            Services.Input.OnLockEvent -= OnLock;
        }

        #endregion

        #region Input

        private void OnLock()
        {
            if (!seismicSonarHUD.NarrowBandOpen)
            {
                seismicSonarHUD.OpenNarrowBand("Opening !");
                //seismicSonarDisplay.LoadSegments(seismicSonarRing.GetSelectedIndex());
            }
            else if (seismicSonarHUD.NarrowBandOpen)
            {
                seismicSonarHUD.CloseNarrowBand();
            }
        }

        #endregion


        #region Private



        #endregion
    }
}
