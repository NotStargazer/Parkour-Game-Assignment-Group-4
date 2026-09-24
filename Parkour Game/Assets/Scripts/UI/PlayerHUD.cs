using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerHUD : MonoBehaviour
{
    [SerializeField] private Camera _playerCam;
    [SerializeField] private PlayerMovement _playerMovement;
    [SerializeField] private Animation _anim;
    [SerializeField] private TMP_Text _scoreText;
    

    
    AnimationState _timeState;
    AnimationState _speedState;
    AnimationState _fallState;
    AnimationState _timeOutState;

    AnimationState _scoreState;
    AnimationState _scoreStateZero;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        GameManager.Instance.AddGameOverEvent(GameOverEvent);
        GameManager.Instance.AddIncreaseScoreEvent(IncreaseScoreEvent);

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

        _scoreState = _anim["IncreaseScore"];
        _scoreState.layer = 2;

        _scoreStateZero = _anim["IncreaseScoreZero"];
        _scoreStateZero.layer = 2;

        _fallState = _anim["PlayerFall"];
        _fallState.layer = 5;

        _timeOutState = _anim["PlayerTimeOut"];
        _timeOutState.layer = 6;

    }

    // Update is called once per frame
    void Update()
    {
        float _speedProgress = Mathf.InverseLerp(0, _playerMovement.MaxSpeed, _playerMovement.Speed); //change with rigidbody velocity later
        _speedState.normalizedTime = _speedProgress;

        float _timeProgress = Mathf.InverseLerp(GameManager.Instance.MaxTime, 0, GameManager.Instance.Timer);
       _timeState.normalizedTime = _timeProgress;
    }

    private void GameOverEvent(string reasonOfDeath)
    {
        _anim.Play(reasonOfDeath);
    }

    private void IncreaseScoreEvent(int score)
    {
        // float scoreMagnitude = Mathf.InverseLerp(0, Mathf.Min(score, 50), score); //This part doesn't work unfortunately.
        _anim.Stop("IncreaseScore");
        _anim.Play("IncreaseScore");
        _scoreText.text = GameManager.Instance.Score.ToString();
    }
}
