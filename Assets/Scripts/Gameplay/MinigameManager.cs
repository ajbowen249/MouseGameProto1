using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MinigameManager : MonoBehaviour
{
    public static MinigameManager Instance;

    private Minigame _activeMinigame;
    private PlayerMouse _activePlayer;
    private GameObject _activeMinigameOwner;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }

        Instance = this;
    }

    public void StartMinigame(GameObject minigamePrefab, PlayerMouse player, GameObject minigameOwner)
    {
        EndMinigame();
        _activeMinigameOwner = minigameOwner;
        var minigameObject = Instantiate(minigamePrefab, transform.position, transform.rotation);
        _activeMinigame = minigameObject.GetComponent<Minigame>();
        minigameObject.BroadcastMessage("OnStartMinigame", player.gameObject, SendMessageOptions.DontRequireReceiver);
        CameraStackManager.Instance.AddCamera(_activeMinigame.Camera.GetComponent<Camera>());
        _activePlayer = player;
        _activePlayer.Suspend();
    }

    public void EndMinigame()
    {
        if (_activeMinigame != null)
        {
            CameraStackManager.Instance.RemoveCamera(_activeMinigame.Camera.GetComponent<Camera>());
            Destroy(_activeMinigame.gameObject);
            _activeMinigame = null;
        }

        if (_activePlayer != null)
        {
            _activePlayer.Resume();
            _activePlayer = null;
        }

        if (_activeMinigameOwner != null)
        {
            _activeMinigameOwner.BroadcastMessage("OnMinigameEnded", null, SendMessageOptions.DontRequireReceiver);
            _activeMinigameOwner = null;
        }
    }
}
