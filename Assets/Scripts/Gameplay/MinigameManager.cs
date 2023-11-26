using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MinigameManager : MonoBehaviour
{
    public static MinigameManager Instance;

    private Minigame _activeMinigame;
    private MouseController _activePlayer;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }

        Instance = this;
    }

    public void StartMinigame(GameObject minigamePrefab, MouseController player)
    {
        EndMinigame();
        var minigameObject = Instantiate(minigamePrefab, transform.position, transform.rotation);
        _activeMinigame = minigameObject.GetComponent<Minigame>();
        CameraStackManager.Instance.AddCamera(_activeMinigame.Camera.GetComponent<Camera>());
        _activePlayer = player;
        _activePlayer.Suspend();
    }

    public void EndMinigame()
    {
        if (_activeMinigame != null)
        {
            CameraStackManager.Instance.RemoveCamera(_activeMinigame.Camera.GetComponent<Camera>());
            Destroy(_activeMinigame);
            _activeMinigame = null;
        }

        if (_activePlayer != null)
        {
            _activePlayer.Resume();
            _activePlayer = null;
        }
    }
}
