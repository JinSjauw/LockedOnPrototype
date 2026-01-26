using UnityEngine;
using UnityEngine.InputSystem;

namespace ActiveSensing
{

    [RequireComponent(typeof(IndicatorController))]
    public class SeismicSonarRingController : MonoBehaviour
    {
        [SerializeField] private Transform sonarRingTransform;
        [SerializeField] private float sensitivity;
        [SerializeField] private float snapSpeed;

        private bool dragSonarRing;
        private IndicatorController indicatorController;
        private IndicatorSegment[] indicatorArray;

        private IndicatorSegment selectedIndicator;

        #region Unity

        private void Awake()
        {
            indicatorController = GetComponent<IndicatorController>();
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
                Vector2 delta = Mouse.current.delta.ReadValue();
                float speed = delta.x + -delta.y;

                sonarRingTransform.Rotate(0, speed * sensitivity * Time.deltaTime, 0, Space.Self);
            }
            else if (selectedIndicator != null)
            {
                Quaternion targetRot = Quaternion.Euler(0, selectedIndicator.MidAngle, 0);
                sonarRingTransform.rotation = Quaternion.Slerp(sonarRingTransform.rotation, targetRot, Time.deltaTime * snapSpeed);
            }
        }

        #endregion

        #region Ring Control

        private void AdjustSonarRing()
        {
            if (dragSonarRing) return;

            Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
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

            SnapToIndicator(sonarRingTransform.forward);
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

                SnapToIndicator(sonarRingTransform.forward);

                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
        }

        #endregion

        //Snap to closest position
        private void SnapToIndicator(Vector3 direction)
        {
            Vector2 input = new Vector2(direction.x, direction.z);

            if (input.sqrMagnitude < 0.01f) return;

            // Convert input to angle
            float inputAngle = Mathf.Atan2(input.x, input.y) * Mathf.Rad2Deg;
            if (inputAngle < 0) inputAngle += 360f;

            // Find closest point
            int smallestIndex = 0;
            float smallest = float.MaxValue;

            for (int i = 0; i < indicatorArray.Length; i++)
            {
                float angleDiff = Mathf.Abs(inputAngle - indicatorArray[i].MidAngle);
                if (angleDiff > 180f) angleDiff = 360f - angleDiff; // Wrap around

                if (angleDiff < smallest)
                {
                    smallest = angleDiff;
                    smallestIndex = i;
                }
            }

            // Save selected
            selectedIndicator = indicatorArray[smallestIndex];
        }
        
        public IndicatorSegment GetSelectedIndicator() 
        {
            return selectedIndicator;
        }

    }
}
