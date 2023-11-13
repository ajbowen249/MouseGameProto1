using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using StarterAssets;

public class FailScreen : MonoBehaviour
{
    private StarterAssetsInputs _input;

    void Start()
    {
        _input = gameObject.GetComponent<StarterAssetsInputs>();
    }

    void Update()
    {
        if (_input.interact)
        {
            _input.interact = false;
            SceneManager.LoadScene("Playground");
        }
    }
}
