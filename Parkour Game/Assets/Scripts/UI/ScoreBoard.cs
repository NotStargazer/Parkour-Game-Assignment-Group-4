using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro;
using UnityEngine;

[System.Serializable]
public struct ScoreBoardData
{
    [SerializeField] public List<ScoreBoardEntry> ScoreBoardEntries;
}

[System.Serializable]
public struct ScoreBoardEntry
{
    [SerializeField] public string Name;
    [SerializeField] public int Score;
}
public class ScoreBoard : MonoBehaviour
{
    [SerializeField] TMP_Text _scoreBoardEntryText;
    private TMP_Text[] _textFields;
    private ScoreBoardData _scoreBoardData = new ScoreBoardData
    {
        ScoreBoardEntries = new List<ScoreBoardEntry>
        {
            //new ScoreBoardEntry{ Name = "Midas", Score = 68 },
            //new ScoreBoardEntry{ Name = "Sam", Score = 419 },
            //new ScoreBoardEntry{ Name = "Simone", Score = 600 },
            //new ScoreBoardEntry{ Name = "Simgone", Score = 600 },
            //new ScoreBoardEntry{ Name = "Simghone", Score = 56 },
            //new ScoreBoardEntry{ Name = "Simone", Score = 564 },
            //new ScoreBoardEntry{ Name = "hgh", Score = 667 },
            //new ScoreBoardEntry{ Name = "Simone", Score = 555 },
            //new ScoreBoardEntry{ Name = "gh", Score = 65 },
            //new ScoreBoardEntry{ Name = "ghhg", Score = 77 },
            //new ScoreBoardEntry{ Name = "Simgggone", Score = 11 },
            //new ScoreBoardEntry{ Name = "Simo55ne", Score = 444 }
        }
    };

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        var path = Application.persistentDataPath + "/Scores.json";

        //LOAD LOGIC

        string jsonRead = File.ReadAllText(path);
        _scoreBoardData = JsonUtility.FromJson<ScoreBoardData>(jsonRead);

        _textFields = new TextMeshProUGUI[20]; //Unity does not assign things at the start of the game, we have to do it manually
        
        //DISPLAY

        int i = 0;
        foreach (var se in _scoreBoardData.ScoreBoardEntries.OrderByDescending(s => s.Score).Take(10)) // ".Take(10) only accepts 10 results
        {
            _textFields[i] = Instantiate(_scoreBoardEntryText, transform);
            _textFields[i].text = se.Name;

            _textFields[i + 1] = Instantiate(_scoreBoardEntryText, transform);
            _textFields[i + 1].text = se.Score.ToString();

            i += 2;
        }

        //SAVE LOGIC

        //var json = JsonUtility.ToJson(_scoreBoardData);

        //if (!File.Exists(path))
        //{
        //    File.Create(path);
        //}

        //File.WriteAllText(path, json);
    }

    // Update is called once per frame
    void Update()
    {

    }
}
