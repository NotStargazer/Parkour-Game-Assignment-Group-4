using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerSlideMovement : MonoBehaviour
{
    [Header("References")]
    private CharacterController controller;
    private PlayerMovement playerMovement;
    [SerializeField] private Transform cameraTransform;

    [Header("Slide Settings")]
    [SerializeField] private float slideFriction = 2.5f;
    [SerializeField] private float minSpeedToSlide = 3f;
    [SerializeField] private float stopSpeed = 0.5f;          // Slide ends when speed drops below this
    [SerializeField] private float crouchHeight = 1f;
    [Tooltip("Extra camera drop while sliding, on top of the capsule shrinking.")]
    [SerializeField] private float extraCameraDrop = 0f;
    [SerializeField] private float slopeAcceleration = 15f;   // Extra speed gained sliding downhill
    [SerializeField] private float uphillResistance = 20f;    // Speed lost sliding uphill
    [SerializeField] private float maxSlideSpeed = 20f;
    [Tooltip("Degrees per second the slide turns toward where you look. 0 = locked direction.")]
    [SerializeField] private float steerSpeed = 180f;

    [Header("Physics")]
    [SerializeField] private float gravity = -9.81f;
    [Tooltip("How far below the capsule counts as still being on the ground.")]
    [SerializeField] private float groundSnapDistance = 0.3f;
    [Tooltip("Layers that count as ground/ceiling. Exclude the Player layer.")]
    [SerializeField] private LayerMask groundMask = ~0;

    private float originalHeight;
    private Vector3 originalCenter;
    private Vector3 originalCameraPos;
    private Vector3 slideDirection;
    private float verticalVelocity;
    private bool isSliding;
    private bool wantsToStop;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        playerMovement = GetComponent<PlayerMovement>();

        if (controller != null)
        {
            originalHeight = controller.height;
            originalCenter = controller.center;
        }

        if (cameraTransform != null)
        {
            originalCameraPos = cameraTransform.localPosition;
        }
    }

    private void OnDisable()
    {
        // Don't leave the player stuck crouched if this script gets disabled mid-slide
        if (isSliding)
        {
            StopSlide();
        }
    }

    private void Update()
    {
        Keyboard keyboard = Keyboard.current;

        if (keyboard != null)
        {
            if (keyboard.cKey.wasPressedThisFrame)
            {
                StartSlide();
            }

            if (keyboard.cKey.wasReleasedThisFrame && isSliding)
            {
                wantsToStop = true;
            }
        }

        if (!isSliding)
        {
            return;
        }

        PerformSlide();

        // End the slide when released or out of speed, but only if there's room to stand up
        bool outOfSpeed = playerMovement.currentSpeed <= stopSpeed && controller.isGrounded;
        if ((wantsToStop || outOfSpeed) && CanStandUp())
        {
            StopSlide();
        }
    }

    private void StartSlide()
    {
        if (isSliding)
        {
            return;
        }

        // Only slide on the ground and when moving fast enough
        if (!controller.isGrounded || playerMovement.currentSpeed < minSpeedToSlide)
        {
            return;
        }

        isSliding = true;
        wantsToStop = false;
        playerMovement.isSliding = true;
        verticalVelocity = -2f;

        // Lock the slide to the direction the player is facing
        slideDirection = transform.forward;

        // Shrink the capsule, and shift the center down so the feet stay on the ground
        controller.height = crouchHeight;
        controller.center = originalCenter - Vector3.up * ((originalHeight - crouchHeight) * 0.5f);

        if (cameraTransform != null)
        {
            float drop = (originalHeight - crouchHeight) + extraCameraDrop;
            cameraTransform.localPosition = originalCameraPos - Vector3.up * drop;
        }
    }

    private void PerformSlide()
    {
        if (steerSpeed > 0f)
        {
            // Turn the slide toward where the player is looking, at a limited rate
            slideDirection = Vector3.RotateTowards(
                slideDirection,
                transform.forward,
                steerSpeed * Mathf.Deg2Rad * Time.deltaTime,
                0f);
        }

        // Cast from the capsule center so the ray never starts inside the floor,
        // and treat "close to the ground" as grounded so friction can't drop out
        Bounds bounds = controller.bounds;
        bool rayHit = Physics.Raycast(bounds.center, Vector3.down, out RaycastHit hit,
            bounds.extents.y + groundSnapDistance, groundMask, QueryTriggerInteraction.Ignore);
        bool grounded = controller.isGrounded || rayHit;

        // Accumulate gravity so falling off ledges behaves like normal movement
        if (grounded && verticalVelocity < 0f)
        {
            verticalVelocity = -2f;
        }
        verticalVelocity += gravity * Time.deltaTime;

        Vector3 moveDirection = slideDirection;

        if (grounded)
        {
            if (rayHit)
            {
                // Direction parallel to the slope surface
                Vector3 slopeDirection = Vector3.ProjectOnPlane(slideDirection, hit.normal).normalized;
                float incline = slopeDirection.y;

                if (incline < -0.05f)
                {
                    // Downhill: speed up based on steepness
                    playerMovement.currentSpeed += Mathf.Abs(incline) * slopeAcceleration * Time.deltaTime;
                    playerMovement.currentSpeed = Mathf.Min(playerMovement.currentSpeed, maxSlideSpeed);
                }
                else if (incline > 0.05f)
                {
                    // Uphill: lose speed quickly, never below zero
                    playerMovement.currentSpeed -= incline * uphillResistance * Time.deltaTime;
                    playerMovement.currentSpeed = Mathf.Max(playerMovement.currentSpeed, 0f);
                }
                else
                {
                    // Flat: normal friction
                    playerMovement.currentSpeed = Mathf.MoveTowards(playerMovement.currentSpeed, 0f, slideFriction * Time.deltaTime);
                }

                moveDirection = slopeDirection;
            }
            else
            {
                // Fallback if the raycast misses
                playerMovement.currentSpeed = Mathf.MoveTowards(playerMovement.currentSpeed, 0f, slideFriction * Time.deltaTime);
            }
        }
        // In the air: keep horizontal speed, no friction

        Vector3 slideMove = moveDirection * playerMovement.currentSpeed + Vector3.up * verticalVelocity;
        controller.Move(slideMove * Time.deltaTime);

        // Hit a wall: kill the speed (the slide then ends via the stop check)
        if ((controller.collisionFlags & CollisionFlags.Sides) != 0)
        {
            playerMovement.currentSpeed = 0f;
        }
    }

    private void StopSlide()
    {
        isSliding = false;
        wantsToStop = false;
        playerMovement.isSliding = false;

        controller.height = originalHeight;
        controller.center = originalCenter;

        if (cameraTransform != null)
        {
            cameraTransform.localPosition = originalCameraPos;
        }
    }

    // Returns true if there's enough headroom to return to full height
    private bool CanStandUp()
    {
        Bounds bounds = controller.bounds;
        float standingTop = bounds.min.y + originalHeight;
        float distance = standingTop - bounds.center.y;

        return !Physics.Raycast(bounds.center, Vector3.up, distance, groundMask, QueryTriggerInteraction.Ignore);
    }
}