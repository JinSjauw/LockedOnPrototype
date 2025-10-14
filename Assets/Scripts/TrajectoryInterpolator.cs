using UnityEngine;

public class TrajectoryInterpolator : MonoBehaviour
{
    [SerializeField] private Transform previewTarget, realTarget;
    [SerializeField] private CurveController previewCurveControl, realCurveControl;
    [SerializeField] private float changeRate;


    // Update is called once per frame
    void Update()
    {
        realTarget.position = Vector3.MoveTowards(realTarget.position, previewTarget.position, changeRate * Time.deltaTime);
        realCurveControl.DesiredCurveHeight = Mathf.MoveTowards(realCurveControl.DesiredCurveHeight, previewCurveControl.DesiredCurveHeight, changeRate * Time.deltaTime);
    }
}
