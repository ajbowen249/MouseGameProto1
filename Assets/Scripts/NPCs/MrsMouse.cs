using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MrsMouse : MonoBehaviour
{
    private NPC _npc;

    void Start()
    {
        _npc = gameObject.GetComponent<NPC>();
    }

    void Update()
    {

    }

    void OnInteraction(GameObject interactor)
    {
        _npc.InitiateDialog(interactor);
        // StartCoroutine(ShowMessages());
    }

    IEnumerator ShowMessages()
    {
        HUD.Instance.AddMessage("My love!");
        yield return new WaitForSeconds(0.5f);
        HUD.Instance.AddMessage("We are out of Mouserella cheese!");
        yield return new WaitForSeconds(0.5f);
        HUD.Instance.AddMessage("You must get more. Be careful, the city is dangerous!");
    }
}
