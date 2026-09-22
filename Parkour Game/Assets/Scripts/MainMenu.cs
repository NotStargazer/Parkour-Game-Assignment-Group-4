using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using UnityEngine.Assertions.Must;
using UnityEngine.EventSystems;
using Unity.VisualScripting;

public class MainMenu : MonoBehaviour
{
    [SerializeField] private Button _playButton;
    [SerializeField] private Button _scoreButton;
    [SerializeField] private Button _exitButton;

    [SerializeField] private Animation _animation;

    private bool _showingScore = false; // adding "_" at the beginning + camelCase for fields

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    // I wanna trigger a looped animation when the intro finishes.
    void Start()
    {
        _playButton.onClick.AddListener(PlayGame);
        _scoreButton.onClick.AddListener(ShowScore);
        _exitButton.onClick.AddListener(ExitGame);
    }

    void PlayGame()
    {
        StartCoroutine(WaitForAnimationToFinish("FadeExit", false, () => {SceneManager.LoadScene("Level");}));
    }

    void ShowScore()
    {
        if (!_showingScore)
        {
            _showingScore = true;
            StartCoroutine(WaitForAnimationToFinish("ScoreTransition", false));
        }
    }

    void ExitGame()
    {
        if (_showingScore)
        {
            StartCoroutine(WaitForAnimationToFinish("ScoreTransition", true, () => { _showingScore = false; }));
        }
        else
        {
            StartCoroutine(WaitForAnimationToFinish("FadeExit", false, () =>
            {
#if UNITY_EDITOR //"#" is a pre-processor, checks for the environment flags (flags = yes/no statement) before the code compiles. Should be at the very left, standard code practice.
                UnityEditor.EditorApplication.ExitPlaymode();
#else
            Application.Quit();
#endif
            }));
        }
        
    }



    IEnumerator WaitForAnimationToFinish(string animName, bool inverted = false, System.Action endFunction = null)
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
