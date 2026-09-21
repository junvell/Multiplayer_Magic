using Mirror;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : NetworkBehaviour
{
    public float speed = 5f;

    void Update()
    {
        // Check if this script belongs to the local player controlling it
        if (!isLocalPlayer) return;

        float moveX = 0f;
        float moveZ = 0f;

        // Using the New Input System to check for WASD keys
        if (Keyboard.current != null)
        {
            if (Keyboard.current.wKey.isPressed) moveZ = 1f;
            if (Keyboard.current.sKey.isPressed) moveZ = -1f;
            if (Keyboard.current.aKey.isPressed) moveX = -1f;
            if (Keyboard.current.dKey.isPressed) moveX = 1f;
        }

        // Apply the movement to the object's transform
        Vector3 move = new Vector3(moveX, 0, moveZ).normalized;
        transform.position += move * speed * Time.deltaTime;
    }
}

