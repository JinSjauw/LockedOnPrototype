using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "PointCloudParams", menuName = "Scriptable Objects/PointCloudParams")]
public class PointCloudParams : ScriptableObject
{
    [SerializeField] private PointCloudData[] pointCloudDataArray;
    [SerializeField] private AnimationCurve densityFallOffScoreMultiplier;
    [SerializeField] private AnimationCurve radiusScoreMultiplier;
    [SerializeField] private AnimationCurve pointAmountScoreMultiplier;

    public PointCloudData GetPointData(PointCloudType type) 
    {
        PointCloudData pointCloudData = pointCloudDataArray.Where(x => x.type == type).FirstOrDefault();

        return pointCloudData;
    }

    public PointCloudData GetScoredPointData(PointCloudType typeToGet, float score)
    {
        PointCloudData pointCloudData = GetPointData(typeToGet);

        pointCloudData.densityFalloff *= densityFallOffScoreMultiplier.Evaluate(score);
        pointCloudData.radius *= radiusScoreMultiplier.Evaluate(score);
        pointCloudData.pointAmount = (int)((float)pointCloudData.pointAmount * pointAmountScoreMultiplier.Evaluate(score));

        //Debug.Log("Returning:" + pointCloudData.densityFalloff + " : " + pointCloudData.radius + " : " + pointCloudData.pointAmount);

        return pointCloudData;
    }
}

[System.Serializable]
public struct PointCloudData 
{
    public PointCloudType type;
    public int pointAmount;
    public float radius;
    public float densityFalloff;
}

public enum PointCloudType
{
    DEFAULT = 0,
    DEFAULT_MOVING = 1,
    DEFAULT_STATIONARY = 2,
    FOCUSED_MOVING = 3,
    FOCUSED_STATIONARY = 4,
}
