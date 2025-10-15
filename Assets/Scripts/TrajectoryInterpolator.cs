using UnityEngine;

public class TrajectoryInterpolator : MonoBehaviour
{
    [SerializeField] private Transform previewTarget, realTarget;
    [SerializeField] private CurveController previewCurveControl, realCurveControl;
    [SerializeField] private float changeRate;

    private void Start()
    {
        realCurveControl.InterpolateTo = true;
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 targetPosition = realTarget.position;
        targetPosition = Vector3.MoveTowards(targetPosition, previewTarget.position, changeRate * Time.deltaTime);

        //Vector3 finalPosition = new Vector3(targetPosition.x, 0, targetPosition.y);

        if (Physics.Raycast(targetPosition, Vector3.down, out RaycastHit hit, LayerMask.GetMask("Ground")))
        {
            //Debug.Log("Hit: " + hit.collider.name + " " + hit.point.y);
            targetPosition.y = hit.point.y;
        }

        realTarget.position = targetPosition;

        //realCurveControl.DesiredCurveHeight = Mathf.MoveTowards(realCurveControl.DesiredCurveHeight, previewCurveControl.DesiredCurveHeight, changeRate * Time.deltaTime);
        realCurveControl.DesiredCurveHeight = previewCurveControl.DesiredCurveHeight;
    }
}
