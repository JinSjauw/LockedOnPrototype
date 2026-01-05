using FOVMapping;
using UnityEngine;

public class EnemyUIController : MonoBehaviour
{
    [SerializeField] private FOVAgent fovAgent;
    [SerializeField] private GameObject markTransform;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        //TEMP CODE
        if (!markTransform.activeInHierarchy) 
        {
            if (!fovAgent.IsUnderFOW()) 
            {
                markTransform.SetActive(true);
            }
        }
        else if(markTransform.activeInHierarchy) 
        {
            if (fovAgent.IsUnderFOW()) 
            {
                markTransform.SetActive(false);
            }
        }
    }
}
