using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "PointCloudParams", menuName = "Scriptable Objects/PointCloudParams")]
public class PointCloudParams : ScriptableObject
{
    [SerializeField] private PointCloudData[] pointCloudDataArray;

    public int pointAmount;
    public float radius;
    public float densityFalloff;

    public PointCloudData GetPointData(PointCloudType type) 
    {
        PointCloudData pointCloudData = pointCloudDataArray.Where(x => x.type == type).FirstOrDefault();

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
