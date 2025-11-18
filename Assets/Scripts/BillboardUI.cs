using UnityEngine;

public class BillboardUI : MonoBehaviour
{
    [SerializeField] private Transform cameraTransform;

    private void Awake()
    {
        if (!cameraTransform) 
        {
            cameraTransform = Camera.main.transform;
        }
    }

    // Update is called once per frame
    void Update()
    {
        transform.forward = cameraTransform.forward;
    }
}
