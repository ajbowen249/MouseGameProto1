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
    public TMP_Text GasMeter;
    public TMP_Text EnergyMeter;
    public TMP_Text TimeMeter;
    public TMP_Text DialogText;
    public TMP_Text DialogOptions;

    public int MessageLogLines = 5;
    public int MessageLogTimeSeconds = 7;

    public static HUD Instance { get; private set; }

    private List<string> _messages = new List<string>();
    private float _lastNewLog = 0;

    private DialogNode _dialog;
    private int _dialogIndex;

    void Start()
    {
        Instance = this;

        ClearInteractionPrompt();
        MessageLog.text = "";
        DialogText.text = "";
        DialogOptions.text = "";
        GasMeter.text = "";
        EnergyMeter.text = "";
        TimeMeter.text = "";
    }

    void Update()
    {
        if (_lastNewLog < Time.fixedTime - MessageLogTimeSeconds && MessageLog.text != "")
        {
            MessageLog.text = "";
            _messages = new List<string>();
        }

        if (_dialog != null)
        {
            DialogText.text = _dialog.Message;
            var optionsLines = "";

            for (int i = 0; i < _dialog.Options.Count; i++)
            {
                var option = _dialog.Options[i];
                if (i == _dialogIndex)
                {
                    optionsLines += "<b><color=\"yellow\">";
                }

                optionsLines += option.Text;

                if (i == _dialogIndex)
                {
                    optionsLines += "</color></b>";
                }

                optionsLines += "\n";
            }

            DialogOptions.text = optionsLines;
        }
        else if (DialogText.text != "" || DialogOptions.text != "")
        {
            DialogText.text = "";
            DialogOptions.text = "";
        }
    }

    public void SetInteractionPrompt(string prompt)
    {
        Prompt.text = prompt;
    }

    public void SetDialog(DialogNode node, int selectedIndex)
    {
        _dialog = node;
        _dialogIndex = selectedIndex;
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

    public void SetMeters(float gas, float energy, float time)
    {
        GasMeter.text = $"Gas: {gas}";
        EnergyMeter.text = $"Energy: {energy}";
        TimeMeter.text = $"Time: {time}";
    }
}
