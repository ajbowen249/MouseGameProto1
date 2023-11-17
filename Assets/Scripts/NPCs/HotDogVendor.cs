using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HotDogVendor : MonoBehaviour
{
    public GameObject Crowd;

    private NPC _npc;
    private MouseController _player;

    private static ActionCost WaitForChef = new ActionCost
    {
        description = "You wait for the chef to cook up all 'dem dawgs",
        time = 1,
    };

    void OnPlayerEnter(GameObject player)
    {
        _player = player.GetComponent<MouseController>();
        _npc = gameObject.GetComponent<NPC>();

        _npc.DialogTree = new DialogNode
        {
            Message = "It's 'Dawgageddon! A 'Dawgopolypse! I gotta whip up dogs for all 'dese customas, pronto!",
            Options = new List<DialogOption>
            {
                new DialogOption
                {
                    Text = "Get cookin', then!",
                    Tag = "negative",
                    Cost = WaitForChef,
                },
            },
        };
    }

    void OnInteraction(GameObject interactor)
    {
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
    }
}
