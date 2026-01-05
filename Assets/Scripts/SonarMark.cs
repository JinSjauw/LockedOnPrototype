using TMPro;
using UnityEngine;

public class SonarMark : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI textID;
    private LineRenderer lineRenderer;

    private void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
    }

    public void SetID(string id) 
    {
        textID.text = id;
    }

    public void SetDirectionHint(Vector3 direction) 
    {
        Vector3 targetPoint = transform.position + direction.normalized;
        targetPoint.y = transform.position.y;

        lineRenderer.positionCount = 2;
        lineRenderer.SetPosition(0, transform.position);
        lineRenderer.SetPosition(1, targetPoint);
    }
}
