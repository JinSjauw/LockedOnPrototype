using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;

namespace ActiveSensing
{

    public class SeismicSonarRingController : MonoBehaviour
    {
        [SerializeField] private Transform sonarRingTransform;

        private bool dragSonarRing;
        private IndicatorSegment[] indicatorArray;

        private IndicatorSegment selectedIndicator;
        private int selectedIndex;

        private SeismicSonarRingRotator rotator;
        private SeismicSonarRingSnapper snapper;
        private SeismicSonarSegmentSelector segmentSelector;

        private int indicatorArrayLength;
        //int[] selectedIndices;

        #region Unity

        private void Awake()
        {
            rotator = GetComponent<SeismicSonarRingRotator>();
            snapper =  GetComponent<SeismicSonarRingSnapper>();
            segmentSelector = GetComponent<SeismicSonarSegmentSelector>();

            //selectedIndices = new int[3]; //3 = center + left & right neighbours
        }

        private void Start()
        {
            Services.Input.OnAdjustSonarRingEvent += AdjustSonarRing;
            Services.Input.OnDragSonarRingEvent += DragSonarRing;

            segmentSelector.SetIndicatorArray(indicatorArray);
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

        //Alert: request script to load in detection data. of these and the neighbours;
        private void SelectIndicator() 
        {
            selectedIndex = segmentSelector.SelectIndicatorIndex(sonarRingTransform.forward);

            selectedIndicator = indicatorArray[selectedIndex];
        }

        public int GetSelectedIndex() 
        {
            //Return int array selected + left & right neighbours
            return selectedIndex;
        }

        public float GetCurrentAngle() 
        {
            float angle = rotator.GetCurrentAngle(sonarRingTransform.forward);

            return angle;
        }

        public void SetIndicatorArray(IndicatorSegment[] indicatorArray)
        {
            this.indicatorArray = indicatorArray;
            indicatorArrayLength = indicatorArray.Length;
        }
    }
}
