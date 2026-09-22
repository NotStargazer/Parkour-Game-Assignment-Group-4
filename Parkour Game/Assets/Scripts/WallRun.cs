using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerMovement), typeof(CharacterController))]
public class WallRun : MonoBehaviour
{
    [Header("wall detection")]
    [SerializeField] private Transform pelvisRayPoint;
    [SerializeField] private Transform torsoRayPoint;
    [SerializeField] private LayerMask wallLayer;
    [SerializeField] private float rayDistance = 0.8f;

    [Header("speed settings")]
    [SerializeField] private float baseSpeed = 5f;
    [SerializeField] private float momentumDecayRate = 2.5f;

    [Header("forces")]
    [SerializeField] private float wallJumpForce = 10f;
    [SerializeField] private float wallClimbForce = 12f;

    private CharacterController controller;
    private PlayerMovement playerMovement;
    private WallStick wallStick;
    private PlayerControls controls;

    private bool isWallRunning;
    private float currentMomentum;
    private Vector3 wallTangent;
    private Vector3 currentWallNormal;
    private bool isWallOnRight;
    public bool IsWallRunning => isWallRunning;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        playerMovement = GetComponent<PlayerMovement>();
        wallStick = GetComponent<WallStick>();
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
        if (isWallRunning)
        {
            UpdateWallRunState();
        }
        else
        {
            CheckWallRunConditions();
        }
    }
    private void CheckWallRunConditions()
    {
        if (controller.isGrounded) return;
        if (wallStick != null && wallStick.IsWallStuck) return;
        bool rightHit = Physics.Raycast(pelvisRayPoint.position, transform.right, out RaycastHit hitRight, rayDistance, wallLayer);
        bool leftHit = Physics.Raycast(pelvisRayPoint.position, -transform.right, out RaycastHit hitLeft, rayDistance, wallLayer);
        if (!rightHit && !leftHit) return;

        RaycastHit activeHit = rightHit ? hitRight : hitLeft;
        isWallOnRight = rightHit;
        currentWallNormal = activeHit.normal;

        Vector3 rawTangent = Vector3.Cross(currentWallNormal, Vector3.up);
        if (isWallOnRight)
        {
            rawTangent = -rawTangent;
        }
        wallTangent = rawTangent.normalized;

        float speedAlongWall = Vector3.Dot(controller.velocity, wallTangent);
        if (speedAlongWall >= baseSpeed * 1.0f)
        {
            EnterWallRun(speedAlongWall);
        }
    }

    private void EnterWallRun(float initialSpeed)
    {
        isWallRunning = true;
        currentMomentum = initialSpeed;
        playerMovement.enabled = false;
        playerMovement.SetVerticalVelocity(0f);
        if (wallTangent != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(wallTangent);
        }
    }

    private void UpdateWallRunState()
    {
        playerMovement.SetVerticalVelocity(0f);
        currentMomentum -= momentumDecayRate * Time.deltaTime;
        if (currentMomentum <= baseSpeed * 0.5f)
        {
            ExitWallRun();
            return;
        }
        Vector3 runVelocity = wallTangent * currentMomentum;
        controller.Move(runVelocity * Time.deltaTime);
        Vector3 checkDirection = isWallOnRight ? transform.right : -transform.right;
        if (!Physics.Raycast(pelvisRayPoint.position, checkDirection, rayDistance, wallLayer))
        {
            ExitWallRun();
        }
    }

    private void OnJumpPressed(InputAction.CallbackContext context)
    {
        if (!isWallRunning) return;
        bool torsoHit = Physics.Raycast(torsoRayPoint.position, transform.forward, rayDistance, wallLayer);
        ExitWallRun();

        if (torsoHit)
        {
            playerMovement.SetVerticalVelocity(wallClimbForce);
        }
        else
        {
            Vector3 pushDirection = (Vector3.up + currentWallNormal).normalized;
            Vector3 horizontalDirection = new Vector3(pushDirection.x, 0f, pushDirection.z).normalized;

            if (horizontalDirection != Vector3.zero)
            {
                transform.rotation = Quaternion.LookRotation(horizontalDirection);
            }
            playerMovement.SetVerticalVelocity(wallJumpForce);
            controller.Move(pushDirection * wallJumpForce * Time.deltaTime * 2f);
        }
    }
    private void ExitWallRun()
    {
        isWallRunning = false;
        playerMovement.enabled = true;
    }
}