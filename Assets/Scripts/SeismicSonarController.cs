using UnityEngine;
using UnityEngine.InputSystem;

namespace ActiveSensing
{
    [RequireComponent(typeof(IndicatorController))]
    public class SeismicSonarController : MonoBehaviour
    {
        [SerializeField] private Transform selectionRing;

        private IndicatorController controller;
        private IndicatorSegment[] indicatorArray;

        //Input
        private PlayerInput playerInput;

        #region Unity

        private void Awake()
        {
            playerInput = GetComponentInParent<PlayerInput>();
            controller = GetComponent<IndicatorController>();

            indicatorArray = controller.InitIndicators();

        }

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            
        }

        // Update is called once per frame
        void Update()
        {

        }

        #endregion

        #region Input



        #endregion


        #region Private



        #endregion
    }
}
