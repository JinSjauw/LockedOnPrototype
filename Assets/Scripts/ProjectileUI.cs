using UnityEngine;

public class ProjectileUI : MonoBehaviour
{
    [SerializeField] private GameObject projectileMarkObject;
    
    public void EnableMark(bool state) 
    {
        projectileMarkObject.SetActive(state);
    }
}
