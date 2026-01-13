using Ami.BroAudio;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

public class LevelManager : MonoBehaviour
{
    [SerializeField] private UIEvents uiEvents;
    [SerializeField] private SoundConfig soundConfig;

    [SerializeField] private GameObject artilleryPrefab;
    [SerializeField] private GameObject scoutPrefab;
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private GameObject playerHolder;

    private string artEnemyID = "ART";
    private string sctEnemyID = "SCT";

    [SerializeField] private List<Transform> spawnPointsPlayer;
    [SerializeField] private List<Transform> spawnPointsEnemies;

    [SerializeField] private int maxWaveAmount;
    [SerializeField] private int enemiesPerWaveIncrement;

    private List<EnemyController> enemiesList;
    private ObjectPool objectPool;
    private Transform playerTransform;

    private int totalEnemiesSpawned;
    private int enemiesSpawned;
    private int waveNumber = 0;

    private int enemiesToSpawn;

    private void Awake()
    {
        objectPool = FindFirstObjectByType<ObjectPool>();
        enemiesList = new List<EnemyController>();

        enemiesToSpawn = 1;

        Time.timeScale = 1.0f;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        BroAudio.Play(soundConfig.defaultAmbience).AsBGM();
        
        SpawnPlayer();

        SpawnWave(enemiesToSpawn);

    }

    private bool SamplePosition(GameObject spawnedObject, Vector3 positionToSample, Action<Vector3> success, Action failure) 
    {
        if (NavMesh.SamplePosition(positionToSample, out NavMeshHit hit, float.MaxValue, LayerMask.GetMask("Ground")))
        {
            success(hit.position);

            return true;
        }
        else
        {
            failure();
        }

        return false;
    }

    private void SpawnPlayer() 
    {

        Debug.Log("Spawning Player!");

        int randomSpawnIndex = Random.Range(0, spawnPointsPlayer.Count - 1);
        Vector3 randomSpawn = spawnPointsPlayer[randomSpawnIndex].position;

        GameObject playerObject = Instantiate(playerPrefab);
        playerObject.transform.parent = playerHolder.transform;

        playerObject.transform.position = randomSpawn;

        playerTransform = playerObject.GetComponentInChildren<NavMeshAgent>().transform;
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

        string ID = enemyType + "_" + totalEnemiesSpawned;

        spawnedEnemy.InitializeEnemy(playerTransform, spawnCenterPosition, ID);

        spawnedEnemy.OnDestructionEvent += OnEnemyDestruction;

        //if (NavMesh.SamplePosition(spawnCenterPosition, out NavMeshHit hit, 1000, LayerMask.GetMask("Ground"))) 
        //{
        //    spawnedEnemy.InitializeEnemy(playerTransform, hit.position, ID);
        //}
        //else 
        //{
        //    objectPool.ReturnGameObject(spawnedEnemy.gameObject);
        //    enemiesList.Remove(spawnedEnemy);
        //}
    }

    private void OnEnemyDestruction(object sender, EnemyController enemy)
    {
        if (enemiesList.Contains(enemy)) 
        {
            enemiesList.Remove(enemy);
        }

        if(enemiesList.Count == 0) 
        {
            enemiesToSpawn += enemiesPerWaveIncrement;
            SpawnWave(enemiesToSpawn);
        }
    }

    private void SpawnWave(int enemyAmount) 
    {
        enemiesSpawned = 0;

        waveNumber++;

        for (int i = 0; i < enemyAmount; i++)
        {
            int randomSpawnIndex = Random.Range(0, spawnPointsEnemies.Count - 1);
            Vector3 randomSpawn = spawnPointsEnemies[randomSpawnIndex].position;

            int enemyTypeIndex = Random.Range(-5, 5);
            string enemyType = enemyTypeIndex > 0 ? artEnemyID : sctEnemyID;

            SpawnEnemy(enemyType, randomSpawn);

            totalEnemiesSpawned++;
            enemiesSpawned++;
        }

        uiEvents.OnNewWaveEvent(waveNumber, enemiesToSpawn);
        uiEvents.OnEnemyAmountChangedEvent(enemiesSpawned);
    }
}
