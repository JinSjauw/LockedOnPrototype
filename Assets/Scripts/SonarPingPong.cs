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

    private MaterialPropertyBlock sonarPingPong; 

    //Generate a set of squares.
    //Needed per square are
    // - CenterPosition
    // - yPosition
    // - size
    // - Color -> to show when the player hit it.
    // - Alpha to fade it out

    private void Start()
    {
        sonarPingPong = new MaterialPropertyBlock();

        testBlock.localScale = new Vector3(blockSize.x * quadTransform.localScale.x, blockSize.y * quadTransform.localScale.x, 0.1f);
    }

    private void Update()
    {
        //Apply variables here
        Vector2 blockLocal =  new Vector2(outerRadius, yPosition);
        delta += Time.deltaTime;
        blockLocal.x -= blockLocal.normalized.x * speed * delta;

        testBlock.position = BlockCentreToWorld(blockLocal);

        sonarPingPong.SetFloat("_OuterRadius", outerRadius);
        sonarPingPong.SetFloat("_Inner", innerRadius);
        sonarPingPong.SetFloat("_Speed", speed);
        sonarPingPong.SetFloat("_Delta", delta);
        sonarPingPong.SetFloat("_PositionY", yPosition);
        sonarPingPong.SetVector("_HalfSize", blockSize);

        meshRender.SetPropertyBlock(sonarPingPong);
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
