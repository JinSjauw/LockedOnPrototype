using UnityEngine;

public class BillboardUI : MonoBehaviour
{
    [SerializeField] private Transform cameraTransform;

    // Update is called once per frame
    void Update()
    {
        transform.forward = cameraTransform.forward;
    }
}
