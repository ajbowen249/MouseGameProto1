using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MrsMouse : MonoBehaviour
{
    private NPC _npc;

    void Start()
    {
        var renderer = GetComponentInChildren<Renderer>();
        var block = new MaterialPropertyBlock();
        block.SetColor("_Tint", new Color(0.4f, 0f, 0.1f));
        renderer.SetPropertyBlock(block);

        _npc = gameObject.GetComponent<NPC>();
        _npc.DialogTree = new DialogNode
        {
            Message = "My love! We are out of Mouserella cheese! You must get more. Be careful, the city is dangerous!",
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

    void Update()
    {

    }

    void OnInteraction(GameObject interactor)
    {
        _npc.InitiateDialog(interactor);
    }

    void OnDialogChoice(DialogOption option)
    {
        if (option.Tag == "negative")
        {
            SceneManager.LoadScene("Fail");
        }
    }
}
