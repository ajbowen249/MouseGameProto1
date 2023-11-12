using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractionVolume : MonoBehaviour
{
    [Tooltip("Text displayed to the player")]
    public string Prompt;

    [Tooltip("Receiver of OnInteract")]
    public GameObject Interactable;

    public void Interact(GameObject interactor) {
        if (Interactable != null) {
            Interactable.SendMessage("OnInteract", interactor);
        }
    }
}
