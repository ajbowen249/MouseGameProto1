using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GeneratorHUD : MonoBehaviour
{
    public TMP_Text PhaseText;
    public GameObject GeneratorObject;
    public TMP_InputField SeedInput;

    public Button StepButton;
    public Button StepToEndButton;
    public Button PauseStepButton;
    public Button GenerateButton;
    public Button ResetButton;
    public Button IncrementButton;
    public Button DecrementButton;

    public Toggle GranularCollapseToggle;

    public Button MainMenuButton;
    public Button ExitButton;

    private GameGenerator _generator;
    private bool _isSteppingToEnd = false;

    void Start()
    {
        _generator = GeneratorObject.GetComponent<GameGenerator>();

        StepButton.onClick.AddListener(() =>
        {
            ResetIfDone();
            UpdateSeed();
            _generator.Step();
        });

        StepToEndButton.onClick.AddListener(() =>
        {
            ResetIfDone();
            UpdateSeed();
            _isSteppingToEnd = true;
        });

        PauseStepButton.onClick.AddListener(() =>
        {
            _isSteppingToEnd = false;
        });

        GenerateButton.onClick.AddListener(() =>
        {
            ResetIfDone();
            _generator.GenerateComplete();
        });

        ResetButton.onClick.AddListener(() =>
        {
            ResetGenerator();
        });

        Action<bool> incDec = isIncrement =>
        {
            var seedValue = int.Parse(SeedInput.text);
            seedValue += isIncrement ? 1 : -1;
            SeedInput.text = $"{seedValue}";
            UpdateSeed();
        };

        IncrementButton.onClick.AddListener(() => incDec(true));
        DecrementButton.onClick.AddListener(() => incDec(false));

        GranularCollapseToggle.onValueChanged.AddListener(value =>
        {
            _generator.WCF.GranularCollapse = GranularCollapseToggle.isOn;
        });

        MainMenuButton.onClick.AddListener(() =>
        {
            SceneManager.LoadScene("MainMenu");
        });

        ExitButton.onClick.AddListener(() =>
        {
            Application.Quit();
        });
    }

    void Update()
    {
        PhaseText.text = $"{_generator.WCF.Phase}";

        if (_isSteppingToEnd)
        {
            _generator.Step();
            _isSteppingToEnd = !_generator.IsGenerationComplete;
        }

        if (SeedInput.text == "<seed>")
        {
            SeedInput.text = $"{RandomInstances.GetInstance(RandomInstances.Names.Generator).Seed}";
        }
    }

    private void ResetGenerator()
    {
        _generator.ResetSelf();
        UpdateSeed();
        _generator.WCF.GranularCollapse = GranularCollapseToggle.isOn;
    }

    private void UpdateSeed()
    {
        RandomInstances.SetSeed(RandomInstances.Names.Generator, int.Parse(SeedInput.text));
    }

    private void ResetIfDone()
    {
        if (_generator.WCF.Phase == GenerationPhase.DONE)
        {
            ResetGenerator();
        }
    }
}
