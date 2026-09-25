using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.InputSystem;
using Utility;

[Serializable]
public struct ScoreBoardData
{
    [SerializeField] public List<ScoreBoardEntry> ScoreBoardEntries;
}

[Serializable]
public struct ScoreBoardEntry
{
    [SerializeField] public string Name;
    [SerializeField] public int Score;
}

public class GameManager : SingletonBehaviour<GameManager>
{
    //Score
    public int Score;
    private float _timer;
    public float Timer { get => _timer; }
    [SerializeField] private float _maxTime;
    public float MaxTime { get => _maxTime; }
    public float MouseSens { get; set; }
    public bool IsGameOver => _isGameOver;

    public IReadOnlyCollection<ScoreBoardEntry> ScoreBoardEntries => _scoreBoardData.ScoreBoardEntries;

    private Action<string> _gameOverEvent;
    private Action<int> _increaseScoreEvent;
    private bool _isGameOver;

    [SerializeField] private int _scoreAmount;
    [SerializeField] private float _timeAmount;

    private ScoreBoardData _scoreBoardData;
    private float _actualMax;

    public override void Instantiate() //START FUNCTION
    {
        MouseSens = 20;
        LoadScores();
        _actualMax = _maxTime;
    }

    private void LoadScores()
    {
        var path = Application.persistentDataPath + "/Scores.json";

        //LOAD LOGIC

        if (File.Exists(path))
        {
            string jsonRead = File.ReadAllText(path);
            _scoreBoardData = JsonUtility.FromJson<ScoreBoardData>(jsonRead);
        }
        else
        {
            _scoreBoardData = new ScoreBoardData
            {
                ScoreBoardEntries = new List<ScoreBoardEntry>()
            };
        }
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
        _gameOverEvent = null;
        _increaseScoreEvent = null;
        
        _gameOverEvent = _ =>
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        };
        
        Score = 0;
        _maxTime = _actualMax;
        _timer = _maxTime + 1;
        _isGameOver = false;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update() //UPDATE
    {
        _timer -= Time.deltaTime;
        
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
        _timer += score / 10f;
        Score += score;
        _maxTime = Mathf.Max(_maxTime, _timer);
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
