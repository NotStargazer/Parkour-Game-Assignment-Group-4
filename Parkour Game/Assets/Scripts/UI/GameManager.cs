using System;
using System.Threading;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;
using Utility;

public class GameManager : SingletonBehaviour<GameManager>
{
    //Score
    public int Score;
    private float _timer;
    public float Timer { get => _timer; }
    [SerializeField] private float _maxTime;
    public float MaxTime { get => _maxTime; }

    private Action<string> _gameOverEvent;
    private Action<int> _increaseScoreEvent;
    private bool _isGameOver;

    [SerializeField] private int _scoreAmount;
    [SerializeField] private float _timeAmount;

    public override void Instantiate() //START FUNCTION
    {
        _timer = _maxTime;
    }

    void Update() //UPDATE
    {

        _timer -= Time.deltaTime;

        //Animations

        float _timeProgress = Mathf.InverseLerp(_maxTime, 0, _timer);


        if (Timer <= 0)
        {
            _gameOverEvent?.Invoke("TimeOut"); //GameOver by timeout
        }

        if (Keyboard.current.qKey.wasPressedThisFrame)
        {
            AddScore();
        }
    }

    void AddScore()
    {
        _timer += _timeAmount;
        Score += _scoreAmount;
        _increaseScoreEvent?.Invoke(_scoreAmount);
    }

    public void AddGameOverEvent(Action<string> gameOverEvent) //Invoked by PlayerHUD, establishes connection for gameOver screen
    {
        if (_gameOverEvent != null)
        {
            _gameOverEvent += gameOverEvent;
            return;
        }

        _gameOverEvent = gameOverEvent;
    }

    public void AddIncreaseScoreEvent(Action<int> increaseScoreEvent) //Invoked by PlayerHUD, establishes connection to scoring
    {
        if (_increaseScoreEvent != null)
        {
            _increaseScoreEvent += increaseScoreEvent;
            return;
        }

        _increaseScoreEvent = increaseScoreEvent;
    }

    public void FallToDeath()
    {
        _gameOverEvent?.Invoke("PlayerFell"); //GameOver by falling
    }
}
