using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Neighbor1 : MonoBehaviour
{
    public GameObject PlowTarget;
    public GameObject ReturnTarget1;
    public GameObject ReturnTarget2;

    public GameObject SnowBank;

    private NPC _npc;

    private MouseController _player;
    private MouseController _controller;

    void OnInteraction(GameObject interactor)
    {
        _controller = GetComponent<MouseController>();
        _player = interactor.GetComponent<MouseController>();
        _npc = gameObject.GetComponent<NPC>();

        var clearCost = GetSnowClearCost(PlayerHasShovel());

        _npc.DialogTree = new DialogNode
        {
            Message = "Well hey, there neighbor! Can you help me clear out this here snow?",
            Options = new List<DialogOption>
            {
                new DialogOption
                {
                    Text = "Sure thing!",
                    Tag = "positive",
                    Cost = clearCost,
                },
                new DialogOption
                {
                    Text = "No way!",
                    Node = new DialogNode
                    {
                        Message = "Are you sure? We could make light work of it together!",
                        Options = new List<DialogOption>
                        {
                            new DialogOption
                            {
                                Text = "Fine",
                                Tag = "positive",
                                Cost = clearCost,
                            },
                            new DialogOption
                            {
                                Text = "Yes, really.",
                                Node = new DialogNode
                                {
                                    Message = "Well, okay. I guess just sit tight while I handle this.",
                                    Options = new List<DialogOption>
                                    {
                                        new DialogOption
                                        {
                                            Text = "Ok.",
                                            Tag = "negative",
                                        },
                                    }
                                }
                            },
                        },
                    },
                },
            },
        };

        _npc.InitiateDialog(interactor);
    }

    void OnDialogChoice(DialogOption option)
    {
        if (string.IsNullOrEmpty(option.Tag))
        {
            return;
        }

        _controller.WalkTo(new WalkTarget
        {
            position = PlowTarget.transform.position,
            distance = 0.1f,
            callback = () =>
            {
                _npc.Avatar.Emote(MouseEmotes.NondescriptAction1, () =>
                {
                    if (tag == "positive")
                    {
                        _player.ExpendResources(GetSnowClearCost(PlayerHasShovel()));
                    }
                    else
                    {
                        _player.ExpendResources(new ActionCost
                        {
                            description = "You watch your neighbor clear the snow alone",
                            time = 2,
                        });
                    }

                    Destroy(SnowBank);

                    _controller.WalkTo(new WalkTarget
                    {
                        position = ReturnTarget1.transform.position,
                        distance = 0.1f,
                        callback = () =>
                        {
                            _controller.WalkTo(new WalkTarget
                            {
                                position = ReturnTarget2.transform.position,
                                distance = 0.1f,
                                callback = () => { },
                            });
                        },
                    });
                });
            },
        });
    }

    private ActionCost GetSnowClearCost(bool hasShovel)
    {
        return hasShovel ?
            new ActionCost { description = "You help clear the snow with your shovel", energy = 5, time = 2.5f / 60f } :
            new ActionCost { description = "You help clear the snow with your bare hands", energy = 30, time = 1 };
    }

    private bool PlayerHasShovel()
    {
        int shovels;
        _player.Inventory.TryGetValue("Shovel", out shovels);
        return shovels > 0;
    }
}
