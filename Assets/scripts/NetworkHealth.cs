using Mirror;
using UnityEngine;

public class NetworkHealth : NetworkBehaviour
{
    [Header("Health Settings")]
    public int maxHealth = 100;

    [SyncVar(hook = nameof(OnHealthChanged))]
    public int currentHealth;

    // Optional visual feedback
    private Renderer[] renderers;

    void Start()
    {
        currentHealth = maxHealth;
        renderers = GetComponentsInChildren<Renderer>();
    }

    [Server]
    public void TakeDamage(int amount)
    {
        // Only the server can change the health!
        if (currentHealth <= 0) return;

        currentHealth -= amount;
        
        if (currentHealth <= 0)
        {
            currentHealth = 0;
            Die();
        }
    }

    [Server]
    void Die()
    {
        Debug.Log($"{gameObject.name} has died!");
        // Example: NetworkServer.Destroy(gameObject);
        // Or handle respawn logic
    }

    // This is called automatically on all clients when the server changes 'currentHealth'
    void OnHealthChanged(int oldHealth, int newHealth)
    {
        Debug.Log($"{gameObject.name} took damage! Health: {newHealth}/{maxHealth}");

        // Example Visual Feedback: Flash Red on hit
        if (newHealth < oldHealth)
        {
            if (renderers != null)
            {
                foreach (var r in renderers)
                {
                    // Basic color flash logic - in a real game, you might use a coroutine or shader flash
                    r.material.color = Color.red; 
                }
            }
        }
    }
}

