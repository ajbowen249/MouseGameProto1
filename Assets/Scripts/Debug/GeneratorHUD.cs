using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GeneratorHUD : MonoBehaviour
{
    public TMP_Text PhaseText;
    public GameObject GeneratorObject;
    public TMP_InputField SeedInput;

    private GameGenerator _generator;
    private bool _isSteppingToEnd = false;
    private bool _granularCollapse;

    void Start()
    {
        _generator = GeneratorObject.GetComponent<GameGenerator>();
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

    public void OnClickStep()
    {
        ResetIfDone();
        UpdateSeed();
        _generator.Step();
    }

    public void OnClickStepToEnd()
    {
        ResetIfDone();
        UpdateSeed();
        _isSteppingToEnd = true;
    }

    public void OnClickPauseStepToEnd()
    {
        _isSteppingToEnd = false;
    }

    public void OnClickGenerateInstant()
    {
        ResetIfDone();
        _generator.GenerateComplete();
    }

    public void OnGranularCollapseChanged(Toggle toggle)
    {
        _generator.WCF.GranularCollapse = toggle.isOn;
        _granularCollapse = toggle.isOn;
    }

    public void OnCLickReset()
    {
        _generator.ResetSelf();
        UpdateSeed();
        _generator.WCF.GranularCollapse = _granularCollapse;
    }

    public void OnClickInDecSeed(bool isIncrement)
    {
        var seedValue = int.Parse(SeedInput.text);
        seedValue += isIncrement ? 1 : -1;
        SeedInput.text = $"{seedValue}";
        UpdateSeed();
    }

    private void UpdateSeed()
    {
        RandomInstances.SetSeed(RandomInstances.Names.Generator, int.Parse(SeedInput.text));
    }

    private void ResetIfDone()
    {
        if (_generator.WCF.Phase == GenerationPhase.DONE)
        {
            OnCLickReset();
        }
    }
}
