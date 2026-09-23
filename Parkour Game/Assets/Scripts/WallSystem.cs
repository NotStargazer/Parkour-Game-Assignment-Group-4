using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerMovement), typeof(CharacterController))]
public class WallSystem : MonoBehaviour
{
    private enum WallState { None, Running, Stuck }

    [Header("raycast")]
    [SerializeField] private Transform pelvisRayPoint;
    [SerializeField] private Transform torsoRayPoint;
    [SerializeField] private LayerMask wallLayer;

    [Header("detection")]
    [SerializeField] private float rayDistance = 1.2f;
    [SerializeField] private float sphereCastRadius = 0.2f;
    [SerializeField] private float stickDistance = 0.8f;
    [SerializeField] private float maxAngleDeviance = 15f;

    [Header("wall run")]
    [SerializeField] private float baseSpeed = 8f;

    [Header("wall stick")]
    [SerializeField] private float stickDuration = 1.0f;
    [SerializeField] private float stickCooldown = 0.5f;

    [Header("jump and climb")]
    [SerializeField] private float wallJumpUpForce = 8f;
    [SerializeField] private float wallJumpPushForce = 5f;
    [SerializeField] private float wallClimbForce = 12f;
    [SerializeField] private float rotationSmoothness = 15f;

    private CharacterController controller;
    private PlayerMovement playerMovement;
    private PlayerControls controls;

    private WallState currentState = WallState.None;
    private float stickTimer;
    private float cooldownTimer;

    private Vector3 wallTangent;
    private Vector3 currentWallNormal;
    private Coroutine activeRotationRoutine;

    public bool IsWallRunning => currentState == WallState.Running;
    public bool IsWallStuck => currentState == WallState.Stuck;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        playerMovement = GetComponent<PlayerMovement>();
        controls = new PlayerControls();
    }
    private void OnEnable()
    {
        controls.Player.Enable();
        controls.Player.Jump.performed += OnJumpPressed;
    }
    private void OnDisable()
    {
        controls.Player.Jump.performed -= OnJumpPressed;
        controls.Player.Disable();
    }
    private void Update()
    {
        if (cooldownTimer > 0f)
        {
            cooldownTimer -= Time.deltaTime;
        }

        switch (currentState)
        {
            case WallState.Running:
                UpdateWallRun();
                break;
            case WallState.Stuck:
                UpdateWallStick();
                break;
            case WallState.None:
                CheckWallTransitions();
                break;
        }
    }
    private void CheckWallTransitions()
    {
        if (controller.isGrounded || cooldownTimer > 0f) return;

        Vector2 moveInput = controls.Player.Move.ReadValue<Vector2>();
        if (moveInput.y <= 0.1f) return;

        if (Physics.Raycast(pelvisRayPoint.position, transform.forward, out RaycastHit stickHit, stickDistance, wallLayer))
        {
            float angle = Vector3.Angle(-stickHit.normal, transform.forward);
            if (angle <= maxAngleDeviance)
            {
                EnterStickState(stickHit);
                return;
            }
        }

        bool rightHit = Physics.SphereCast(pelvisRayPoint.position, sphereCastRadius, transform.right, out RaycastHit hitRight, rayDistance, wallLayer);
        bool leftHit = Physics.SphereCast(pelvisRayPoint.position, sphereCastRadius, -transform.right, out RaycastHit hitLeft, rayDistance, wallLayer);

        if (rightHit || leftHit)
        {
            RaycastHit activeHit = rightHit ? hitRight : hitLeft;
            currentWallNormal = activeHit.normal;

            Vector3 rawTangent = Vector3.Cross(currentWallNormal, Vector3.up);
            if (Vector3.Dot(rawTangent, transform.forward) < 0f)
            {
                rawTangent = -rawTangent;
            }

            wallTangent = rawTangent.normalized;
            EnterRunState();
        }
    }
    private void EnterRunState()
    {
        currentState = WallState.Running;
        playerMovement.enabled = false;
        playerMovement.SetVerticalVelocity(0f);

        if (wallTangent != Vector3.zero)
        {
            SmoothRotateTowards(wallTangent);
        }
    }
    private void UpdateWallRun()
    {
        playerMovement.SetVerticalVelocity(0f);

        Vector2 moveInput = controls.Player.Move.ReadValue<Vector2>();
        if (moveInput.y <= 0.1f)
        {
            ExitState();
            return;
        }

        Vector3 moveVelocity = wallTangent * baseSpeed;
        controller.Move(moveVelocity * Time.deltaTime);

        Vector3 directionToWall = -currentWallNormal;
        if (!Physics.Raycast(pelvisRayPoint.position, directionToWall, rayDistance + 0.5f, wallLayer))
        {
            ExitState();
        }
    }
    private void EnterStickState(RaycastHit hit)
    {
        currentState = WallState.Stuck;
        stickTimer = stickDuration;
        currentWallNormal = hit.normal;
        playerMovement.SetVerticalVelocity(0f);
        playerMovement.enabled = false;
    }
    private void UpdateWallStick()
    {
        stickTimer -= Time.deltaTime;
        playerMovement.SetVerticalVelocity(0f);

        if (stickTimer <= 0f)
        {
            ExitState();
        }
    }
    private void OnJumpPressed(InputAction.CallbackContext context)
    {
        if (currentState == WallState.None) return;

        bool torsoHit = Physics.Raycast(torsoRayPoint.position, transform.forward, rayDistance, wallLayer);

        if (currentState == WallState.Running)
        {
            ExitState();
            if (!torsoHit)
            {
                playerMovement.SetVerticalVelocity(wallClimbForce);
            }
            else
            {
                Vector3 jumpDirection = (transform.forward * 0.7f + currentWallNormal * 0.3f).normalized;

                SmoothRotateTowards(new Vector3(jumpDirection.x, 0f, jumpDirection.z).normalized);

                playerMovement.SetVerticalVelocity(wallJumpUpForce);
                playerMovement.SetCurrentSpeed(baseSpeed);
                controller.Move(jumpDirection * baseSpeed * Time.deltaTime * 1.5f);
            }
        }
        else if (currentState == WallState.Stuck)
        {
            ExitState();

            if (torsoHit)
            {
                Vector3 pushDirection = new Vector3(currentWallNormal.x, 0f, currentWallNormal.z).normalized;
                SmoothRotateTowards(pushDirection);

                playerMovement.SetVerticalVelocity(wallJumpUpForce);
                controller.Move(pushDirection * wallJumpPushForce * Time.deltaTime * 10f);
            }
            else
            {
                playerMovement.SetVerticalVelocity(wallClimbForce);
            }
        }
    }
    private void ExitState()
    {
        currentState = WallState.None;
        cooldownTimer = stickCooldown;
        playerMovement.SetCurrentSpeed(baseSpeed);
        playerMovement.enabled = true;
    }
    private void SmoothRotateTowards(Vector3 targetDirection)
    {
        if (targetDirection == Vector3.zero) return;

        if (activeRotationRoutine != null)
        {
            StopCoroutine(activeRotationRoutine);
        }
        activeRotationRoutine = StartCoroutine(RotateRoutine(Quaternion.LookRotation(targetDirection)));
    }
    private IEnumerator RotateRoutine(Quaternion targetRotation)
    {
        while (Quaternion.Angle(transform.rotation, targetRotation) > 0.5f)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSmoothness * Time.deltaTime);
            yield return null;
        }
        transform.rotation = targetRotation;
    }
}