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

    public GameObject PlayerObject;

    public int MessageLogLines = 5;
    public int MessageLogTimeSeconds = 7;

    public static HUD Instance { get; private set; }

    private List<string> _messages = new List<string>();
    private float _lastNewLog = 0;

    private DialogNode _dialog;
    private int _dialogIndex;

    private MouseController _player;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }

        Instance = this;
    }

    void Start()
    {
        ClearInteractionPrompt();
        MessageLog.text = "";
        DialogText.text = "";
        DialogOptions.text = "";
        GasMeter.text = "";
        EnergyMeter.text = "";
        TimeMeter.text = "";

        _player = PlayerObject.GetComponent<MouseController>();
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
            DrawDialog();
        }
        else if (DialogText.text != "" || DialogOptions.text != "")
        {
            DialogText.text = "";
            DialogOptions.text = "";
        }
    }

    public void SetPlayer(MouseController player)
    {
        _player = player;
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
        TimeMeter.text = $"Time: {TimeUtils.FormatHours(time)}";
    }

    private void DrawDialog()
    {
        if (_player == null)
        {
            return;
        }

        DialogText.text = _dialog.Message;
        var optionsLines = "";

        for (int i = 0; i < _dialog.Options.Count; i++)
        {
            var option = _dialog.Options[i];
            optionsLines += FormatDialogOption(option, i == _dialogIndex) + "\n";
        }

        DialogOptions.text = optionsLines;
    }

    private string FormatDialogOption(DialogOption option, bool isSelected)
    {
        var color = _player.CanSelectDialogOption(option) ?
            (isSelected ? "yellow" : "white") :
            (isSelected ? "#5c5c01" : "#505050");

        var text = option.Text;

        if (option.Cost?.gas is float gas)
        {
            text += $" G {gas}";
        }

        if (option.Cost?.energy is float energy)
        {
            text += $" E {energy}";
        }

        if (option.Cost?.time is float time)
        {
            text += $" T {TimeUtils.FormatHours(time)}";
        }

        return $"{(isSelected ? "<b>" : "")}<color={color}>{text}</color>{(isSelected ? "</b>" : "")}";
    }
}
