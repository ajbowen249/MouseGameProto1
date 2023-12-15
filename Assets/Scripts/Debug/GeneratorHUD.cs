using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GeneratorHUD : MonoBehaviour
{
    public TMP_Text PhaseText;
    public GameObject GeneratorObject;
    public float StepRateSeconds = 0.01f;

    private GameGenerator _generator;
    private bool _isSteppingToEnd = false;
    private float _lastStep = 0f;

    void Start()
    {
        _generator = GeneratorObject.GetComponent<GameGenerator>();
    }

    void Update()
    {
        PhaseText.text = $"{_generator.WCF.Phase}";

        if (_isSteppingToEnd && Time.time - _lastStep >= StepRateSeconds)
        {
            _lastStep = Time.time;
            _generator.Step();
            _isSteppingToEnd = !_generator.IsGenerationComplete;
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

    public void OnClickGenerateInstant()
    {
        _generator.GenerateComplete();
    }
}
