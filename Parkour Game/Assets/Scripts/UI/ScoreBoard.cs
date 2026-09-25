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

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _textFields = new TextMeshProUGUI[20]; //Unity does not assign things at the start of the game, we have to do it manually
        
        //DISPLAY

        int i = 0;
        foreach (var se in GameManager.Instance.ScoreBoardEntries.OrderByDescending(s => s.Score).Take(10)) // ".Take(10) only accepts 10 results
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
