using TMPro;
using UnityEngine;

public class EnemyTrajectoryController : MonoBehaviour
{
    [Header("Component Refs")]
    [SerializeField] private Transform previewMuzzleTransform;
    [SerializeField] private Transform muzzleTransform;
    [SerializeField] private Transform targetTransform;

    [SerializeField] private CurveController previewCurveController;
    [SerializeField] private CurveController curveController;
    [SerializeField] private EnemyTargetingController targetingController;

    [Header("Trajectory Variables")]
    [SerializeField] private int attemptsPerFrame = 2;
    [SerializeField] private int maxTotalAttempts = 8;
    [SerializeField] private float trajectoryAdjustmentStep = 1;

    private int attemptsTotal = 0;

    private float curveHeight;

    private Vector3 targetPosition;
    private bool hasTrajectory;
    private bool hasTarget;
    private bool hasFailed;
   
    public bool HasTrajectory { get => hasTrajectory; }
    public bool HasFailed { get => hasFailed; }
    public bool HasTarget { get => hasTarget; }

    #region Functions

    public void SetTarget(Vector3 position) 
    {
        targetingController.SetPreviewTarget(position);
        targetPosition = position;
        hasTarget = true;

    }

    public void FindTrajectory(float startingHeight)
    {
        hasFailed = false;
        hasTrajectory = false;

        //Maybe do this a few times per frame. for optimization
        int linePositions = 8;

        Vector3 startPosition = previewMuzzleTransform.position;
        Vector3 endPosition = targetPosition;

        if (attemptsTotal == 0)
        {
            curveHeight = startingHeight;
            previewCurveController.SetHeight(startingHeight);
        }

        Vector3 oldPosition = startPosition;
        Vector3 newPosition;

        for (int i = 0; i < attemptsPerFrame && !hasTrajectory; i++)
        {

            for (int j = 0; j < linePositions; j++)
            {
                newPosition = QuadraticCurve.EvaluateCurve(startPosition, endPosition, previewCurveController.transform.position, (float)j / (float)linePositions);

                Debug.DrawLine(oldPosition, newPosition, i % 2 == 0 ? Color.red : Color.blue, 2);
                if (Physics.Linecast(oldPosition, newPosition, out RaycastHit hit, LayerMask.GetMask("Ground")))
                {
                    curveHeight += trajectoryAdjustmentStep;
                    previewCurveController.SetHeight(curveHeight);

                    hasTrajectory = false;
                    break;
                }

                if (j + 1 >= linePositions)
                {
                    hasTrajectory = true;
                    break;
                }
                oldPosition = newPosition;
            }
        }

        attemptsTotal++;

        if (hasTrajectory)
        {
            Debug.Log("Found Trajectory");
            targetingController.SetTarget(targetPosition);
            curveController.InterpolateTo = true;
            hasFailed = false;
            //Add extra margin for bullet girth;
            curveController.DesiredCurveHeight = curveHeight + trajectoryAdjustmentStep;
            attemptsTotal = 0;
        }

        if (attemptsTotal >= maxTotalAttempts && !hasTrajectory)
        {
            attemptsTotal = 0;
            hasTarget = false;
            hasFailed = true;
            Debug.Log("Failed to find trajectory...");
        }
    }

    public void ResetTries() 
    {
        hasFailed = false;
    }

    public void ResetTrajectory()
    {
        hasTarget = false;
        hasTrajectory = false;
    }

    #endregion
}
