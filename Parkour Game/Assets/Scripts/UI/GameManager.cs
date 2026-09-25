using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using TMPro;
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
    public bool IsGameOver => _isGameOver;

    public IReadOnlyCollection<ScoreBoardEntry> ScoreBoardEntries => _scoreBoardData.ScoreBoardEntries;

    private Action<string> _gameOverEvent;
    private Action<int> _increaseScoreEvent;
    private bool _isGameOver;

    [SerializeField] private int _scoreAmount;
    [SerializeField] private float _timeAmount;

    private ScoreBoardData _scoreBoardData;

    public override void Instantiate() //START FUNCTION
    {
        LoadScores();
        _timer = _maxTime;
        _gameOverEvent = _ =>
        {
            UnityEngine.Cursor.lockState = CursorLockMode.None;
            UnityEngine.Cursor.visible = true;
        };
    }

    private void LoadScores()
    {
        var path = Application.persistentDataPath + "/Scores.json";

        //LOAD LOGIC

        string jsonRead = File.ReadAllText(path);
        _scoreBoardData = JsonUtility.FromJson<ScoreBoardData>(jsonRead);
    }

    public void SaveScore(string name)
    {
        var path = Application.persistentDataPath + "/Scores.json";
        _scoreBoardData.ScoreBoardEntries.Add(new ScoreBoardEntry { Name = name, Score = Score });
        var json = JsonUtility.ToJson(_scoreBoardData);
        File.WriteAllText(path, json);
    }

    public void StartSession()
    {
        Score = 0;
        _timer = _maxTime;
        UnityEngine.Cursor.lockState = CursorLockMode.Locked;
        UnityEngine.Cursor.visible = false;
    }

    void Update() //UPDATE
    {

        _timer -= Time.deltaTime;

        //Animations

        float timeProgress = Mathf.InverseLerp(_maxTime, 0, _timer);

        
        if (Timer <= 0 && !_isGameOver)
        {
            _isGameOver = true;
            _gameOverEvent?.Invoke("TimeOutAnim"); //GameOver by timeout
        }

        if (Keyboard.current.qKey.wasPressedThisFrame)
        {
            AddScore(_scoreAmount);
        }
    }

    public void AddScore(int score)
    {
        _timer += score / 5;
        Score += score;
        _increaseScoreEvent?.Invoke(score);
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
        _gameOverEvent?.Invoke("FallAnim"); //GameOver by falling
    }

}
