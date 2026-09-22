using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    private CharacterController controller;// reference to CharacterController component
    private PlayerControls controls;// reference to your generated PlayerControls input class

    
    [SerializeField] private float moveSpeed = 5f;//   - movement speed
    [SerializeField] private float jumpHeight = 2f;//   - jump height
    [SerializeField] private float gravity = -9.81f;//   - gravity value
    [SerializeField] Transform cameraTransform;//   - a Transform reference for the camera (assign in Inspector)
    [SerializeField] private float mouseSensitivity = 2f;//   - mouse sensitivity
    private float xRotation;//   - a variable to track current vertical velocity (for gravity/jump)
    private float verticalVelocity;//   - a variable to track camera pitch (up/down look), separate from player yaw

    private void Awake()
    {
        controller = GetComponent<CharacterController>();// hint: GetComponent<CharacterController>() here
        controls = new PlayerControls();// hint: instantiate your PlayerControls instance here
    }

    private void OnEnable()
    {
        controls.Player.Enable();// hint: enable the input actions
        controls.Player.Jump.performed += Jump;// hint: subscribe PlayerControls.Jump's "performed" event to a Jump() method
    }

    private void OnDisable()
    {
        controls.Player.Jump.performed -= Jump;
        controls.Player.Disable();// hint: unsubscribe from Jump, then disable the input actions
    }

    private void Update()
    {
        Vector2 moveInput = controls.Player.Move.ReadValue<Vector2>();// hint: read Move action's value (Vector2)
        Vector3 moveDirection = transform.right * moveInput.x + transform.forward * moveInput.y;// hint: convert it into a world-space direction using transform.forward/right
        if (controller.isGrounded && verticalVelocity < 0f) 
        {
            verticalVelocity = -2f;
        }

        verticalVelocity += gravity * Time.deltaTime;// hint: apply gravity to your vertical velocity every frame
        Vector3 finalMove = moveDirection * moveSpeed + Vector3.up * verticalVelocity;
        controller.Move(finalMove * Time.deltaTime);// hint: call controller.Move() combining horizontal movement + vertical velocity * Time.deltaTime

        Vector2 lookInput = controls.Player.Look.ReadValue<Vector2>(); // hint: read Look action's value (Vector2)

        float mouseX = lookInput.x * mouseSensitivity;
        float mouseY = lookInput.y * mouseSensitivity;// hint: rotate the Player transform left/right (yaw)
        transform.Rotate(Vector3.up * mouseX);

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);
        cameraTransform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);// hint: rotate the camera transform up/down (pitch) — clamp it so it can't flip past straight up/down
    }

    private void Jump(InputAction.CallbackContext context)
    {
        if (controller.isGrounded)
        {
            verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }// hint: only allow jumping if controller.isGrounded
        // hint: set vertical velocity using: sqrt(jumpHeight * -2 * gravity)
    }
    public void SetVerticalVelocity(float newVelocity)
    {
        verticalVelocity = newVelocity;
    }
}