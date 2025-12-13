using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UIElements;
using static UnityEngine.Rendering.VirtualTexturing.Debugging;

public class KalmanController : MonoBehaviour
{
    [SerializeField] private Material kalmanPointMaterial;
    [SerializeField] private ComputeShader pointCloudCalc;

    [SerializeField] private Terrain terrainObject;
    [SerializeField] private Mesh kalmanPointMesh;

    [SerializeField] private int rawSamples;
    [SerializeField] private int targetPointAmount;
    [SerializeField] private float kalmanRadius;
    [SerializeField] private float densityFallof;
    [SerializeField] private float heightOffset;
    [SerializeField] private float heightCutoff;
    [SerializeField] private float angleCutoff;

    ComputeBuffer positionBuffer;
    ComputeBuffer argBuffer;
    ComputeBuffer counterBuffer;

    int kernel;

    Bounds drawBounds;

    const int THREADS_AMOUNT = 64;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Awake()
    {
        //Terrain data
        TerrainData data = terrainObject.terrainData;

        Vector3 origin = terrainObject.transform.position;
        Vector3 size = data.size;
        Texture heightMap = data.heightmapTexture;
        Texture normalMap = Terrain.activeTerrain.normalmapTexture;
        Matrix4x4 terrainToWorld = terrainObject.transform.localToWorldMatrix;

        positionBuffer = new ComputeBuffer(targetPointAmount, sizeof(float) * 4, ComputeBufferType.Append);
        argBuffer = new ComputeBuffer(1, sizeof(uint) * 5, ComputeBufferType.IndirectArguments);
        counterBuffer = new ComputeBuffer(1, 4, ComputeBufferType.Default);

        kernel = pointCloudCalc.FindKernel("CSMain");
        if (kernel < 0) Debug.Log("Kernel not found!");

        pointCloudCalc.SetInt("_SampleAmount", rawSamples);
        pointCloudCalc.SetBuffer(kernel, "_Positions", positionBuffer);
        pointCloudCalc.SetBuffer(kernel, "_Counters", counterBuffer);

        pointCloudCalc.SetInt("_MaxAmount", targetPointAmount);
        pointCloudCalc.SetFloat("_DensityFalloff", densityFallof);

        pointCloudCalc.SetMatrix("_TerrainToWorld", terrainToWorld);
        pointCloudCalc.SetVector("_TerrainOrigin", origin);
        pointCloudCalc.SetVector("_TerrainSize", size);

        pointCloudCalc.SetTexture(kernel, "_HeightMap", data.heightmapTexture);
        pointCloudCalc.SetTexture(kernel, "_NormalMap", normalMap);

        kalmanPointMaterial.SetBuffer("_Positions", positionBuffer);

        float boundSize = kalmanRadius + 2;
        drawBounds = new Bounds(transform.position, Vector3.one * boundSize * 2);

        kalmanPointMaterial.SetTexture("_NormalMap", normalMap);
        kalmanPointMaterial.SetVector("_TerrainOrigin", origin);
        kalmanPointMaterial.SetVector("_TerrainSize", size);

        ResetPointCloud();
        //InvokeRepeating(nameof(ResetPointCloud), 1, 2);
    }

    // Update is called once per frame
    private void Update()
    {
        ResetPointCloud();
        Graphics.DrawMeshInstancedIndirect(kalmanPointMesh, 0, kalmanPointMaterial, drawBounds, argBuffer, castShadows: ShadowCastingMode.Off, receiveShadows: false);
    }

    private void OnDestroy()
    {
        positionBuffer?.Release();
        argBuffer?.Release();
        counterBuffer?.Release();
    }

    private void ResetPointCloud()
    {
        Vector3 position = transform.position;

        pointCloudCalc.SetFloat("_DensityFalloff", densityFallof);
        pointCloudCalc.SetVector("_Center", position);
        pointCloudCalc.SetFloat("_Radius", kalmanRadius);
        pointCloudCalc.SetFloat("_HeightOffset", heightOffset);
        pointCloudCalc.SetFloat("_HeightCutoff", heightCutoff);
        pointCloudCalc.SetFloat("_AngleCutoff", angleCutoff);

        uint[] zero = { 0 };
        counterBuffer.SetData(zero);
        positionBuffer.SetCounterValue(0);

        int groups = Mathf.Max(1, Mathf.CeilToInt(rawSamples / (float)THREADS_AMOUNT));

        pointCloudCalc.Dispatch(kernel, groups, 1, 1);
        ComputeBuffer.CopyCount(positionBuffer, argBuffer, 4);

        uint[] args = new uint[5];
        argBuffer.GetData(args);

        args[0] = (uint)kalmanPointMesh.GetIndexCount(0);
        args[1] = (uint)Mathf.Min(args[1], targetPointAmount);

        //"Point printing" effect

        //copy final array into different array
        //feed 2nd array only part of the whole array. 
        //Every frame stream more into the 2nd array while everyframe setting the argbuffer data

        argBuffer.SetData(args);
    }
}
