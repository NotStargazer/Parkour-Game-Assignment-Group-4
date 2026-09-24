using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController), typeof(PlayerMovement))] //requirements
public class WallStick : MonoBehaviour
{
    [Header("raycast points")]
    [SerializeField] private Transform pelvisRayPoint;
    [SerializeField] private Transform torsoRayPoint;

    [Header("detection")]
    [SerializeField] private float stickDistance = 0.8f;
    [SerializeField] private LayerMask wallLayer;
    [SerializeField] private float maxAngleDeviance = 45f;

    [Header("stick settings")]
    [SerializeField] private float stickduration = 1.0f;
    [SerializeField] private float stickCooldown = 0.5f; //this fixes the infinite stick loop problem

    [Header("launch forces")]
    [SerializeField] private float wallClimbLaunchForce = 12f;
    [SerializeField] private float wallJumpUpForce = 8f;
    [SerializeField] private float wallJumpPushForce = 6f;

    private CharacterController controller;
    private PlayerMovement playerMovement;
    private WallRun wallRun;
    private PlayerControls controls;

    private bool isWallStuck;
    private float stickTimer;
    private float cooldownTimer;
    private Vector3 currentWallNormal;

    public bool IsWallStuck => isWallStuck;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        playerMovement = GetComponent<PlayerMovement>();
        wallRun = GetComponent<WallRun>();
        controls = new PlayerControls();
    }

    private void OnEnable()
    {
        controls.Player.Enable();
        controls.Player.Jump.performed += OnJumpPressed;
    }

    private void OnDisable() //in case of bugs
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

        if (isWallStuck)
        {
            UpdateStickState();
        }
        else
        {
            CheckStickConditions();
        }
    }

    private void CheckStickConditions() //this checks if you're valid to stick to the wall
    {
        if (controller.isGrounded || cooldownTimer > 0f) return;
        if (wallRun != null && wallRun.IsWallRunning) return;

        Vector2 moveInput = controls.Player.Move.ReadValue<Vector2>();
        if (moveInput.y <= 0.1f) return;

        if (Physics.Raycast(pelvisRayPoint.position, transform.forward, out RaycastHit hit, stickDistance, wallLayer))
        {
            float angle = Vector3.Angle(-hit.normal, transform.forward);
            if (angle <= maxAngleDeviance)
            {
                EnterStickState(hit);
            }
        }
    }

    private void EnterStickState(RaycastHit hit)//sticks u to the wall
    {
        isWallStuck = true;
        stickTimer = stickduration;
        currentWallNormal = hit.normal;
        playerMovement.SetVerticalVelocity(0f);
        playerMovement.enabled = false;
    }

    private void UpdateStickState()
    {
        stickTimer -= Time.deltaTime;
        playerMovement.SetVerticalVelocity(0f);

        if (stickTimer <= 0f)
        {
            ExitStickState();
        }
    }

    private void OnJumpPressed(InputAction.CallbackContext context)
    {
        if (!isWallStuck) return;
        bool torsoHit = Physics.Raycast(torsoRayPoint.position, transform.forward, stickDistance, wallLayer);

        if (torsoHit)
        {
            Vector3 pushDirection = new Vector3(currentWallNormal.x, 0f, currentWallNormal.z).normalized;
            if (pushDirection != Vector3.zero)
            {
                transform.rotation = Quaternion.LookRotation(pushDirection);
            }

            ExitStickState();
            playerMovement.SetVerticalVelocity(wallJumpUpForce);
            controller.Move(pushDirection * wallJumpPushForce * Time.deltaTime * 10f);
        }
        else
        {
            ExitStickState();
            playerMovement.SetVerticalVelocity(wallClimbLaunchForce);
        }
    }

    private void ExitStickState()
    {
        isWallStuck = false;
        cooldownTimer = stickCooldown;
        playerMovement.enabled = true;
    }

    public void ExitStickStateExternal()
    {
        ExitStickState();
    }
}