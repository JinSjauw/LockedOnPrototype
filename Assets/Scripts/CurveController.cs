using UnityEngine;

public class CurveController : MonoBehaviour
{
    [SerializeField][Range(0, 1)] private float position;
    [SerializeField] private Transform startCurve, endCurve;
    
    public float DesiredCurveHeight {  get => desiredCurveHeight; set => desiredCurveHeight = value; }
    public float CurveApexHeight { get => curveApexHeight; }
    public float CurrentCurveHeight { get => transform.position.y; }

    [SerializeField] private float desiredCurveHeight;
    [SerializeField] private float curveApexHeight;
    [SerializeField][Range(0, 1)] private float apexPosition;
    [SerializeField] private Transform debug;

    void Update()
    {
        Vector3 inlinePositon = Vector3.Lerp(startCurve.position, endCurve.position, position);

        curveApexHeight = QuadraticCurve.EvaluateCurve(startCurve.position, endCurve.position, transform.position, apexPosition).y;

        if(debug != null) debug.position = new Vector3(inlinePositon.x, curveApexHeight, inlinePositon.z);

        transform.position = new Vector3(inlinePositon.x, desiredCurveHeight, inlinePositon.z);
    }
}
