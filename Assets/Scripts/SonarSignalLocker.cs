using UnityEngine;
using UnityEngine.PlayerLoop;

public class SonarSignalLocker : MonoBehaviour
{
    [SerializeField] private float outerRadius;
    [SerializeField] private float innerRadius;
    [SerializeField] private float speed;
    [SerializeField] private float delta;
    [SerializeField] private float yPosition;
    [SerializeField] private Vector2 blockSize;

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

    //Generate a set of squares.
    //Needed per square are
    // - CenterPosition
    // - yPosition
    // - size -> maybe not needed
    // - Color -> to show when the player hit it. //ColorIndex
    // - Alpha to fade it out

    //Position check. Check if the selector point is within the x value & correct y value when it passes by. 
    //Check limits. 
    //Positive boundary = x - size - smallMargin
    //Negative boundary = x + size + smallMargin

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
    }

    private void OnCollision(object sender, Collider collider)
    {
        Debug.Log("Name: " + collider.name);
    }

    private void Update()
    {
        //Apply variables here
        //Vector2 blockLocal =  new Vector2(outerRadius, yPosition);
        ////delta += Time.deltaTime;
        //blockLocal.x -= blockLocal.normalized.x * speed * delta;

        //testBlock.position = BlockCentreToWorld(blockLocal);

        //propertyBlock.SetFloat("_OuterRadius", outerRadius);
        //propertyBlock.SetFloat("_Inner", innerRadius);
        propertyBlock.SetFloat("_Speed", speed);
        //propertyBlock.SetFloat("_Delta", delta);
        //propertyBlock.SetFloat("_PositionY", yPosition);
        //propertyBlock.SetVector("_HalfSize", blockSize);

        UpdateBlockSetPosition();
        UpdateMaterial();
        UpdateBlockColliders();
        CheckCollision();
    }

    private void CheckCollision()
    {
        for (int i = 0; i < blockCollidersArray.Length; i++) 
        {
            Vector3 position = blockCollidersArray[i].position;
            Vector3 halfSize = blockCollidersArray[i].localScale / 2;

            
        }
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
        //Loop over array/data set and update positions
        //Extract out of array/buffer

        //Update
        //blockLocal.x -= blockLocal.normalized.x * speed * delta;
        //delta += Time.deltaTime;

        for (int i = 0; i < blockDataArray.Length; i++) 
        {
            Vector2 blockPosition = blockDataArray[i].position;
            blockPosition.y -= speed * Time.deltaTime;

            //Debug.Log(blockPosition.x);

            blockDataArray[i].position = blockPosition;
            blockPositionsArray[i] = BlockCentreToWorld(blockPosition);
        }

        //Insert back into array and then buffer
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

            float colorIndex = 0;

            BlockData blockData = new BlockData(position, size, colorIndex);

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
    public float colorIndex;

    public BlockData(Vector2 position, Vector2 size, float colorIndex) 
    {
        this.position = position;   
        this.size = size;
        this.colorIndex = colorIndex;
    }
}
