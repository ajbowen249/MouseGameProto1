using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HotDogStand : MonoBehaviour
{
    public GameObject Crowd;
    public GameObject MinigameCamera;
    public GameObject MinigamePrefab;

    void Start()
    {
        MinigameCamera.SetActive(false);
    }

    void OnPlayerEnter(GameObject player)
    {
        var cell = gameObject.GetComponent<GameCell>();
        cell.SetExitRequirement(() => {
            if (Crowd == null)
            {
                return true;
            }

            HUD.Instance.AddMessage("The crowd, ravenous for Hot Dogs, blocks your path.");

            return false;
        });
    }

    public void StartMinigame(MouseController player)
    {
        MinigameCamera.SetActive(true);
        MinigameManager.Instance.StartMinigame(MinigamePrefab, player);
    }

    public void OnMinigameEnded()
    {
        MinigameCamera.SetActive(false);
    }
}
