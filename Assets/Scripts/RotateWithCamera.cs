using UnityEngine;

public class RotateWithCamera : MonoBehaviour
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
        transform.rotation = Quaternion.AngleAxis(cameraTransform.eulerAngles.y, Vector3.up);
    }
}
