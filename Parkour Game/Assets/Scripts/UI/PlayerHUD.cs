using System.Collections;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PlayerHUD : MonoBehaviour
{
    [SerializeField] private Camera _playerCam;
    [SerializeField] private PlayerMovement _playerMovement;
    [SerializeField] private Animation _animation;
    [SerializeField] private TMP_Text _scoreText;

    [SerializeField] private TMP_InputField _inputField;
    [SerializeField] private TMP_Text _finalScoreText;
    [SerializeField] private Button _backToMenu;
    
    private AnimationState _timeState;
    private AnimationState _speedState;
    private AnimationState _fallState;
    private AnimationState _timeOutState;
    private AnimationState _scoreState;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Start()
    {
        GameManager.Instance.AddGameOverEvent(GameOverEvent);
        GameManager.Instance.AddIncreaseScoreEvent(IncreaseScoreEvent);
        _backToMenu.onClick.AddListener(() =>
        {
            if (!string.IsNullOrEmpty(_inputField.text))
            {
                StartCoroutine(WaitForAnimationToFinish("ExitGameOver", endFunction: () =>
                {
                    GameManager.Instance.SaveScore(_inputField.text);
                    SceneManager.LoadScene("Menu");
                }));
            }
        });

        _speedState = _animation["PlayerSpeed"];
        _speedState.wrapMode = WrapMode.ClampForever;
        _speedState.speed = 0;
        _speedState.layer = 0;

        _timeState = _animation["PlayerTime"];
        _timeState.wrapMode = WrapMode.ClampForever;
        _timeState.speed = 0;
        _timeState.layer = 1;

        StartCoroutine(WaitForAnimationToFinish("Start", endFunction: () =>
        {
            _animation.Play("PlayerSpeed");
            _animation.Play("PlayerTime");
        }));
        
        _scoreState = _animation["IncreaseScore"];
        _scoreState.layer = 2;

        _fallState = _animation["FallAnim"];
        _fallState.layer = 5;

        _timeOutState = _animation["TimeOutAnim"];
        _timeOutState.layer = 6;

    }

    // Update is called once per frame
    private void Update()
    {
        if (GameManager.Instance.IsGameOver)
        {
            return;
        }

        float speedProgress = Mathf.InverseLerp(0, _playerMovement.MaxSpeed, _playerMovement.Speed); //change with rigidbody velocity later
        _speedState.normalizedTime = speedProgress;

        float timeProgress = Mathf.InverseLerp(GameManager.Instance.MaxTime, 0, GameManager.Instance.Timer);
       _timeState.normalizedTime = timeProgress;
    }
    private void IncreaseScoreEvent(int score)
    {
        _animation.Stop("IncreaseScore");
        _animation.Play("IncreaseScore");
        _scoreText.text = GameManager.Instance.Score.ToString();
    }


    //GAMEOVER
    private void GameOverEvent(string deathAnim)
    {
        if (!_animation)
        {
            return;
        }
        
        _animation.Stop("PlayerSpeed");
        _animation.Stop("PlayerTime");
        _animation.Play(deathAnim);
        _finalScoreText.SetText(GameManager.Instance.Score.ToString());
        StartCoroutine(WaitForAnimationToFinish("ShowGameOver"));
    }
    
    private IEnumerator WaitForAnimationToFinish(string animName, bool inverted = false, System.Action endFunction = null)
    {
        var eventSystem = EventSystem.current; //This establishes a reference first, by creating a variable
        eventSystem.enabled = false; //EventSystem is the central controller for Unity's UI interaction
        eventSystem.SetSelectedGameObject(null); //EventSystem selects objects, and that is how buttons determine their behaviors

        if (inverted)
        {
            _animation[animName].time = _animation[animName].length;
            _animation[animName].speed = -1f;
        }
        else
        {
            _animation[animName].time = 0f;
            _animation[animName].speed = 1f;
        }

        _animation.Play(animName);

        while (_animation.IsPlaying(animName))
        {
            yield return new WaitForNextFrameUnit(); //return null doesn't wait. WaitForNextFrameUnit waits a frame
        }

        eventSystem.enabled = true;

        endFunction?.Invoke(); // the "?" checks for null, if null don't execute the rest
    }
}
