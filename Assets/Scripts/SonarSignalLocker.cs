using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.PlayerLoop;

public class SonarSignalLocker : MonoBehaviour
{
    [SerializeField] private float outerRadius;
    [SerializeField] private float speed;
    [SerializeField] private Vector2 blockSize;

    [SerializeField] private float bounceSpeed;
    [SerializeField] private float bounceBounds;
    private float xVelocity;

    [SerializeField] private Transform quadTransform;
    [SerializeField] private MeshRenderer meshRender;

    [SerializeField] private Transform testBlock;
    [SerializeField] private GameObject blockColliderPrefab;

    private MaterialPropertyBlock propertyBlock;

    //BufferData

    [SerializeField] private int amountOfBlocks;
    private ComputeBuffer signalBlockDataBuffer;
    BlockData[] blockDataArray;
    Vector3[] blockPositionsArray;
    Transform[] blockCollidersArray;

    private ObjectPool pool;

    private void OnBounceSonarSignal(InputValue value) 
    {
        if (value.isPressed) 
        {
            //Change travelDirection
            xVelocity = -xVelocity;
        }
    }

    private void Start()
    {
        pool = FindFirstObjectByType<ObjectPool>();
        
        propertyBlock = new MaterialPropertyBlock();

        //testBlock.localScale = new Vector3(blockSize.x * quadTransform.localScale.x, blockSize.y * quadTransform.localScale.x, 0.1f);
        signalBlockDataBuffer = new ComputeBuffer(amountOfBlocks, 5 * sizeof(float), ComputeBufferType.Default);

        blockDataArray = new BlockData[amountOfBlocks];
        blockPositionsArray = new Vector3[amountOfBlocks];
        blockCollidersArray = new Transform[amountOfBlocks];

        CreateBlockSet(amountOfBlocks);
        CreateBlockColliders(amountOfBlocks);

        testBlock.localScale = new Vector3(blockSize.x, blockSize.y, 0.1f);

        testBlock.GetComponent<CollisionChecker>().CollisionEvent += OnCollision;

        xVelocity = bounceSpeed;
    }

    private void Update()
    {
        propertyBlock.SetFloat("_Speed", speed);

        UpdateBlockSetPosition();
        UpdateMaterial();
        UpdateBlockColliders();
        //CheckCollision();
    }

    private void LateUpdate()
    {
        BouncePoint();
    }

    private void OnCollision(object sender, Collider collider)
    {

        SonarSignalBlock signalBlock = collider.GetComponent<SonarSignalBlock>();
        blockDataArray[signalBlock.Index].hitIndex = 1; //Change when Hit
    }

    private void BouncePoint() 
    {
        Vector3 position = testBlock.localPosition;

        if (Mathf.Abs(position.x) > bounceBounds) xVelocity = -xVelocity;

        position.x += xVelocity * Time.deltaTime;
        testBlock.localPosition = position;
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

    private void UpdateBlockColliders()
    {
        for (int i = 0; i < blockPositionsArray.Length; i++) 
        {
            blockCollidersArray[i].position = blockPositionsArray[i];
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
