using UnityEngine;

[CreateAssetMenu(fileName = "PointCloudParams", menuName = "Scriptable Objects/PointCloudParams")]
public class PointCloudParams : ScriptableObject
{
    public int pointAmount;
    public float radius;
    public float densityFalloff;
}
