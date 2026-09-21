using Mirror;
using UnityEngine;

public class NetworkedProjectile : NetworkBehaviour
{
    [Header("Trajectory Settings")]
    public float speed = 15f;
    public float lifeTime = 5f;
    [HideInInspector] public Vector3 direction;
    [HideInInspector] public GameObject owner;

    [Header("AoE / Collision Settings")]
    public float aoeRadius = 3.0f;
    public int spellDamage = 25;
    
    // Server-only lifecycle
    [ServerCallback]
    void Start()
    {
        // Auto-destroy on the server after lifeTime expires
        Invoke(nameof(DestroySelf), lifeTime);
    }

    // Both server and client run this to move the projectile (Client-Side Prediction/Interpolation via NetworkTransform can also be used)
    void Update()
    {
        transform.position += direction * speed * Time.deltaTime;
    }

    [ServerCallback]
    void OnTriggerEnter(Collider other)
    {
        // Ignore collision with the owner
        if (other.gameObject == owner) return;

        // Perform Authoritative AoE Splash Damage
        ResolveAoE(transform.position);

        // Tell all clients to play the explosion effect
        RpcOnExplode(transform.position, aoeRadius);

        // Destroy on the network
        DestroySelf();
    }

    [Server]
    void ResolveAoE(Vector3 impactPoint)
    {
        Collider[] hitColliders = Physics.OverlapSphere(impactPoint, aoeRadius);
        foreach (var hit in hitColliders)
        {
            NetworkHealth targetHealth = hit.GetComponent<NetworkHealth>();
            if (targetHealth != null)
            {
                // Optional: Check line of sight from impactPoint to hit.transform.position
                
                // Optional: Apply distance falloff
                float distance = Vector3.Distance(impactPoint, hit.transform.position);
                float damageMultiplier = 1f - (distance / aoeRadius);
                int calculatedDamage = Mathf.Max(1, Mathf.RoundToInt(spellDamage * damageMultiplier));

                // Apply authoritative damage
                targetHealth.TakeDamage(calculatedDamage);
            }
        }
    }

    [ClientRpc]
    void RpcOnExplode(Vector3 position, float radius)
    {
        // This runs on all clients.
        // Instantiate a visual explosion particle effect or play an audio sound here.
        Debug.Log($"Explosion at {position} with radius {radius}");
        
        // Example:
        // Instantiate(explosionEffectPrefab, position, Quaternion.identity);
    }

    [Server]
    void DestroySelf()
    {
        NetworkServer.Destroy(gameObject);
    }
}

