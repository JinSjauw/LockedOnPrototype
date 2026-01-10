using TMPro;
using UnityEngine;

public class SonarMark : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI textID;
    [SerializeField] private LineRenderer bearingLineRenderer;
    [SerializeField] private LineRenderer accelerationLineRenderer;

    private SpriteRenderer[] spriteRenderers;

    private void Awake()
    {
        spriteRenderers = GetComponentsInChildren<SpriteRenderer>();
    }

    public void SetID(string id) 
    {
        textID.text = id;
    }

    public void SetColor(Color color) 
    {
        for(int i = 0; i < spriteRenderers.Length; i++) 
        {
            spriteRenderers[i].color = color.gamma;
        }

        if(accelerationLineRenderer == null) return;

        accelerationLineRenderer.material.SetColor("_BaseColor", color);
        accelerationLineRenderer.material.SetColor("_EmisionColor", color);
    }

    public void SetMovementHint(Vector3 velocity, float maxSpeed) 
    {
        if (bearingLineRenderer == null || accelerationLineRenderer == null) 
        {
            Debug.LogError("No LineRenderer Refs!");

            return;
        }

        Vector3 targetPoint = transform.position + velocity.normalized;
        targetPoint.y = transform.position.y;

        bearingLineRenderer.positionCount = 2;
        bearingLineRenderer.SetPosition(0, transform.position);
        bearingLineRenderer.SetPosition(1, targetPoint);

        float currentAccel = velocity.magnitude + (maxSpeed * 0.1f); //Add small floor, min 10% acceleration displayed
        float accelAlpha = (currentAccel - 0) / (maxSpeed - 0);

        //Handle if there is no velocity
        if(maxSpeed <= 0) 
        {
            accelAlpha = 0.1f;
        }

        Vector3 accelPoint = transform.position + (velocity.normalized * accelAlpha);
        accelPoint.y = transform.position.y + 0.001f;

        accelerationLineRenderer.positionCount = 2;
        accelerationLineRenderer.SetPosition(0, transform.position);
        accelerationLineRenderer.SetPosition(1, accelPoint);

    }
}
