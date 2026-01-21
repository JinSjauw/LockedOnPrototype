using UnityEngine;
using UnityEngine.InputSystem;

public class SonarRingController : MonoBehaviour
{
    [SerializeField] private Transform sonarRingTransform;
    [SerializeField] private float sensitivity;

    [SerializeField] private bool dragSonarRing;
    [SerializeField] private InputActionReference dragSonarRingAction;

    private void OnAdjustSonarRing(InputValue value)
    {
        if (value.isPressed)
        {
            //dragSonarRing = true;
            AdjustSonarRing();
        }
        else
        {
            //dragSonarRing = false;
        }
    }

    private void DragSonarRing(InputAction.CallbackContext context) 
    {
        if (context.performed)
        {
            dragSonarRing = true;

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        else if (context.canceled)
        {
            dragSonarRing = false;

            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    private void OnEnable()
    {
        dragSonarRingAction.action.performed += DragSonarRing;
        dragSonarRingAction.action.canceled += DragSonarRing;
        dragSonarRingAction.action.Enable();
    }

    private void OnDisable()
    {
        dragSonarRingAction.action.performed -= DragSonarRing;
        dragSonarRingAction.action.canceled -= DragSonarRing;
        dragSonarRingAction.action.Disable();
    }

    private void LateUpdate()
    {
        if (dragSonarRing) 
        {
            Vector2 delta = Mouse.current.delta.ReadValue();
            float speed = delta.x + -delta.y;

            sonarRingTransform.Rotate(0, speed * sensitivity * Time.deltaTime, 0, Space.Self);
        }
    }

    private void AdjustSonarRing()
    {
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
        
    }
}
