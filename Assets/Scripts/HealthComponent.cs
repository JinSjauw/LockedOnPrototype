using UnityEngine;

public class HealthComponent : MonoBehaviour
{
    [SerializeField] private float maxHealth;
    [SerializeField] private float health;
    [SerializeField] private float percentageHealth;

    private void Awake()
    {
        health = maxHealth;
    }

    public void TakeDamage(float damage) 
    {
        health -= damage;
        //Caculate percentage HP

        if(health <= 0) 
        {
            Die();
        }
    }

    private void Die() 
    {
        Debug.Log(gameObject.name + " Has been Destroyed");
    }
}
