using UnityEngine;

public class PlayerHUD : MonoBehaviour
{
    [SerializeField] private Camera _playerCam;
    [SerializeField] private PlayerMovement _playerMovement;
    [SerializeField] private Animation _anim;

    private float _timer;
    [SerializeField] private float _maxTime;

    AnimationState _speedState;
    AnimationState _timeState;

    private Vector3 lastRotationAngle;

    private float _uiX;
    private float _uiY;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _speedState = _anim["PlayerSpeed"];

        _speedState.wrapMode = WrapMode.ClampForever;
        _speedState.speed = 0;
        _speedState.layer = 0;

        _timeState = _anim["PlayerTime"];

        _timeState.wrapMode = WrapMode.ClampForever;
        _timeState.speed = 0;
        _timeState.layer = 1;



        _anim.Play("PlayerSpeed");
        _anim.Play("PlayerTime");

        _timer = _maxTime;
    }

    // Update is called once per frame
    void Update()
    {
        _timer -= Time.deltaTime;

        //Animations

        float _speedProgress = Mathf.InverseLerp(_playerMovement.minSpeed, _playerMovement.maxSpeed, _playerMovement.currentSpeed); //change with rigidbody velocity later
        _speedState.normalizedTime = _speedProgress;

        float _timeProgress = Mathf.InverseLerp(_maxTime, 0, _timer);
        _timeState.normalizedTime = _timeProgress;

        //HUD movements

        Vector3 rotationAngle = _playerCam.transform.rotation.eulerAngles;

        float deltaRotX = rotationAngle.x - lastRotationAngle.x;
        float deltaRotY = rotationAngle.y - lastRotationAngle.y;

    }
}
