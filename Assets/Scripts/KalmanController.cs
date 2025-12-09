using UnityEngine;

public class KalmanController : MonoBehaviour
{
    [SerializeField] private Material kalmanShader;
    [SerializeField] private Terrain terrainObject;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Vector3 origin = terrainObject.transform.position;
        Vector3 size = terrainObject.terrainData.size;
        Texture heightMap = terrainObject.terrainData.heightmapTexture;

        kalmanShader.SetTexture("_HeightMap", heightMap);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
