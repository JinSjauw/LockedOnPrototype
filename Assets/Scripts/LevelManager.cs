using Ami.BroAudio;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

public class LevelManager : MonoBehaviour
{
    [SerializeField] private SoundConfig soundConfig;

    [SerializeField] private GameObject artilleryPrefab;
    [SerializeField] private GameObject scoutPrefab;
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private GameObject playerHolder;

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
        enemiesList = new List<EnemyController>();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        BroAudio.Play(soundConfig.defaultAmbience).AsBGM();
        
        SpawnPlayer();

        SpawnWave(1);

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

        string ID = enemyType + "_" + enemiesList.Count;

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
            SpawnWave(4);
        }
    }

    private void SpawnWave(int enemyAmount) 
    {
        for (int i = 0; i < enemyAmount; i++)
        {
            int randomSpawnIndex = Random.Range(0, spawnPointsEnemies.Count - 1);
            Vector3 randomSpawn = spawnPointsEnemies[randomSpawnIndex].position;

            int enemyTypeIndex = Random.Range(-5, 5);
            string enemyType = enemyTypeIndex > 0 ? artEnemyID : sctEnemyID;

            SpawnEnemy(enemyType, randomSpawn);
        }
    }
    
    private void ReloadScene() 
    {
        
    }
}
