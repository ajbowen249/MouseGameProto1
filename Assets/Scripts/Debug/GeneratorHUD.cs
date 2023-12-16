using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GeneratorHUD : MonoBehaviour
{
    public TMP_Text PhaseText;
    public GameObject GeneratorObject;
    public TMP_InputField SeedInput;

    private GameGenerator _generator;
    private bool _isSteppingToEnd = false;

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
            SeedInput.text = $"{_generator.WCF.RandomSeed}";
        }
    }

    public void OnClickStep()
    {
        _generator.Step();
    }

    public void OnClickStepToEnd()
    {
        _isSteppingToEnd = true;
    }

    public void OnClickPauseStepToEnd()
    {
        _isSteppingToEnd = false;
    }

    public void OnClickGenerateInstant()
    {
        _generator.GenerateComplete();
    }

    public void OnClickSetSeed()
    {
        _generator.WCF.RandomSeed = int.Parse(SeedInput.text);
    }
}
