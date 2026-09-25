using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerSlideMovement : MonoBehaviour
{
    [Header("References")]
    private CharacterController controller;
    private PlayerMovement playerMovement;
    [SerializeField] private Transform cameraTransform;

    [Header("Slide Settings")]
    [SerializeField] private float _slideFriction = 2.5f;
    [SerializeField] private float _minSpeedToSlide = 3f;
    [SerializeField] private float _stopSpeed = 0.5f;          // Slide ends when speed drops below this
    [SerializeField] private float _crouchHeight = 1f;
    [Tooltip("Extra camera drop while sliding, on top of the capsule shrinking.")]
    [SerializeField] private float _extraCameraDrop = 0f;
    [SerializeField] private float _slopeAcceleration = 15f;   // Extra speed gained sliding downhill
    [SerializeField] private float _uphillResistance = 20f;    // Speed lost sliding uphill
    [SerializeField] private float _maxSlideSpeed = 20f;
    [Tooltip("Degrees per second the slide turns toward where you look. 0 = locked direction.")]
    [SerializeField] private float _steerSpeed = 180f;

    [Header("Physics")]
    [SerializeField] private float _gravity = -9.81f;
    [Tooltip("How far below the capsule counts as still being on the ground.")]
    [SerializeField] private float _groundSnapDistance = 0.3f;
    [Tooltip("Layers that count as ground/ceiling. Exclude the Player layer.")]
    [SerializeField] private LayerMask _groundMask = ~0;

    private float _originalHeight;
    private Vector3 _originalCenter;
    private Vector3 _originalCameraPos;
    private Vector3 _slideDirection;
    private float _verticalVelocity;
    private bool _isSliding;
    private bool _wantsToStop;

    private float _currentSlideSpeed;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        playerMovement = GetComponent<PlayerMovement>();

        if (controller != null)
        {
            _originalHeight = controller.height;
            _originalCenter = controller.center;
        }

        if (cameraTransform != null)
        {
            _originalCameraPos = cameraTransform.localPosition;
        }
    }

    private void OnDisable()
    {
        // Don't leave the player stuck crouched if this script gets disabled mid-slide
        if (_isSliding)
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

            if (keyboard.cKey.wasReleasedThisFrame && _isSliding)
            {
                _wantsToStop = true;
            }
        }

        if (!_isSliding)
        {
            return;
        }

        PerformSlide();

        // End the slide when released or out of speed, but only if there's room to stand up
        bool outOfSpeed = playerMovement.HorizontalVelocity.magnitude <= _stopSpeed && controller.isGrounded;
        if ((_wantsToStop || outOfSpeed) && CanStandUp())
        {
            StopSlide();
        }
    }

    private void StartSlide()
    {
        if (_isSliding)
        {
            return;
        }

        // Only slide on the ground and when moving fast enough
        if (!controller.isGrounded || playerMovement.HorizontalVelocity.magnitude < _minSpeedToSlide)
        {
            return;
        }

        _isSliding = true;
        _wantsToStop = false;
        playerMovement.IsSliding = true;
        _verticalVelocity = -2f;

        // Lock the slide to the direction the player is facing
        _slideDirection = transform.forward;

        // Shrink the capsule, and shift the center down so the feet stay on the ground
        controller.height = _crouchHeight;
        controller.center = _originalCenter - Vector3.up * ((_originalHeight - _crouchHeight) * 0.5f);

        if (cameraTransform != null)
        {
            float drop = (_originalHeight - _crouchHeight) + _extraCameraDrop;
            cameraTransform.localPosition = _originalCameraPos - Vector3.up * drop;
        }
    }

    private void PerformSlide()
    {
        if (_steerSpeed > 0f)
        {
            // Turn the slide toward where the player is looking, at a limited rate
            _slideDirection = Vector3.RotateTowards(
                _slideDirection,
                transform.forward,
                _steerSpeed * Mathf.Deg2Rad * Time.deltaTime,
                0f);
        }

        // Cast from the capsule center so the ray never starts inside the floor,
        // and treat "close to the ground" as grounded so friction can't drop out
        Bounds bounds = controller.bounds;
        bool rayHit = Physics.Raycast(bounds.center, Vector3.down, out RaycastHit hit,
            bounds.extents.y + _groundSnapDistance, _groundMask, QueryTriggerInteraction.Ignore);
        bool grounded = controller.isGrounded || rayHit;

        // Accumulate gravity so falling off ledges behaves like normal movement
        if (grounded && _verticalVelocity < 0f)
        {
            _verticalVelocity = -2f;
        }
        _verticalVelocity += _gravity * Time.deltaTime;

        Vector3 moveDirection = _slideDirection;

        if (grounded)
        {
            if (rayHit)
            {
                // Direction parallel to the slope surface
                Vector3 slopeDirection = Vector3.ProjectOnPlane(_slideDirection, hit.normal).normalized;
                float incline = slopeDirection.y;

                if (incline < -0.05f)
                {
                    // Downhill: speed up based on steepness
                    _currentSlideSpeed += Mathf.Abs(incline) * _slopeAcceleration * Time.deltaTime;
                    _currentSlideSpeed = Mathf.Min(_currentSlideSpeed, _maxSlideSpeed);
                }
                else if (incline > 0.05f)
                {
                    // Uphill: lose speed quickly, never below zero
                    _currentSlideSpeed -= incline * _uphillResistance * Time.deltaTime;
                    _currentSlideSpeed = Mathf.Max(_currentSlideSpeed, 0f);
                }
                else
                {
                    // Flat: normal friction
                    _currentSlideSpeed = Mathf.MoveTowards(_currentSlideSpeed, 0f, _slideFriction * Time.deltaTime);
                }

                moveDirection = slopeDirection;
            }
            else
            {
                // Fallback if the raycast misses
                _currentSlideSpeed = Mathf.MoveTowards(_currentSlideSpeed, 0f, _slideFriction * Time.deltaTime);
            }
        }
        // In the air: keep horizontal speed, no friction

        Vector3 slideMove = moveDirection * _currentSlideSpeed + Vector3.up * _verticalVelocity;
        controller.Move(slideMove * Time.deltaTime);

        // Hit a wall: kill the speed (the slide then ends via the stop check)
        if ((controller.collisionFlags & CollisionFlags.Sides) != 0)
        {
            _currentSlideSpeed = 0f;
        }
    }

    private void StopSlide()
    {
        _isSliding = false;
        _wantsToStop = false;
        playerMovement.IsSliding = false;

        controller.height = _originalHeight;
        controller.center = _originalCenter;

        if (cameraTransform != null)
        {
            cameraTransform.localPosition = _originalCameraPos;
        }
    }

    // Returns true if there's enough headroom to return to full height
    private bool CanStandUp()
    {
        Bounds bounds = controller.bounds;
        float standingTop = bounds.min.y + _originalHeight;
        float distance = standingTop - bounds.center.y;

        return !Physics.Raycast(bounds.center, Vector3.up, distance, _groundMask, QueryTriggerInteraction.Ignore);
    }
}