using System.Collections;
using Mirror;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : NetworkBehaviour
{
    [Header("Character Selection")]
    [SyncVar(hook = nameof(OnCharacterIndexChanged))]
    public int characterIndex = 0; // 0 = Dog, 1 = Cat

    public GameObject dogModel;
    public GameObject catModel;

    private GameObject activeModel;

    [Header("Movement Settings")]
    public float speed = 5f;
    public float rotationSpeed = 12f;

    [Tooltip("If the model faces backwards or sideways when walking, adjust this (e.g., 0, 90, 180, -90).")]
    public float modelFacingOffset = 0f;

    [Header("Procedural Animation (Bounce & Waddle)")]
    public float bobFrequency = 12f;
    public float bobHeight = 0.15f;
    public float tiltAngle = 8f;

    private Vector3 initialModelLocalPos;
    private Quaternion initialModelLocalRot;
    private float bobTimer = 0f;
    private bool isCasting = false;

    void Start()
    {
        UpdateCharacterModel(characterIndex);
    }

    void OnCharacterIndexChanged(int oldIndex, int newIndex)
    {
        UpdateCharacterModel(newIndex);
    }

    void UpdateCharacterModel(int index)
    {
        if (dogModel != null) dogModel.SetActive(index == 0);
        if (catModel != null) catModel.SetActive(index == 1);

        activeModel = (index == 0) ? dogModel : catModel;
        if (activeModel != null)
        {
            initialModelLocalPos = activeModel.transform.localPosition;
            initialModelLocalRot = activeModel.transform.localRotation;
        }
    }

    void Update()
    {
        if (!isLocalPlayer) return;

        float moveX = 0f;
        float moveZ = 0f;

        if (Keyboard.current != null)
        {
            if (Keyboard.current.wKey.isPressed) moveZ = 1f;
            if (Keyboard.current.sKey.isPressed) moveZ = -1f;
            if (Keyboard.current.aKey.isPressed) moveX = -1f;
            if (Keyboard.current.dKey.isPressed) moveX = 1f;
        }

        Vector3 moveDirection = new Vector3(moveX, 0, moveZ).normalized;

        // Move Player
        transform.position += moveDirection * speed * Time.deltaTime;

        // Rotate smoothly towards movement direction + facing offset
        if (moveDirection != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(moveDirection) * Quaternion.Euler(0f, modelFacingOffset, 0f);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }

        // Procedural Walking Bounce & Tilt
        AnimateProcedural(moveDirection.magnitude > 0.1f);
    }

    void AnimateProcedural(bool isMoving)
    {
        if (activeModel == null || isCasting) return;

        if (isMoving)
        {
            bobTimer += Time.deltaTime * bobFrequency;
            
            // Vertical bounce (hop)
            float newY = initialModelLocalPos.y + Mathf.Abs(Mathf.Sin(bobTimer)) * bobHeight;
            activeModel.transform.localPosition = new Vector3(initialModelLocalPos.x, newY, initialModelLocalPos.z);

            // Side-to-side waddle tilt
            float tilt = Mathf.Sin(bobTimer * 0.5f) * tiltAngle;
            activeModel.transform.localRotation = initialModelLocalRot * Quaternion.Euler(0f, 0f, tilt);
        }
        else
        {
            // Smoothly return to resting stance
            bobTimer = 0f;
            activeModel.transform.localPosition = Vector3.Lerp(activeModel.transform.localPosition, initialModelLocalPos, Time.deltaTime * 10f);
            activeModel.transform.localRotation = Quaternion.Lerp(activeModel.transform.localRotation, initialModelLocalRot, Time.deltaTime * 10f);
        }
    }

    // Called by SpellCaster when player left clicks
    public void PlayCastAnimation()
    {
        if (activeModel != null && gameObject.activeInHierarchy)
        {
            StartCoroutine(CastRecoilCoroutine());
        }
    }

    private IEnumerator CastRecoilCoroutine()
    {
        isCasting = true;
        Vector3 punchPos = initialModelLocalPos + Vector3.forward * 0.35f + Vector3.up * 0.1f;
        
        // Lunge forward
        float t = 0f;
        while (t < 0.12f)
        {
            t += Time.deltaTime;
            activeModel.transform.localPosition = Vector3.Lerp(initialModelLocalPos, punchPos, t / 0.12f);
            yield return null;
        }

        // Return back
        t = 0f;
        while (t < 0.18f)
        {
            t += Time.deltaTime;
            activeModel.transform.localPosition = Vector3.Lerp(punchPos, initialModelLocalPos, t / 0.18f);
            yield return null;
        }

        activeModel.transform.localPosition = initialModelLocalPos;
        isCasting = false;
    }
}
