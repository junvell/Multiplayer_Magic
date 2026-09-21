using Mirror;
using UnityEngine;
using UnityEngine.InputSystem;

public class SpellCaster : NetworkBehaviour
{
    [Header("Spell Settings")]
    public GameObject projectilePrefab;
    public Transform spellOrigin; // The point where the spell spawns (e.g., hand or staff)
    
    public float spellCooldown = 1.0f;
    private float lastCastTime = -9999f;
    
    // Server-side tolerance to prevent players from teleporting their cast origin
    public float maxOriginTolerance = 2.0f; 

    private PlayerMovement playerMovement;

    void Awake()
    {
        playerMovement = GetComponent<PlayerMovement>();
    }

    void Update()
    {
        if (!isLocalPlayer) return;

        // Check for Left Mouse Button click using New Input System
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            if (Time.time >= lastCastTime + spellCooldown)
            {
                CastSpell();
                lastCastTime = Time.time;
            }
        }
    }

    void CastSpell()
    {
        // Calculate aiming direction (for simplicity, we cast forward)
        // In a more complex game, you could raycast to the mouse position here
        Vector3 direction = transform.forward;
        Vector3 origin = (spellOrigin != null) ? spellOrigin.position : transform.position + Vector3.up;

        // Trigger local casting animation immediately for responsiveness
        if (playerMovement != null)
        {
            playerMovement.PlayCastAnimation();
        }

        // Send Command to Server
        CmdCastSpell(origin, direction, NetworkTime.time);
    }

    [Command]
    void CmdCastSpell(Vector3 origin, Vector3 direction, double clientTimestamp)
    {
        // 1. Authoritative Server Check: Cooldown Enforcement
        if (Time.time < lastCastTime + spellCooldown)
        {
            Debug.LogWarning($"[Anti-Cheat] {gameObject.name} attempted to cast spell too quickly!");
            return; 
        }

        // 2. Authoritative Server Check: Origin Proximity Verification
        // Ensure the client didn't spoof a coordinate across the map
        float distToOrigin = Vector3.Distance(transform.position, origin);
        if (distToOrigin > maxOriginTolerance)
        {
            Debug.LogWarning($"[Anti-Cheat] {gameObject.name} spoofed spell origin!");
            // Snap back to a valid server position instead of rejecting entirely
            origin = transform.position + Vector3.up;
        }

        lastCastTime = Time.time;

        // 3. Server-side instantiation
        if (projectilePrefab != null)
        {
            GameObject proj = Instantiate(projectilePrefab, origin, Quaternion.LookRotation(direction));
            
            // Setup the projectile's trajectory on the server before spawning it to clients
            NetworkedProjectile np = proj.GetComponent<NetworkedProjectile>();
            if (np != null)
            {
                np.direction = direction;
                np.owner = this.gameObject;
            }

            // Spawn over the network
            NetworkServer.Spawn(proj);
        }
    }
}

