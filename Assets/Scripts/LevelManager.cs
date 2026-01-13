using Ami.BroAudio;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class LevelManager : MonoBehaviour
{
    [SerializeField] private SoundConfig soundConfig;

    [SerializeField] private GameObject artilleryPrefab;
    [SerializeField] private GameObject scoutPrefab;
    [SerializeField] private GameObject playerPrefab;

    private string artEnemyID = "ART";
    private string sctEnemyID = "SCT";

    [SerializeField] private List<Transform> spawnPointsPlayer;
    [SerializeField] private List<Transform> spawnPointsEnemies;

    [SerializeField] private int waveAmount;
    [SerializeField] private int enemiesPerWave;

    private List<EnemyController> enemiesList;
    private ObjectPool objectPool;
    private Transform playerTransform;

    private void Awake()
    {
        objectPool = FindFirstObjectByType<ObjectPool>();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        BroAudio.Play(soundConfig.defaultAmbience).AsBGM();
        
    }

    private void SpawnPlayer() 
    {
    
    }

    private void SpawnEnemy(string enemyType, Vector3 spawnCenterPosition)
    {
        GameObject objectToSpawn = artilleryPrefab;

        if (enemyType == sctEnemyID)
        {
            objectToSpawn = scoutPrefab;
        }

        EnemyController spawnedEnemy = objectPool.GetObject(objectToSpawn).GetComponent<EnemyController>();

        //Generate ID!
        enemiesList.Add(spawnedEnemy);

        string ID = enemyType + "_" + enemiesList.Count;

        if (NavMesh.SamplePosition(spawnCenterPosition, out NavMeshHit hit, 1000, LayerMask.GetMask("Ground"))) 
        {
            spawnedEnemy.InitializeEnemy(playerTransform, hit.position, ID);
        }
        else 
        {
            objectPool.ReturnGameObject(spawnedEnemy.gameObject);
            enemiesList.Remove(spawnedEnemy);
        }
    }

    private void SpawnWave() 
    {
        
    }
    
    private void ReloadScene() 
    {
        
    }
}
