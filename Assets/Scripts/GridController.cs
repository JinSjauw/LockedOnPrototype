using System.IO;
using UnityEngine;
using UnityEngine.AI;

public class GridController : MonoBehaviour
{
    [SerializeField] private Material gridPatternShader;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //gridPatternShader = GetComponent<Renderer>().material;
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
        
        gridPatternShader.SetVector("_Grid_Center", transform.position);
    }
}
