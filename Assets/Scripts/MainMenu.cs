using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    public Button StartGameButton;
    public Button GenerationSandboxButton;
    public Button ExitButton;

    private void Start()
    {
        Cursor.lockState = CursorLockMode.None;

        StartGameButton.onClick.AddListener(() => {
            SceneManager.LoadScene("Main");
        });

        GenerationSandboxButton.onClick.AddListener(() => {
            SceneManager.LoadScene("GenerationSandbox");
        });

        ExitButton.onClick.AddListener(() =>
        {
            Application.Quit();
        });
    }
}
