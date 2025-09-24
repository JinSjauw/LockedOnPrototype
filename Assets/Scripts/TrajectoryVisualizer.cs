using UnityEngine;

public class TrajectoryVisualizer : MonoBehaviour
{
    [SerializeField] private Transform a;
    [SerializeField] private Transform b;
    [SerializeField] private Transform control;

    private LineRenderer lineRenderer;

    private void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
    }

    private void Update()
    {
        DrawTrajectory();
    }

    private void DrawTrajectory() 
    {
        int linePositions = 40;

        lineRenderer.positionCount = linePositions;

        for (int i = 0; i < linePositions; i++)
        {
            lineRenderer.SetPosition(i, QuadraticCurve.EvaluateCurve(a.position, b.position, control.position, i / 40f));
        }
    }
}
