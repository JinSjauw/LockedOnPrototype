using UnityEngine;

public class CurveController : MonoBehaviour
{
    [SerializeField][Range(0, 1)] private float position;
    [SerializeField] private Transform startCurve, endCurve, control;
    
    public Vector3 ControlPosition { get => control.position; }
    public float DesiredCurveHeight {  get => desiredCurveHeight; set => desiredCurveHeight = value; }
    public float CurrentCurveHeight { get => transform.position.y; }

    [SerializeField] private float desiredCurveHeight;

    void Update()
    {
        Vector3 inlinePositon = Vector3.Lerp(startCurve.position, endCurve.position, position);

        control.position = new Vector3(inlinePositon.x, desiredCurveHeight, inlinePositon.z);
    }
}
