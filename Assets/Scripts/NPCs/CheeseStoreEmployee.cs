using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CheeseStoreEmployee : MonoBehaviour
{
    private NPC _npc;
    private MouseController _player;

    void OnInteraction(GameObject interactor)
    {
        _player = interactor.GetComponent<MouseController>();

        var items = new List<(string, float)>()
        {
            ("Asiago", 12.75f),
            ("Brie", 12.75f),
            ("Colby-Jack", 45.98f),
            ("Mouserella", 49.99f),
            ("Muenster", 74.95f),
            ("Swiss", 299.99f),
        };

        if (_npc == null)
        {
            _npc = gameObject.GetComponent<NPC>();

            _npc.DialogTree = new DialogNode
            {
                Message = "Welcome to Cheese-Mart!",
                Options = new List<DialogOption>
                {
                    new DialogOption
                    {
                        Text = "Buy",
                        Node = new DialogNode
                        {
                            Message = "<i>select item</i>",
                            Options = items.Select(item => new DialogOption
                            {
                                Text = item.Item1,
                                Tag = $"buy",
                                Cost = new ActionCost
                                {
                                    description = $"Bought {item.Item1}",
                                    money = item.Item2,
                                },
                            }).Append(new DialogOption
                            {
                                Text = "<i>cancel</i>",
                            }).ToList(),
                        },
                    },
                    new DialogOption
                    {
                        Text = "Exit",
                    },
                },
            };
        }

        _npc.InitiateDialog(interactor);
    }

    void OnDialogChoice(DialogOption option)
    {
        if (option.Tag == "buy")
        {
            _player.ExpendResources((ActionCost)option.Cost);
            _player.AddInventoryItem(option.Text, 1);
        }
    }
}
