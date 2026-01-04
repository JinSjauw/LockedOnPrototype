using UnityEngine;
using UnityEngine.InputSystem;

public class SonarRingController : MonoBehaviour
{
    [SerializeField] private Transform sonarRingTransform;
    private bool moveSonarRing;

    private void OnAdjustSonarRing(InputValue value)
    {
        if (value.isPressed)
        {
            moveSonarRing = true;
            AdjustSonarRing();
        }
        else
        {
            moveSonarRing = false;
        }
    }

    private void AdjustSonarRing()
    {
        if (moveSonarRing)
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit, float.MaxValue, LayerMask.GetMask("Ground")))
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
}
