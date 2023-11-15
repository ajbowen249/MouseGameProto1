using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Neighbor1 : MonoBehaviour
{
    private NPC _npc;

    void Start()
    {
        _npc = gameObject.GetComponent<NPC>();
        _npc.DialogTree = new DialogNode
        {
            Message = "Well hey, there neighbor! Can you help me clear out this here snow?",
            Options = new List<DialogOption>
            {
                new DialogOption
                {
                    Text = "Okay!",
                    Tag = "positive",
                    Node = null,
                },
                new DialogOption
                {
                    Text = "No way!",
                    Tag = "",
                    Node = new DialogNode
                    {
                        Message = "What the heck, for real?!",
                        Options = new List<DialogOption>
                        {
                            new DialogOption
                            {
                                Text = "Fine",
                                Tag = "positive",
                                Node = null,
                            },
                            new DialogOption
                            {
                                Text = "Yeah, really!",
                                Tag = "negative",
                                Node = null,
                            },
                        },
                    },
                },
            },
        };
    }

    void OnInteraction(GameObject interactor)
    {
        _npc.InitiateDialog(interactor);
    }
}
