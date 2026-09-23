using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{

    private CharacterController controller;
    private PlayerControls controls;

    [SerializeField] private float jumpForce = 8f;
    [SerializeField] private float minSpeed = 0f;
    [SerializeField] private float acceleration = 1f;
    [SerializeField] private float maxSpeed = 8f;
    [SerializeField] private float gravity = -9.81f;
    [SerializeField] Transform cameraTransform;
    [SerializeField] private float mouseSensitivity = 2f;


    public float currentSpeed;
    private float xRotation;
    private float verticalVelocity;

    private float logTimer;
    public bool isSliding;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        controls = new PlayerControls();
    }

    private void OnEnable()
    {
        controls.Player.Enable();
        controls.Player.Jump.performed += Jump;
    }

    private void OnDisable()
    {
        controls.Player.Jump.performed -= Jump;
        controls.Player.Disable();
    }

    private void Update()
    {
        Vector2 moveInput = controls.Player.Move.ReadValue<Vector2>();
        Vector3 moveDirection = transform.right * moveInput.x + transform.forward * moveInput.y;
        moveDirection = moveDirection.normalized;

        if (!isSliding)
        {
            if (moveInput != Vector2.zero)
            {
                currentSpeed += acceleration * Time.deltaTime;
                currentSpeed = Mathf.Clamp(currentSpeed, minSpeed, maxSpeed);
            }
            else
            {
                currentSpeed = 0f;
            }

            verticalVelocity += gravity * Time.deltaTime;
            Vector3 finalMove = moveDirection * currentSpeed + Vector3.up * verticalVelocity;
            controller.Move(finalMove * Time.deltaTime);

            if ((controller.collisionFlags & CollisionFlags.Sides) != 0)
            {
                currentSpeed = minSpeed;
            }
        }

        // Speed Check
        logTimer += Time.deltaTime;
        if (logTimer >= 0.75f)
        {
            Debug.Log(currentSpeed);
            logTimer = 0f;
        }

        Vector2 lookInput = controls.Player.Look.ReadValue<Vector2>();

        float mouseX = lookInput.x * mouseSensitivity;
        float mouseY = lookInput.y * mouseSensitivity;
        transform.Rotate(Vector3.up * mouseX);

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);
        cameraTransform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
    }

    private void Jump(InputAction.CallbackContext context)
    {
        if (controller.isGrounded)
        {
            verticalVelocity = jumpForce;
        }
    }

}