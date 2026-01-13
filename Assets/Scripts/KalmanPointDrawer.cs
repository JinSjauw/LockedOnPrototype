using System;
using UnityEngine;
using UnityEngine.Rendering;

public class KalmanPointDrawer : MonoBehaviour
{
    [SerializeField] private Material kalmanPointMaterial;

    [SerializeField] private Terrain terrainObject;
    [SerializeField] private Mesh pointMesh;
    [SerializeField] private ComputeShader pointCloudComputeShader;

    [SerializeField] private int rawSamples;
    [SerializeField] private int maxPointAmount = 200;

    //Point Param Sets
    [SerializeField] private PointCloudParams defaultPointCloudParams;
    [SerializeField] private PointCloudParams movingPointParams;
    [SerializeField] private PointCloudParams stationaryPointParams;
    private PointCloudParams currentPointParams;

    [SerializeField] private float heightOffset;
    [SerializeField] private float heightCutoff;
    [SerializeField] private float angleCutoff;

    [SerializeField] private float timeToDim;
    [SerializeField] private AnimationCurve pointDimCurve;
    private float dimTimer;

    [SerializeField] private float timeToPrint;

    private float perPrintDelay;
    private float printTimer;
    private float printDelayTimer;

    const int THREADS_AMOUNT = 64;

    private ComputeShader compute;

    private ComputeBuffer startTimeBuffer;
    private ComputeBuffer positionBuffer;
    private ComputeBuffer oldPositionBuffer;
    private ComputeBuffer argBuffer;
    private ComputeBuffer oldArgBuffer;
    private ComputeBuffer counterBuffer;
    private ComputeBuffer printBuffer;

    Vector4[] printPositions;
    uint[] printArray = new uint[5];
    uint[] totalArray = new uint[5];

    float[] startTimeArray;

    int kernel;
    int printIndex = 0;

    private Mesh kalmanPointMesh;
    private Bounds drawBounds;
    private Material instancedPointMaterial;
    private MaterialPropertyBlock gpuData;
    private MaterialPropertyBlock oldGpuData;

    private bool isDrawing = false;

    private LineRenderer lineRenderer;
    private Vector3 lineOrigin;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Awake()
    {
        currentPointParams = defaultPointCloudParams;

        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.positionCount = 2;

        printPositions = new Vector4[maxPointAmount];

        //Timer Data
        perPrintDelay = timeToPrint / maxPointAmount;
        startTimeArray = new float[maxPointAmount];

        if(terrainObject == null) 
        {
            terrainObject = FindFirstObjectByType<Terrain>();
        }        

        //Terrain data
        TerrainData data = terrainObject.terrainData;

        Vector3 origin = terrainObject.transform.position;
        Vector3 size = data.size;

        Texture normalMap = Terrain.activeTerrain.normalmapTexture;
        Matrix4x4 terrainToWorld = terrainObject.transform.localToWorldMatrix;

        kalmanPointMesh = Instantiate(pointMesh);

        kalmanPointMesh.RecalculateBounds();

        compute = Instantiate(pointCloudComputeShader);

        startTimeBuffer = new ComputeBuffer(maxPointAmount, sizeof(float), ComputeBufferType.Default);
        positionBuffer = new ComputeBuffer(maxPointAmount, sizeof(float) * 4, ComputeBufferType.Append);
        oldPositionBuffer = new ComputeBuffer(maxPointAmount, sizeof(float) * 4, ComputeBufferType.Append);
        argBuffer = new ComputeBuffer(1, sizeof(uint) * 5, ComputeBufferType.IndirectArguments);
        oldArgBuffer = new ComputeBuffer(1, sizeof(uint) * 5, ComputeBufferType.IndirectArguments);
        counterBuffer = new ComputeBuffer(1, 4, ComputeBufferType.Default);
        printBuffer = new ComputeBuffer(1, sizeof(uint) * 5, ComputeBufferType.IndirectArguments);

        kernel = compute.FindKernel("CSMain");
        if (kernel < 0) Debug.Log("Kernel not found!");

        compute.SetInt("_SampleAmount", rawSamples);
        compute.SetBuffer(kernel, "_Positions", positionBuffer);
        compute.SetBuffer(kernel, "_Counters", counterBuffer);

        compute.SetInt("_MaxAmount", maxPointAmount);
        compute.SetFloat("_DensityFalloff", currentPointParams.densityFalloff);

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
       
        oldGpuData = new MaterialPropertyBlock();
        oldGpuData.SetBuffer("_Positions", oldPositionBuffer);
        oldGpuData.SetFloat("_FlashDuration", 0);
        oldGpuData.SetFloat("_Visibility", 1);
    }

    // Update is called once per frame
    private void Update()
    {
        if (!isDrawing) return;
             
        UpdatePrintData();
        DimOldPoints();

        Graphics.DrawMeshInstancedIndirect(kalmanPointMesh, 0, instancedPointMaterial, drawBounds, oldArgBuffer, 0, oldGpuData, castShadows: ShadowCastingMode.Off, receiveShadows: false);
        Graphics.DrawMeshInstancedIndirect(kalmanPointMesh, 0, instancedPointMaterial, drawBounds, printBuffer, 0, gpuData, castShadows: ShadowCastingMode.Off, receiveShadows: false);
    }

    private void OnDestroy()
    {
        positionBuffer?.Release();
        argBuffer?.Release();
        counterBuffer?.Release();
        printBuffer?.Release();
        startTimeBuffer?.Release();
        oldArgBuffer?.Release();
        oldPositionBuffer?.Release();
    }

    private void CreatePointCloud(Vector3 position)
    {
        compute.SetFloat("_DensityFalloff", currentPointParams.densityFalloff);
        compute.SetVector("_Center", position);
        compute.SetFloat("_Radius", currentPointParams.radius);
        compute.SetFloat("_HeightOffset", heightOffset);
        compute.SetFloat("_HeightCutoff", heightCutoff);
        compute.SetFloat("_AngleCutoff", angleCutoff);

        float boundSize = currentPointParams.radius + 2;
        drawBounds = new Bounds(position, 2 * boundSize * Vector3.one);

        uint[] zero = { 0 };
        counterBuffer.SetData(zero);
        positionBuffer.SetCounterValue(0);

        int groups = Mathf.Max(1, Mathf.CeilToInt(rawSamples / (float)THREADS_AMOUNT));

        compute.Dispatch(kernel, groups, 1, 1);
        ComputeBuffer.CopyCount(positionBuffer, argBuffer, 4);

        positionBuffer.GetData(printPositions);

        totalArray = new uint[5];
        printArray = new uint[5];

        argBuffer.GetData(totalArray);

        totalArray[0] = (uint)kalmanPointMesh.GetIndexCount(0);
        totalArray[1] = (uint)Mathf.Min(totalArray[1], currentPointParams.pointAmount);

        Array.Copy(totalArray, printArray, totalArray.Length);

        startTimeArray = new float[totalArray[1]];
    }

    private void UpdatePrintData() 
    {
        if (printTimer >= timeToPrint)
        {
            lineRenderer.positionCount = 0;

            return;
        }

        printTimer += Time.deltaTime;
        //Stream data into printBuffer;

        if(printDelayTimer >= perPrintDelay) 
        {
            printArray[1] = (uint)printIndex;

            printBuffer.SetData(printArray);

            if(printIndex < startTimeArray.Length) 
            {
                startTimeArray[printIndex] = Time.time;
            }

            startTimeBuffer.SetData(startTimeArray);

            if(printIndex < totalArray[1]) 
            {
                printIndex++;
            }

            //Flash a line to print position
            if(lineRenderer.positionCount > 0) 
            {
                lineRenderer.SetPosition(1, printPositions[printIndex]);
            }
            printDelayTimer = 0;
        }
        else 
        {
            printDelayTimer += Time.deltaTime;
        }
    }

    private void DimOldPoints() 
    {
        if (dimTimer >= timeToDim) return;

        dimTimer += Time.deltaTime;

        float dimAlpha = (dimTimer - 0) / (timeToDim - 0);

        float dimProgress = pointDimCurve.Evaluate(dimAlpha);

        oldGpuData.SetFloat("_Dim", 1 - dimProgress);
    }

    public void PrintPointCloud(Vector3 position, Vector3 markPosition) 
    {
        if (markPosition != Vector3.zero) 
        {
            lineRenderer.positionCount = 2;

            lineOrigin = markPosition;
            lineRenderer.SetPosition(0, lineOrigin);
        }
        else 
        {
            lineRenderer.positionCount = 0;
        }

        isDrawing = true;

        printIndex = 0;
        printTimer = 0;
        printDelayTimer = 0;

        dimTimer = 0;

        //If there is older data set olderData
        if(printArray.Length > 0 && positionBuffer.count > 0) 
        {
            oldArgBuffer.SetData(printArray);

            Vector4[] positions = new Vector4[currentPointParams.pointAmount];
            positionBuffer.GetData(positions);
            oldPositionBuffer.SetData(positions);
        }

        //Index to keep progress
        CreatePointCloud(position);
    }

    public void SetPointColor(Color color) 
    {
        instancedPointMaterial.color = color;
    }

    public void StopDrawing() 
    {
        isDrawing = false;

        //Clear all drawing data
        uint[] emptyArray = new uint[5];
        oldArgBuffer.SetData(emptyArray);

        Vector4[] emptyPositions = new Vector4[0];
        positionBuffer.SetData(emptyPositions);
        oldPositionBuffer.SetData(emptyPositions);

        Array.Clear(printArray, 0, printArray.Length);
    }

    public void SetPointParams(bool isStationary) 
    {
        if (isStationary) 
        {
            currentPointParams = stationaryPointParams;
        }
        else 
        {
            currentPointParams = movingPointParams;
        }
    }

    public bool IsDrawing() 
    {
        return isDrawing;
    }
}
