using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HUD : MonoBehaviour
{
    public GameObject Canvas;
    public TMP_Text Prompt;
    public TMP_Text MessageLog;
    public TMP_Text EnergyMeter;
    public TMP_Text DialogText;
    public TMP_Text DialogOptions;

    public int MessageLogLines = 5;
    public int MessageLogTimeSeconds = 7;

    public static HUD Instance { get; private set; }

    private List<string> _messages = new List<string>();
    private float _lastNewLog = 0;

    void Start()
    {
        Instance = this;

        ClearInteractionPrompt();
        MessageLog.text = "";
        EnergyMeter.text = "";
        DialogText.text = "";
        DialogOptions.text = "";
    }

    void Update()
    {
        if (_lastNewLog < Time.fixedTime - MessageLogTimeSeconds && MessageLog.text != "")
        {
            MessageLog.text = "";
            _messages = new List<string>();
        }
    }

    public void SetInteractionPrompt(string prompt)
    {
        Prompt.text = prompt;
    }

    public void ClearInteractionPrompt()
    {
        Prompt.text = "";
    }

    public void AddMessage(string message)
    {
        _messages.Add(message);

        var toRender = _messages.Count() <= MessageLogLines ? _messages :
            _messages.Skip(_messages.Count() - MessageLogLines);

        MessageLog.text = string.Join("\n", toRender.ToArray());

        _lastNewLog = Time.fixedTime;
    }

    public void SetEnergy(float value)
    {
        EnergyMeter.text = $"Energy: {value}";
    }
}
