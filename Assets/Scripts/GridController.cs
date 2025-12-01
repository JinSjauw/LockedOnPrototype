using System.IO;
using UnityEngine;
using UnityEngine.AI;

public class GridController : MonoBehaviour
{
    [SerializeField] private Material gridPatternShader;
    [SerializeField] private Vector4[] heights = new Vector4[2];   // 8 floats
    [SerializeField] private Vector4[] pos = new Vector4[8];

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        gridPatternShader = GetComponent<Renderer>().material;

        //gridPatternShader.SetInt("_BumpCount", 8);
        //gridPatternShader.SetVectorArray("_BumpWorldPos", pos);
        //gridPatternShader.SetVectorArray("_BumpHeight", heights);
        //gridPatternShader.SetFloat("_BumpWidth", 0.5f);
    }

    // Update is called once per frame
    void Update()
    {
        ////Get point in world space from screen space.
        //RaycastHit hit;
        //Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        ////Use navmesh to pathfind
        //if (Physics.Raycast(ray, out hit))
        //{
        //    Vector3 worldPosition = hit.point;
        //    //transform.position = new Vector3(worldPosition.x, .3f, worldPosition.z);
        //}

        //for (int i = 0; i < 8; ++i)
        //{
        //    heights[i / 4][i % 4] = heightPerBump;
        //    pos[i] = targets[i].position;   // world space
        //}

        gridPatternShader.SetInt("_BumpCount", 8);
        gridPatternShader.SetVectorArray("_BumpWorldPositions", pos);
        gridPatternShader.SetVectorArray("_BumpHeights", heights);
        gridPatternShader.SetFloat("_BumpWidths", 0.22f);

        gridPatternShader.SetVector("_Center", transform.position);
    }
}
