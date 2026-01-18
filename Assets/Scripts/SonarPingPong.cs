using UnityEngine;
using UnityEngine.PlayerLoop;

public class SonarPingPong : MonoBehaviour
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

    private MaterialPropertyBlock propertyBlock;

    //BufferData

    [SerializeField] private int amountOfBlocks;
    private ComputeBuffer signalBlockDataBuffer;
    BlockData[] blockDataArray;

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
        propertyBlock = new MaterialPropertyBlock();

        //testBlock.localScale = new Vector3(blockSize.x * quadTransform.localScale.x, blockSize.y * quadTransform.localScale.x, 0.1f);
        signalBlockDataBuffer = new ComputeBuffer(amountOfBlocks, 5 * sizeof(float), ComputeBufferType.Default);

        blockDataArray = new BlockData[amountOfBlocks];

        CreateSignalSet(amountOfBlocks);
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

        UpdateBlocksetPosition();
        UpdateMaterial();
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

    private void UpdateBlocksetPosition() 
    {
        //Loop over array/data set and update positions
        //Extract out of array/buffer

        //Update
        //blockLocal.x -= blockLocal.normalized.x * speed * delta;
        //delta += Time.deltaTime;

        for (int i = 0; i < blockDataArray.Length; i++) 
        {
            Vector2 blockPosition = blockDataArray[i].position;
            blockPosition.x -= speed * Time.deltaTime;

            //Debug.Log(blockPosition.x);

            blockDataArray[i].position = blockPosition;
        }

        //Insert back into array and then buffer
    }

    private void CreateSignalSet(int amount) 
    {
        float positionStagger = 0;

        for (int i = 0; i < amount; i++) 
        {
            Vector2 size = blockSize;
            size.x = Random.Range(0.04f, 0.15f);
            size.y = Random.Range(0.03f, 0.075f);

            float yAxisRange = (size.y - 0.85f);
            float randomYPosition = Random.Range(-yAxisRange, yAxisRange);

            float randomPositionStagger = Random.Range(0.2f, 0.5f);

            positionStagger += 0.5f; //positional stagger/offset
            Vector2 position = new Vector2(outerRadius + positionStagger, randomYPosition);
            //position.x -= position.normalized.x * speed * delta;

            float colorIndex = 0;

            BlockData blockData = new BlockData(position, size, colorIndex);

            blockDataArray[i] = blockData;
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
