using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HUD : MonoBehaviour
{
    public GameObject Canvas;
    public TMP_Text Prompt;

    public static HUD Instance { get; private set; }

    void Start()
    {
        Instance = this;

        if (Canvas == null || Prompt == null)
        {
            Debug.LogError("One or more elements is null.");
        }

        ClearInteractionPrompt();
    }

    public void SetInteractionPrompt(string prompt)
    {
        Prompt.text = prompt;
    }

    public void ClearInteractionPrompt()
    {
        Prompt.text = "";
    }
}
