using UnityEngine;
using UnityEngine.TerrainTools;
using UnityEngine.TerrainUtils;

public class KalmanController : MonoBehaviour
{
    [SerializeField] private Material kalmanShader;
    [SerializeField] private Terrain terrainObject;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        TerrainData data = terrainObject.terrainData;

        Vector3 origin = terrainObject.transform.position;
        Vector3 size = data.size;
        Texture heightMap = data.heightmapTexture;

        kalmanShader.SetTexture("_HeightMap", heightMap);
        kalmanShader.SetVector("_TerrainOrigin", new Vector2(origin.x, origin.z));
        kalmanShader.SetVector("_TerrainSize", new Vector2(size.x, size.z));
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
