using UnityEngine;

/// <summary>
/// Speedbased FOV and strafe tilt for the first-person camera
/// Put this on the CHILD camera object (the one with the Camera component)
/// not on the player itself. It only reads from the player scripts.
/// </summary>
public class DynamicCamera : MonoBehaviour
{
    [Header("References (auto-found in parents if empty)")]
    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private CharacterController controller;

    [Header("FOV")]
    [SerializeField] private float maxFovBoost = 15f;
    [SerializeField, Min(0.1f)] private float speedForMaxFov = 15f;
    [SerializeField] private float slideFovBonus = 5f;
    [SerializeField] private float fovSmoothTime = 0.15f;

    [Header("Roll (tilt)")]
    [SerializeField] private float strafeRoll = 3f;
    [SerializeField] private float slideRollMultiplier = 2f;
    [SerializeField] private float rollSmoothTime = 0.1f;

    private Camera cam;
    private float baseFov;
    private float fovVelocity;
    private float currentRoll;
    private float rollVelocity;

    private void Awake()
    {
        cam = GetComponent<Camera>();
        if (cam == null)
        {
            Debug.LogError("DynamicCamera must be on the object with the Camera component.", this);
            enabled = false;
            return;
        }

        baseFov = cam.fieldOfView;

        if (playerMovement == null)
        {
            playerMovement = GetComponentInParent<PlayerMovement>();
        }

        if (controller == null)
        {
            controller = GetComponentInParent<CharacterController>();
        }

        if (playerMovement == null || controller == null)
        {
            Debug.LogWarning("DynamicCamera: couldn't find PlayerMovement or CharacterController in a parent.", this);
        }
        else if (playerMovement.gameObject == gameObject)
        {
            Debug.LogWarning("DynamicCamera is on the player object. Move it to the child camera.", this);
        }
    }

    private void OnDisable()
    {
        
        if (cam != null)
        {
            cam.fieldOfView = baseFov;
        }
    }


    private void LateUpdate()
    {
        if (playerMovement == null || controller == null)
        {
            return;
        }

        bool sliding = playerMovement.isSliding;

        Vector3 velocity = controller.velocity;
        Vector3 horizontal = new Vector3(velocity.x, 0f, velocity.z);
        float speed01 = Mathf.Clamp01(horizontal.magnitude / speedForMaxFov);

        float targetFov = baseFov + speed01 * maxFovBoost + (sliding ? slideFovBonus : 0f);
        cam.fieldOfView = Mathf.SmoothDamp(cam.fieldOfView, targetFov, ref fovVelocity, fovSmoothTime);

        float sideAmount = Vector3.Dot(horizontal, playerMovement.transform.right) / speedForMaxFov;
        sideAmount = Mathf.Clamp(sideAmount, -1f, 1f);

        float gain = strafeRoll * (sliding ? slideRollMultiplier : 1f);
        float targetRoll = -sideAmount * gain;
        currentRoll = Mathf.SmoothDamp(currentRoll, targetRoll, ref rollVelocity, rollSmoothTime);

        transform.localRotation = transform.localRotation * Quaternion.Euler(0f, 0f, currentRoll);
    }
}