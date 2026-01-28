using UnityEngine;

namespace ActiveSensing 
{

    public class SeismicSonarHUD : MonoBehaviour
    {
        [SerializeField] private Transform narrowbandDisplay;

        public bool NarrowBandOpen {  get; private set; }

        private void Start()
        {
            NarrowBandOpen = false;
        }

        public void OpenNarrowBand(string name) 
        {
            narrowbandDisplay.gameObject.SetActive(true);
            Debug.Log("Detected: " + name);
            
            NarrowBandOpen = true;
        }

        public void CloseNarrowBand() 
        {
            narrowbandDisplay.gameObject.SetActive(false);
            NarrowBandOpen = false;
        }
    }

}

