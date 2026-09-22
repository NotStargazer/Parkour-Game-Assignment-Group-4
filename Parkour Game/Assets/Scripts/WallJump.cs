using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController), typeof(PlayerMovement), typeof(WallStick))]
public class WallJump : MonoBehaviour
{
    [Header("raycast")]
    [SerializeField] private Transform torsoRayPoint;
    [SerializeField] private float rayDistance = 0.8f;
    [SerializeField] private LayerMask wallLayer;

    [Header("force when launching")]
    [SerializeField] private float launchForce = 12f;

    private CharacterController controller;
    private PlayerMovement playerMovement;
    private WallStick wallStick;
    private PlayerControls controls;

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
        controls.Player.Jump.performed += WallJumpAttempt;
    }

    private void OnDisable()
    {
        controls.Player.Jump.performed -= WallJumpAttempt;
        controls.Player.Disable();
    }

    private void WallJumpAttempt(InputAction.CallbackContext context)
    {
        if (!wallStick.IsWallStuck) return;
        if (Physics.Raycast(torsoRayPoint.position, transform.forward, out RaycastHit hit, rayDistance, wallLayer))
        {
            AllowWallJump(hit.normal);
        }
    }

    private void AllowWallJump(Vector3 wallNormal)
    {
        Vector3 launchDirection = (Vector3.up + wallNormal).normalized;

        Vector3 horizontalDirection = new Vector3(launchDirection.x, 0f, launchDirection.z).normalized;
        if (horizontalDirection != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(horizontalDirection);
        }

        wallStick.ExitStickStateExternal();

        float verticalLaunchSpeed = launchDirection.y * launchForce;
        playerMovement.SetVerticalVelocity(verticalLaunchSpeed);
        Vector3 horizontalImpulse = horizontalDirection * (launchDirection.magnitude * launchForce);
        controller.Move(horizontalImpulse * Time.deltaTime);
    }

} //67 xD