using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

public class SonarSignalLocker : MonoBehaviour
{
    [SerializeField] private UIEvents uiEvents;
    [SerializeField] private ActiveSensingChannel activeSensingChannel;

    [SerializeField] private float outerRadius;
    [SerializeField] private float speed;
    [SerializeField] private Vector2 blockSize;

    [SerializeField] private float bounceSpeed;
    [SerializeField] private float bounceBounds;
    private float xVelocity;

    [SerializeField] private float bounceLockout;
    private float bounceLockoutTimer;

    [SerializeField] private Transform quadTransform;
    [SerializeField] private MeshRenderer meshRender;

    [SerializeField] private Transform testBlock;
    [SerializeField] private GameObject blockColliderPrefab;

    private MaterialPropertyBlock propertyBlock;

    //BufferData

    [SerializeField] private int amountOfBlocks;
    private int totalBlocksHit;

    private ComputeBuffer signalBlockDataBuffer;
    BlockData[] blockDataArray;
    Vector3[] blockPositionsArray;
    Transform[] blockCollidersArray;

    private ObjectPool pool;
    private bool hasInitialized;
    private bool isRunning;

    private int trackIndex;

    private void OnBounceSonarSignal(InputValue value) 
    {
        if (value.isPressed) 
        {
            //Change travelDirection
            if(bounceLockoutTimer <= 0) 
            {
                xVelocity = -xVelocity;
            }
        }
    }

    private void Start()
    {
        pool = FindFirstObjectByType<ObjectPool>();
        
        propertyBlock = new MaterialPropertyBlock();

        signalBlockDataBuffer = new ComputeBuffer(amountOfBlocks, 5 * sizeof(float), ComputeBufferType.Default);

        blockDataArray = new BlockData[amountOfBlocks];
        blockPositionsArray = new Vector3[amountOfBlocks];
        blockCollidersArray = new Transform[amountOfBlocks];

        if (!hasInitialized) 
        {
            CreateBlockSet(amountOfBlocks);
            CreateBlockColliders(amountOfBlocks);

            hasInitialized = true;
        }

        testBlock.localScale = new Vector3(blockSize.x, blockSize.y, 0.1f);

        testBlock.GetComponent<CollisionChecker>().CollisionEvent += OnCollision;

        xVelocity = bounceSpeed;

        uiEvents.sonarSuperLockButtonEvent += OnOpenLocker;
        uiEvents.sonarLockLossEvent += OnCloseLocker;
    }

    private void Update()
    {
        if (!isRunning) return;

        propertyBlock.SetFloat("_Speed", speed);

        UpdateBlockSetPosition();
        UpdateMaterial();
        CheckEnd();
    }

    private void FixedUpdate()
    {
        BouncePoint();
        UpdateBlockColliders();
    }

    private void OnOpenLocker(object sender, int trackIndex)
    {
        //Open this UI
        //Create Set!

        if (isRunning) return;

        this.trackIndex = trackIndex;

        CreateBlockSet(amountOfBlocks);
        CreateBlockColliders(amountOfBlocks);

        Vector3 position = testBlock.localPosition;
        position.x = 0;
        testBlock.localPosition = position;

        quadTransform.gameObject.SetActive(true);
        isRunning = true;
    }

    private void OnCloseLocker(object sender, int lockIndex) 
    {
        quadTransform.gameObject.SetActive(false);
        isRunning = false;
    }

    private void OnCollision(object sender, Collider collider)
    {
        SonarSignalBlock signalBlock = collider.GetComponent<SonarSignalBlock>();
        
        if (blockDataArray[signalBlock.Index].hitIndex > 0) return;

        blockDataArray[signalBlock.Index].hitIndex = 1; //Change when Hit
        totalBlocksHit++;
    }

    private void BouncePoint() 
    {
        if (bounceLockoutTimer > 0)
        {
            bounceLockoutTimer -= Time.deltaTime;
        }

        Vector3 position = testBlock.localPosition;

        if (Mathf.Abs(position.x) > bounceBounds) 
        {
            xVelocity = -xVelocity;
            bounceLockoutTimer = bounceLockout;
        }

        if(position.x > bounceBounds && position.x + xVelocity > bounceBounds) 
        {
            position.x = bounceBounds;
            xVelocity = -xVelocity;

            bounceLockoutTimer = bounceLockout;
        }

        if (position.x < -bounceBounds && position.x + xVelocity < -bounceBounds)
        {
            position.x = -bounceBounds;
            xVelocity = -xVelocity;

            bounceLockoutTimer = bounceLockout;
        }

        position.x += xVelocity * Time.deltaTime;
        testBlock.localPosition = position;
    }

    private void CheckEnd() 
    {
        if (blockDataArray.Length <= 0) return;

        BlockData lastBlock = blockDataArray[blockDataArray.Length - 1];

        if (lastBlock.hitIndex > 0 || lastBlock.position.y < -1) 
        {
            float score = (totalBlocksHit - 0) / (amountOfBlocks - 0);

            score = Mathf.Clamp(score, 0.2f, 1);

            activeSensingChannel.OnSignalLocked(trackIndex, score);

            //Get rid of the other one only when going super lock
            //uiEvents.OnSonarLockLoss(1);

            quadTransform.gameObject.SetActive(false);
            isRunning = false;
        }
    }

    private void UpdateBlockColliders()
    {
        for (int i = 0; i < blockPositionsArray.Length; i++) 
        {
            blockCollidersArray[i].position = blockPositionsArray[i];
        }
    }

    private void UpdateMaterial() 
    {
        signalBlockDataBuffer.SetData(blockDataArray);
        propertyBlock.SetBuffer("_BlockDataBuffer", signalBlockDataBuffer);
        propertyBlock.SetInt("_BlockCount", blockDataArray.Length);
        meshRender.SetPropertyBlock(propertyBlock);
    }

    private void UpdateBlockSetPosition() 
    {
        for (int i = 0; i < blockDataArray.Length; i++) 
        {
            Vector2 blockPosition = blockDataArray[i].position;
            blockPosition.y -= speed * Time.deltaTime;

            blockDataArray[i].position = blockPosition;
            blockPositionsArray[i] = BlockCentreToWorld(blockPosition);
        }
    }

    private void CreateBlockSet(int amount) 
    {
        float positionStagger = 0;

        for (int i = 0; i < amount; i++) 
        {
            Vector2 size = blockSize;
            size.x = Random.Range(0.03f, 0.055f);
            size.y = Random.Range(0.04f, 0.15f);

            float yAxisRange = (size.x - 0.75f);
            float randomYPosition = Random.Range(-yAxisRange, yAxisRange);

            float randomPositionStagger = Random.Range(0.5f, 1f);

            positionStagger += randomPositionStagger; //positional stagger/offset
            Vector2 position = new Vector2(randomYPosition, outerRadius + positionStagger);
            //position.x -= position.normalized.x * speed * delta;

            float hitIndex = 0;

            BlockData blockData = new BlockData(position, size, hitIndex);

            blockDataArray[i] = blockData;
            blockPositionsArray[i] = BlockCentreToWorld(position);
        }

        signalBlockDataBuffer.SetData(blockDataArray);
    }

    private void CreateBlockColliders(int amountOfBlocks)
    {
        for (int i = 0; i < amountOfBlocks; i++)
        {
            //Create and instantiate
            GameObject blockCollider = pool.GetObject(blockColliderPrefab);
            blockCollider.transform.parent = transform;

            Vector2 blockSize = blockDataArray[i].size;
            //Vector3 quadSize = quadTransform.localScale;

            blockCollider.transform.localScale = new Vector3(blockSize.x, blockSize.y, 0.1f);

            blockCollider.transform.position = blockPositionsArray[i];
            blockCollidersArray[i] = blockCollider.transform;
            blockCollider.GetComponent<SonarSignalBlock>().SetIndex(i);
        }
    }

    private Vector3 BlockCentreToWorld(Vector2 blockLocalCentre)
    {
        // 1. normalise to -0.5..0.5 quad space
        Vector3 local = new Vector3(blockLocalCentre.x / (outerRadius * 2),
                                    blockLocalCentre.y / (outerRadius * 2), 0);
        // 2. transform
        return quadTransform.TransformPoint(local);
    }
}

public struct BlockData 
{
    public Vector2 position;
    public Vector2 size;
    public float hitIndex;

    public BlockData(Vector2 position, Vector2 size, float hitIndex) 
    {
        this.position = position;   
        this.size = size;
        this.hitIndex = hitIndex;
    }
}
