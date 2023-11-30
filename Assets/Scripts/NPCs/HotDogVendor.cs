using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HotDogVendor : MonoBehaviour
{
    public GameObject Crowd;
    public GameObject GameCellObject;

    private NPC _npc;
    private MouseController _player;

    private bool _startMinigame = false;

    private static ActionCost WaitForChef = new ActionCost
    {
        description = "You wait for the chef to cook up all 'dem dawgs",
        time = 1,
    };

    void OnInteraction(GameObject interactor)
    {
        _player = interactor.GetComponent<MouseController>();
        _npc = gameObject.GetComponent<NPC>();

        _npc.DialogTree = new DialogNode
        {
            Message = "It's 'Dawgageddon! A 'Dawgopolypse! I gotta whip up dogs for all 'dese customas, pronto!",
            Options = new List<DialogOption>
            {
                new DialogOption
                {
                    Text = "I wanna help!",
                    Tag = "positive",
                },
                new DialogOption
                {
                    Text = "Get cookin', then!",
                    Tag = "negative",
                    Cost = WaitForChef,
                },
            },
        };

        _npc.InitiateDialog(interactor);
    }

    void OnDialogChoice(string tag)
    {
        if (tag == "negative")
        {
            _player.ExpendResources(WaitForChef);
            Destroy(Crowd);

            _npc.DialogTree = new DialogNode
            {
                Message = "Thanks fa nothin'!",
                Options = new List<DialogOption>
                {
                    new DialogOption
                    {
                        Text = "...",
                    },
                },
            };
        }
        else if (tag == "positive")
        {
            _startMinigame = true;
        }
    }

    void OnEndedDialog(GameObject player)
    {
        if (_startMinigame)
        {
            GameCellObject.GetComponent<HotDogStand>().StartMinigame(_player);
        }
    }
}