using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController), typeof(PlayerMovement), typeof(WallStick))]
public class WallClimb : MonoBehaviour
{
    [Header("raycast")]
    [SerializeField] private Transform torsoRayPoint;
    [SerializeField] private float rayDistance = 0.8f;
    [SerializeField] private LayerMask wallLayer;

    [Header("launch force")]
    [SerializeField] private float climbLaunchForce = 12f;

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
        controls.Player.Jump.performed += WallClimbAttempt;
    }
    private void OnDisable()
    {
        controls.Player.Jump.performed -= WallClimbAttempt;
        controls.Player.Disable();
    }
    private void WallClimbAttempt(InputAction.CallbackContext context) 
    {

        if (!wallStick.IsWallStuck) return;
        bool torsoHit = Physics.Raycast(torsoRayPoint.position, transform.forward, rayDistance, wallLayer);
        if (!torsoHit)
        {
            AllowWallClimb();
        }
    }
    private void AllowWallClimb()
    {
        wallStick.ExitStickStateExternal();
        playerMovement.SetVerticalVelocity(climbLaunchForce);
    }
}