using UnityEngine;

public class IncrementTest : MonoBehaviour
{
    public ComputeShader cs;
    public int max = 300;

    ComputeBuffer pos;
    ComputeBuffer cnt;
    ComputeBuffer arg;

    void Start()
    {
        int k = cs.FindKernel("CSMainOne");

        pos = new ComputeBuffer(max, 16, ComputeBufferType.Append);
        cnt = new ComputeBuffer(1, 4, ComputeBufferType.Default);
        arg = new ComputeBuffer(1, 16, ComputeBufferType.IndirectArguments);

        cs.SetBuffer(k, "_Positions", pos);
        cs.SetBuffer(k, "_Counter", cnt);
        cs.SetInt("_MaxOut", max);

        uint[] zero = { 0 };
        cnt.SetData(zero);

        cs.Dispatch(k, 1, 1, 1);        // 64 threads

        ComputeBuffer.CopyCount(pos, arg, 4);
        uint[] live = new uint[4];
        arg.GetData(live);
        Debug.Log($"TEST  indexCount={live[0]}  instanceCount={live[1]}");
    }

    void OnDestroy()
    {
        pos?.Release();
        cnt?.Release();
        arg?.Release();
    }
}