using System;
using System.Threading;
using UnityEngine;
using UnityEngine.Rendering;

public class KalmanController : MonoBehaviour
{
    [SerializeField] private Material kalmanPointMaterial;

    [SerializeField] private Terrain terrainObject;
    [SerializeField] private Mesh kalmanPointMesh;
    [SerializeField] private ComputeShader pointCloudComputeShader;


    [SerializeField] private int rawSamples;
    [SerializeField] private int targetPointAmount;
    [SerializeField] private float kalmanRadius;
    [SerializeField] private float densityFallof;
    [SerializeField] private float heightOffset;
    [SerializeField] private float heightCutoff;
    [SerializeField] private float angleCutoff;

    [SerializeField] private float timeToFinish;
    [SerializeField] private float perPrintDelay;

    private float printTimer;
    private float printDelayTimer;

    const int THREADS_AMOUNT = 64;

    private ComputeShader compute;

    private ComputeBuffer startTimeBuffer;
    private ComputeBuffer positionBuffer;
    private ComputeBuffer argBuffer;
    private ComputeBuffer counterBuffer;
    private ComputeBuffer printBuffer;
    uint[] printArray = new uint[5];
    uint[] totalArray = new uint[5];
    float[] startTimeArray;

    int kernel;
    int printIndex = 0;

    private Bounds drawBounds;
    private Material instancedPointMaterial;
    private MaterialPropertyBlock gpuData;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Awake()
    {
        //Timer Data
        perPrintDelay = timeToFinish / targetPointAmount;
        startTimeArray = new float[targetPointAmount];

        //Terrain data
        TerrainData data = terrainObject.terrainData;

        Vector3 origin = terrainObject.transform.position;
        Vector3 size = data.size;
        Texture heightMap = data.heightmapTexture;
        Texture normalMap = Terrain.activeTerrain.normalmapTexture;
        Matrix4x4 terrainToWorld = terrainObject.transform.localToWorldMatrix;

        kalmanPointMesh.RecalculateBounds();

        compute = Instantiate(pointCloudComputeShader);

        startTimeBuffer = new ComputeBuffer(targetPointAmount, sizeof(float), ComputeBufferType.Default);
        positionBuffer = new ComputeBuffer(targetPointAmount, sizeof(float) * 4, ComputeBufferType.Append);
        argBuffer = new ComputeBuffer(1, sizeof(uint) * 5, ComputeBufferType.IndirectArguments);
        counterBuffer = new ComputeBuffer(1, 4, ComputeBufferType.Default);
        printBuffer = new ComputeBuffer(1, sizeof(uint) * 5, ComputeBufferType.IndirectArguments);

        kernel = compute.FindKernel("CSMain");
        if (kernel < 0) Debug.Log("Kernel not found!");

        compute.SetInt("_SampleAmount", rawSamples);
        compute.SetBuffer(kernel, "_Positions", positionBuffer);
        compute.SetBuffer(kernel, "_Counters", counterBuffer);

        compute.SetInt("_MaxAmount", targetPointAmount);
        compute.SetFloat("_DensityFalloff", densityFallof);

        compute.SetMatrix("_TerrainToWorld", terrainToWorld);
        compute.SetVector("_TerrainOrigin", origin);
        compute.SetVector("_TerrainSize", size);

        compute.SetTexture(kernel, "_HeightMap", data.heightmapTexture);
        compute.SetTexture(kernel, "_NormalMap", normalMap);

        instancedPointMaterial = new Material(kalmanPointMaterial);

        instancedPointMaterial.SetTexture("_NormalMap", normalMap);
        instancedPointMaterial.SetVector("_TerrainOrigin", origin);
        instancedPointMaterial.SetVector("_TerrainSize", size);

        gpuData = new MaterialPropertyBlock();
        gpuData.SetBuffer("_Positions", positionBuffer);
        gpuData.SetBuffer("_StartTime", startTimeBuffer);
    }

    // Update is called once per frame
    private void Update()
    {
        //PrintPointCloud(transform.position);
        UpdatePointData();
        Graphics.DrawMeshInstancedIndirect(kalmanPointMesh, 0, instancedPointMaterial, drawBounds, printBuffer, 0, gpuData, castShadows: ShadowCastingMode.Off, receiveShadows: false);
    }

    private void OnDisable()
    {
        Debug.Log($"OnDisable called on {name}  -  positionBuffer? {positionBuffer}  count {positionBuffer?.count}");
    }

    private void OnDestroy()
    {
        Debug.Log($"OnDisable called on {name}  -  positionBuffer? {positionBuffer}  count {positionBuffer?.count}");

        positionBuffer?.Release();
        argBuffer?.Release();
        counterBuffer?.Release();
        printBuffer?.Release();
    }

    private void CreatePointCloud(Vector3 position)
    {
 
        compute.SetFloat("_DensityFalloff", densityFallof);
        compute.SetVector("_Center", position);
        compute.SetFloat("_Radius", kalmanRadius);
        compute.SetFloat("_HeightOffset", heightOffset);
        compute.SetFloat("_HeightCutoff", heightCutoff);
        compute.SetFloat("_AngleCutoff", angleCutoff);

        float boundSize = kalmanRadius + 2;
        drawBounds = new Bounds(position, Vector3.one * boundSize * 2);

        uint[] zero = { 0 };
        counterBuffer.SetData(zero);
        positionBuffer.SetCounterValue(0);

        int groups = Mathf.Max(1, Mathf.CeilToInt(rawSamples / (float)THREADS_AMOUNT));

        compute.Dispatch(kernel, groups, 1, 1);
        ComputeBuffer.CopyCount(positionBuffer, argBuffer, 4);

        //Vector4[] cpuPositions = new Vector4[targetPointAmount];
        //positionBuffer.GetData(cpuPositions);

        //for (int i = 0; i < 10; i++)
        //    Debug.Log(cpuPositions[i] + " ID:" + name);

        //uint[] totalArray = new uint[5];
        totalArray = new uint[5];
        printArray = new uint[5];

        argBuffer.GetData(totalArray);

        totalArray[0] = (uint)kalmanPointMesh.GetIndexCount(0);
        totalArray[1] = (uint)Mathf.Min(totalArray[1], targetPointAmount);

        Array.Copy(totalArray, printArray, totalArray.Length);

        startTimeArray = new float[totalArray[1]];

        //printArray[1] = default;

        //"Point printing" effect

        //copy final array into different array
        //feed 2nd array only part of the whole array. 
        //Every frame stream more into the 2nd array while everyframe setting the argbuffer data

        //argBuffer.SetData(totalArray);
        //printBuffer.SetData(totalArray);
    }

    private void UpdatePointData() 
    {
        if (printTimer >= timeToFinish) return;

        printTimer += Time.deltaTime;
        //Stream data into printBuffer;

        if(printDelayTimer >= perPrintDelay) 
        {
            printArray[1] = (uint)printIndex;

            printBuffer.SetData(printArray);

            startTimeArray[printIndex] = Time.time;
            startTimeBuffer.SetData(startTimeArray);

            if(printIndex < totalArray[1]) 
            {
                printIndex++;
            }
            printDelayTimer = 0;
        }
        else 
        {
            printDelayTimer += Time.deltaTime;
        }
    }

    public void PrintPointCloud(Vector3 position) 
    {
        //if array not empty stream point into print array (the one fed to GPU draw func)
        //given total time to complete

        printIndex = 0;
        printTimer = 0;
        printDelayTimer = 0;

        //Index to keep progress
        CreatePointCloud(position);
    }

    public void SetPointColor(Color color) 
    {
        instancedPointMaterial.color = color;
    }
}
