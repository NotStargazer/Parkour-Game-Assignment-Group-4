using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{

    private CharacterController _controller;
    private PlayerControls _controls;

    [SerializeField] private float _jumpForce = 8f;
    [SerializeField] private float _maxSpeed = 8f;
    [SerializeField] private float _gravity = -9.81f;
    [SerializeField] private Transform _cameraTransform;
    [SerializeField] private float _mouseSensitivity = 2f;
    [SerializeField] private float _deceleration;
    [SerializeField] private float _airControlPercentage = 2f;
    [SerializeField] private float _turnTime = 0.2f;
    [SerializeField] private float _coyoteTime = 0.2f;
    [SerializeField] private float _breakingForce;
    [SerializeField] private AnimationCurve _accelerationCurve;
    private float _coyoteTimer;
    private float _xRotation;
    private Vector3 _lastMoveDirection;
    private float _verticalVelocity;
    private Vector2 _horizontalVelocity;

    private Vector2 _smoothVelocity;
    
    private void Awake()
    {
        _controller = GetComponent<CharacterController>();
        _controls = new PlayerControls();
    }

    private void OnEnable()
    {
        _controls.Player.Enable();
        _controls.Player.Jump.performed += Jump;
    }

    private void OnDisable()
    {
        _controls.Player.Jump.performed -= Jump;
        _controls.Player.Disable();
    }

    private void Update()
    {
        Vector2 moveInput = _controls.Player.Move.ReadValue<Vector2>();
        Vector3 moveDirection3 = transform.right * moveInput.x + transform.forward * moveInput.y;
        Vector2 moveDirection = new Vector2(moveDirection3.x, moveDirection3.z);
        moveDirection = moveDirection.normalized;

        if (moveInput != Vector2.zero)
        {
            float acceleration;
            Vector2 currentDirection = _horizontalVelocity.normalized;
            var dotAngle = Vector2.Dot(moveDirection, currentDirection);
            if (dotAngle < 0f)
            {
                acceleration = _breakingForce;
                if (_controller.isGrounded)
                {
                    acceleration *= _airControlPercentage;
                }
            }
            else
            {
                float speed = _horizontalVelocity.magnitude;
                float currentAccelerationPercentage = speed / _maxSpeed;
                acceleration = _accelerationCurve.Evaluate(currentAccelerationPercentage);
                Vector2 newDirection = Vector2.SmoothDamp(currentDirection, moveDirection, ref _smoothVelocity, _turnTime);
                _horizontalVelocity = newDirection.normalized * speed;
            }
            
            float speedIncrease = acceleration * Time.deltaTime;
            _horizontalVelocity += moveDirection * speedIncrease;
            _horizontalVelocity = Vector2.ClampMagnitude(_horizontalVelocity, _maxSpeed);

            if (_controller.isGrounded)
            {
                _lastMoveDirection = moveDirection;
            }
            else 
            {
                _lastMoveDirection = Vector3.Lerp(_lastMoveDirection, moveDirection, _airControlPercentage * Time.deltaTime);
            }
        }
        else if (_horizontalVelocity != Vector2.zero)
        {
            Vector2 inverseDirection = -_horizontalVelocity.normalized;
            float speedDecrease = _deceleration * Time.deltaTime;
            _horizontalVelocity += inverseDirection * speedDecrease;
            var dot = Vector2.Dot(in inverseDirection, in _horizontalVelocity);
            if (dot > 0)
            {
                _horizontalVelocity = Vector2.zero;
            }
        }

        if (_controller.isGrounded && _verticalVelocity < 0f)
        {
            _verticalVelocity = -2f;
        }

        if (_controller.isGrounded)
        {
            _coyoteTimer = _coyoteTime;
        }
        else
        {
            _coyoteTimer -= Time.deltaTime;
        }
        
        _verticalVelocity += _gravity * Time.deltaTime;
        Vector3 finalMove = new Vector3(_horizontalVelocity.x, 0, _horizontalVelocity.y) + Vector3.up * _verticalVelocity;
        _controller.Move(finalMove * Time.deltaTime);

        if (controller.isGrounded && verticalVelocity < 0f)
        {
            verticalVelocity = -2f;
        }

        verticalVelocity += gravity * Time.deltaTime;
        Vector3 finalMove = moveDirection * currentSpeed + Vector3.up * verticalVelocity;
        controller.Move(finalMove * Time.deltaTime);

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
    public void SetVerticalVelocity(float newVelocity)
    {
        verticalVelocity = newVelocity;
    }

    public void SetCurrentSpeed(float speed)
    {
        currentSpeed = speed;
    }
}