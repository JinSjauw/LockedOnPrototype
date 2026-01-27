using UnityEngine;
using UnityEngine.InputSystem;

namespace ActiveSensing
{
    [RequireComponent(typeof(IndicatorController))]
    public class SeismicSonarRingController : MonoBehaviour
    {
        [SerializeField] private Transform sonarRingTransform;

        private bool dragSonarRing;
        private IndicatorController indicatorController;
        private IndicatorSegment[] indicatorArray;

        private IndicatorSegment selectedIndicator;

        private SeismicSonarRingRotator rotator;
        private SeismicSonarRingSnapper snapper;

        #region Unity

        private void Awake()
        {
            indicatorController = GetComponent<IndicatorController>();
            rotator = GetComponent<SeismicSonarRingRotator>();
            snapper =  GetComponent<SeismicSonarRingSnapper>();
        }

        private void Start()
        {
            Services.Input.OnAdjustSonarRingEvent += AdjustSonarRing;
            Services.Input.OnDragSonarRingEvent += DragSonarRing;
            indicatorArray = indicatorController.GetIndicators();
        }

        private void LateUpdate()
        {
            if (dragSonarRing)
            {
                //Drag Rotate Ring
                rotator.RotateByDelta(sonarRingTransform, Mouse.current.delta.ReadValue());
            }
            else if (selectedIndicator != null)
            {
                snapper.SnapToAngle(sonarRingTransform, selectedIndicator);
            }
        }

        #endregion

        #region Input Handling

        private void AdjustSonarRing()
        {
            rotator.AdjustSonarRing(sonarRingTransform, Mouse.current.position.ReadValue());
            SelectIndicator();
        }

        private void DragSonarRing(bool isDragging)
        {
            if (isDragging)
            {
                dragSonarRing = true;

                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
            else if (!isDragging)
            {
                dragSonarRing = false;

                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;

                SelectIndicator();
            }
        }

        #endregion

        private void SelectIndicator() 
        {
            selectedIndicator = snapper.SelectIndicator(indicatorArray, sonarRingTransform.forward);
        }

        public IndicatorSegment GetSelectedIndicator() 
        {
            return selectedIndicator;
        }
    }
}
