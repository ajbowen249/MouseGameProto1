using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.InputSystem;
using System;

public class HUD : MonoBehaviour
{
    public TMP_Text Prompt;
    public TMP_Text MessageLog;

    public TMP_Text GasMeter;
    public TMP_Text EnergyMeter;
    public TMP_Text TimeMeter;
    public TMP_Text MoneyMeter;

    public TMP_Text DialogText;
    public TMP_Text DialogOptions;

    public GameObject PlayerObject;

    public GameObject ConsoleContainer;
    public TMP_InputField ConsoleInput;
    public GameObject ConsoleBaseLine;
    public Scrollbar ConsoleScrollBar;
    public GameObject ConsoleRect;

    public int MessageLogLines = 5;
    public int MessageLogTimeSeconds = 7;

    public static HUD Instance { get; private set; }

    private List<string> _messages = new List<string>();
    private float _lastNewLog = 0;

    private DialogNode _dialog;
    private int _dialogIndex;

    private MouseController _player;

    private bool _isConsoleOpen = false;

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
        ConsoleContainer.SetActive(false);
        _isConsoleOpen = false;
        ClearInteractionPrompt();
        MessageLog.text = "";
        DialogText.text = "";
        DialogOptions.text = "";
        GasMeter.text = "";
        EnergyMeter.text = "";
        TimeMeter.text = "";
        MoneyMeter.text = "";

        _player = PlayerObject.GetComponent<MouseController>();

        ConsoleInput.onSubmit.AddListener(value => HandleConsoleInput(value));
    }

    void Update()
    {
        Cursor.lockState = _isConsoleOpen ? CursorLockMode.None : CursorLockMode.Locked;
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

        if (Keyboard.current[Key.Backquote].wasPressedThisFrame)
        {
            if (!_isConsoleOpen)
            {
                OpenConsole();
            }
            else
            {
                CloseConsole();
            }
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

        WriteConsole(message);
    }

    public void SetMeters(float gas, float energy, float time, float money)
    {
        GasMeter.text = $"Gas: {gas}";
        EnergyMeter.text = $"Energy: {energy}";
        TimeMeter.text = $"Time: {FormatUtils.FormatHours(time)}";
        MoneyMeter.text = FormatUtils.FormatMoney(money);
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
            text += $" T {FormatUtils.FormatHours(time)}";
        }

        if (option.Cost?.money is float money)
        {
            text += $" {FormatUtils.FormatMoney(money)}";
        }

        return $"{(isSelected ? "<b>" : "")}<color={color}>{text}</color>{(isSelected ? "</b>" : "")}";
    }

    private void OpenConsole()
    {
        _player.Suspend();
        ConsoleContainer.SetActive(true);
        ConsoleInput.Select();
        ConsoleInput.ActivateInputField();
        _isConsoleOpen = true;
    }

    private void CloseConsole()
    {
        ConsoleInput.text = "";
        ConsoleContainer.SetActive(false);
        _isConsoleOpen = false;
        _player.Resume();
    }

    private void WriteConsole(string text)
    {
        var lineObject = Instantiate(ConsoleBaseLine, ConsoleRect.transform);
        var textComponent = lineObject.GetComponent<TMP_Text>();
        textComponent.text = text;
    }

    private void HandleConsoleInput(string inputText)
    {
        ConsoleInput.text = "";
        WriteConsole(inputText);

        var parts = inputText.Split(' ');
        switch (parts[0])
        {
            case "teleport":
                Teleport(parts);
                break;
            default:
                break;
        }

        ConsoleScrollBar.value = 0;
        ConsoleInput.Select();
        ConsoleInput.ActivateInputField();
    }

    private void Teleport(string[] parts)
    {
        if (parts.Length != 3)
        {
            WriteConsole("Usage: teleport <row> <col>");
            return;
        }

        try
        {
            var row = int.Parse(parts[1]);
            var col = int.Parse(parts[2]);

            var cellObject = GameGenerator.Instance.GameCellGrid[row][col];
            var cell = cellObject.GetComponent<GameCell>();
            var maybeSpawner = cell.GetComponentInChildren<CarExitSpawner>();
            if (maybeSpawner != null)
            {
                maybeSpawner.SpawnCarExit();
            }

            var gameCell = cell.GetComponent<GameCell>();

            var entryPoint = gameCell.EntryPoint.gameObject.transform.position;

            _player.Teleport(entryPoint);

            gameCell.PlayerEntered(_player.gameObject);
        }
        catch (Exception e)
        {
            WriteConsole(e.Message);
        }
    }
}
