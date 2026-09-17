using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerSlideMovement : MonoBehaviour
{
    [Header("Refrences")]
    private CharacterController controller;
    private PlayerMovement playerMovement;
    [SerializeField] private Transform cameraTransform;

    [Header("Slide Settings")]
    [SerializeField] private float slideFriction = 2.5f;
    [SerializeField] private float minSpeedToSlide = 3f;
    [SerializeField] private float crouchHeight = 1f;
    [SerializeField] private float slopeAcceleration = 15f; // Extra speed gained sliding slope
    [SerializeField] private float uphillResistance = 20f; // uphillresistance
    [SerializeField] private float maxSlideSpeed = 20f;


    private float originalHeight;
    private Vector3 originalCameraPos;
    private Vector3 slideDirection;
    private bool isSliding;


    private void Awake()
    {
        // These two lines grabs the component from the same gameobject, which is the player.
        controller = GetComponent<CharacterController>();
        playerMovement = GetComponent<PlayerMovement>();

        // This if statement saves the players starting height
        if (controller != null)
        {
            originalHeight = controller.height;
        }
        // Saves the camera's starting local position
        if (cameraTransform != null)
        {
            originalCameraPos = cameraTransform.localPosition;
        }
    }


    // Update is called once per frame
    void Update()
    {
        // Checks if the player is holding 'C' to start sliding
        if (Keyboard.current.cKey.wasPressedThisFrame)
        {
            StartSlide();
        }

        // Stops sliding ONLY when 'C' is released
        if (Keyboard.current.cKey.wasReleasedThisFrame && isSliding)
        {
            StopSlide();
        }

        if (isSliding)
        {
            PerformSlide();
        }

    }

    private void StartSlide()
    {
        // this only allows sliding if on the ground and moving fast enough
        if (!controller.isGrounded || playerMovement.currentSpeed < minSpeedToSlide)
        {
            return;
        }

        isSliding = true;
        playerMovement.isSliding = true;

        // This locks the slide towards the players forward direction
        slideDirection = transform.forward;

        // Drops the players height for the crouch "effect"
        controller.height = crouchHeight;

        // lowers the camera position to match the crouch height
        if (cameraTransform != null)
        {
            cameraTransform.localPosition = new Vector3(originalCameraPos.x, originalCameraPos.y / 2f, originalCameraPos.z);
        }
    }

    private void PerformSlide()
    {
        slideDirection = transform.forward;

        // Gravity offset to keep character glued to slopes/ground while sliding
        Vector3 gravityVector = Vector3.down * 9.81f;

        if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, 2.0f))
        {
            // Calculate downhill vector parallel to the slope

            Vector3 slopeDirection = Vector3.ProjectOnPlane(slideDirection, hit.normal).normalized;
            float incline = slopeDirection.y;

            if (incline < -0.05f)
            {
                // Downhill accelerates the players speed using the slope stepness in tandem with the raycast.
                playerMovement.currentSpeed += Mathf.Abs(incline) * slopeAcceleration * Time.deltaTime;
                playerMovement.currentSpeed = Mathf.Min(playerMovement.currentSpeed, maxSlideSpeed);
            }

            else if (incline > 0.05f)
            {
                // Slide uphill speed rapidly goes down
                playerMovement.currentSpeed -= incline * uphillResistance * Time.deltaTime;
            }

            else
            {
                // When on flat ground you have standard
                playerMovement.currentSpeed = Mathf.MoveTowards(playerMovement.currentSpeed, 0f, slideFriction * Time.deltaTime);
            }

            // applies movement allgined to the slope angle
            Vector3 slideMove = (slopeDirection * playerMovement.currentSpeed) + gravityVector;
            controller.Move(slideMove * Time.deltaTime);
        }
        else
        {
            // Fallback fo the ground if the raycast somehow misses
            playerMovement.currentSpeed = Mathf.MoveTowards(playerMovement.currentSpeed, 0f, slideFriction * Time.deltaTime);
            Vector3 slideMove = (slideDirection * playerMovement.currentSpeed) + gravityVector;
            controller.Move(slideMove * Time.deltaTime);
        }
    }

    private void StopSlide()
    {
        isSliding = false;
        playerMovement.isSliding = false;

        // Reset the character height back to normal
        controller.height = originalHeight;

        // Reset the camera back to original position
        if (cameraTransform != null)
        {
            cameraTransform.localPosition = originalCameraPos;
        }
    }

}