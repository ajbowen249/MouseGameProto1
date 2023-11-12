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

    public int MessageLogLines = 5;
    public int MessageLogTimeSeconds = 5;

    public static HUD Instance { get; private set; }

    private List<string> _messages = new List<string>();
    private float _lastNewLog = 0;

    void Start()
    {
        Instance = this;

        if (Canvas == null || Prompt == null || MessageLog == null)
        {
            Debug.LogError("One or more elements is null.");
        }

        ClearInteractionPrompt();
        MessageLog.text = "";
    }

    void Update()
    {
        if (_lastNewLog < Time.fixedTime - MessageLogTimeSeconds && MessageLog.text != "")
        {
            MessageLog.text = "";
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
}
