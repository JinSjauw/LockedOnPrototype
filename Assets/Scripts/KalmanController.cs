using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UIElements;
using static UnityEngine.Rendering.VirtualTexturing.Debugging;

public class KalmanController : MonoBehaviour
{
    [SerializeField] private Material kalmanPointMaterial;
    [SerializeField] private Material kalmanShader;
    [SerializeField] private ComputeShader pointCloudCalc;

    [SerializeField] private Terrain terrainObject;
    [SerializeField] private Mesh kalmanPointMesh;

    [SerializeField] private int rawSamples;
    [SerializeField] private int targetPointAmount;
    [SerializeField] private float kalmanRadius;
    [SerializeField] private float densityFallof;
    [SerializeField] private float heightOffset;

    ComputeBuffer posBuffer;
    ComputeBuffer argBuffer;
    ComputeBuffer counter;

    int kernel;

    Bounds drawBounds;

    const int THREADS_AMOUNT = 64;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Awake()
    {
        TerrainData data = terrainObject.terrainData;

        Vector3 origin = terrainObject.transform.position;
        Vector3 size = data.size;
        Texture heightMap = data.heightmapTexture;

        posBuffer = new ComputeBuffer(targetPointAmount, sizeof(float) * 4, ComputeBufferType.Append);
        argBuffer = new ComputeBuffer(1, sizeof(uint) * 5, ComputeBufferType.IndirectArguments);
        counter = new ComputeBuffer(1, 4, ComputeBufferType.Default);

        kernel = pointCloudCalc.FindKernel("CSMain");
        if (kernel < 0) Debug.Log("Kernel not found!");

        pointCloudCalc.SetInt("_SampleAmount", rawSamples);
        pointCloudCalc.SetBuffer(kernel, "_Positions", posBuffer);
        pointCloudCalc.SetBuffer(kernel, "_Counters", counter);
        pointCloudCalc.SetInt("_MaxAmount", targetPointAmount);
        pointCloudCalc.SetFloat("_DensityFalloff", densityFallof);
        pointCloudCalc.SetVector("_TerrainOrigin", origin);
        pointCloudCalc.SetVector("_TerrainSize", size);

        pointCloudCalc.SetTexture(kernel, "_HeightMap", data.heightmapTexture);

        kalmanPointMaterial.SetBuffer("_Positions", posBuffer);

        //drawBounds = new Bounds(origin + size * 0.5f, size);
        float boundSize = kalmanRadius + 2;
        drawBounds = new Bounds(transform.position, Vector3.one * boundSize * 2);

        kalmanShader.SetTexture("_HeightMap", heightMap);
        kalmanShader.SetVector("_TerrainOrigin", new Vector2(origin.x, origin.z));
        kalmanShader.SetVector("_TerrainSize", new Vector2(size.x, size.z));

        ResetPointCloud();
        InvokeRepeating(nameof(ResetPointCloud), 1, 4);
    }

    // Update is called once per frame
    private void Update()
    {
        Graphics.DrawMeshInstancedIndirect(kalmanPointMesh, 0, kalmanPointMaterial, drawBounds, argBuffer, castShadows: ShadowCastingMode.Off, receiveShadows: false);
    }

    private void OnDestroy()
    {
        posBuffer?.Release();
        argBuffer?.Release();
        counter?.Release();
    }

    private void ResetPointCloud()
    {
        Vector3 position = transform.position;

        pointCloudCalc.SetFloat("_DensityFalloff", densityFallof);
        pointCloudCalc.SetInt("_SampleAmount", rawSamples);
        pointCloudCalc.SetVector("_Center", position);
        pointCloudCalc.SetFloat("_Radius", kalmanRadius);
        pointCloudCalc.SetFloat("_HeightOffset", heightOffset);

        uint[] zero = { 0 };
        counter.SetData(zero);
        posBuffer.SetCounterValue(0);
        //Debug.Log("Kernal: " + kernel);

        int groups = Mathf.Max(1, Mathf.CeilToInt(rawSamples / (float)THREADS_AMOUNT));

        pointCloudCalc.Dispatch(kernel, groups, 1, 1);
        ComputeBuffer.CopyCount(posBuffer, argBuffer, 4);

        Vector4[] cpuPositions = new Vector4[targetPointAmount];
        posBuffer.GetData(cpuPositions);

        for (int i = 0; i < 10; i++)
            Debug.Log(cpuPositions[i]);

        uint[] args = new uint[5];
        argBuffer.GetData(args);

        args[0] = (uint)kalmanPointMesh.GetIndexCount(0);
        args[1] = (uint)Mathf.Min(args[1], targetPointAmount);
        argBuffer.SetData(args);
        Debug.Log($"ARGS: indexCount={args[0]} instanceCount={args[1]} startIndex={args[2]} baseVertex={args[3]} startInstance={args[4]}");
        //Debug.Log($"indexCount={args[0]}  instanceCount={args[1]}  posBuffer.count={args[2]}");

    }
}
