using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{

    private CharacterController controller;
    private PlayerControls controls;

    [SerializeField] private float _jumpForce = 8f;
    [SerializeField ]private float _minSpeed = 0f;
    [SerializeField] private float _acceleration = 1f;
    [SerializeField] private float _maxSpeed = 8f;
    [SerializeField] private float _gravity = -9.81f;
    [SerializeField] Transform _cameraTransform;
    [SerializeField] private float _mouseSensitivity = 2f;
    [SerializeField] private float _deceleration;
    [SerializeField] private float _airControl = 2f;
    [SerializeField] private float _coyoteTime = 0.2f;
    private float _coyoteTimer;
    private Vector3 _lastMoveDirection;
    private float _currentSpeed;
    private float _xRotation;
    private float _verticalVelocity;

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
        

        if (moveInput != Vector2.zero)
        {

            _currentSpeed += _acceleration * Time.deltaTime;
            _currentSpeed = Mathf.Clamp(_currentSpeed, _minSpeed, _maxSpeed);

            if (controller.isGrounded)
            {
                
                _lastMoveDirection = moveDirection;
            }
            else 
            {
                _lastMoveDirection = Vector3.Lerp(_lastMoveDirection, moveDirection, _airControl * Time.deltaTime);
            }
        }
            else 
            {
            _currentSpeed -= _deceleration * Time.deltaTime;
            _currentSpeed = Mathf.Max(_currentSpeed, 0);
            }


        if (controller.isGrounded && _verticalVelocity < 0f)
        {
            _verticalVelocity = -2f;
        }

        if (controller.isGrounded)
        {
            _coyoteTimer = _coyoteTime;
        }

        else 
        {
            _coyoteTimer -= Time.deltaTime;
        }

        

        _verticalVelocity += _gravity * Time.deltaTime;
        Vector3 finalMove = _lastMoveDirection * _currentSpeed + Vector3.up * _verticalVelocity;
        controller.Move(finalMove * Time.deltaTime);


        if ((controller.collisionFlags & CollisionFlags.Sides) != 0)
        {
            _currentSpeed = _minSpeed;
        }
        

        Vector2 lookInput = controls.Player.Look.ReadValue<Vector2>();

        float mouseX = lookInput.x * _mouseSensitivity;
        float mouseY = lookInput.y * _mouseSensitivity;
        transform.Rotate(Vector3.up * mouseX);

        _xRotation -= mouseY;
        _xRotation = Mathf.Clamp(_xRotation, -90f, 90f);
        _cameraTransform.localRotation = Quaternion.Euler(_xRotation, 0f, 0f);
    }

    private void Jump(InputAction.CallbackContext context)
    {
        if (_coyoteTimer > 0f)
        {
            _verticalVelocity = _jumpForce;
            _coyoteTimer = 0f;
        }


    }

   
}