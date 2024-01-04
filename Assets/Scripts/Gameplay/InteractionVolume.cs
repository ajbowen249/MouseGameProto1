using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractionVolume : MonoBehaviour
{
    [Tooltip("Text displayed to the player")]
    public string Prompt;

    [Tooltip("Receiver of OnInteraction")]
    public GameObject Interactable;

    public string EmoteName;

    public int? EmoteHash { get; private set; } = null;

    private void Start()
    {
        if (!string.IsNullOrEmpty(EmoteName))
        {
            var emoteHash = Animator.StringToHash(EmoteName);
            if (!MouseEmotes.AllEmotes.Contains(emoteHash))
            {
                Debug.LogError($"Unknown emote \"{EmoteName}\"!");
            }
            else
            {
                EmoteHash = emoteHash;
            }
        }
    }

    public void Interact(GameObject interactor)
    {
        if (Interactable != null)
        {
            Interactable.SendMessage("OnInteraction", interactor, SendMessageOptions.DontRequireReceiver);
            HUD.Instance.ClearInteractionPrompt();
        }
    }
}
