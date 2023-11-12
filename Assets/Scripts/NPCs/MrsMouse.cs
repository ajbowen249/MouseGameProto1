using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MrsMouse : MonoBehaviour
{
    void Start()
    {

    }

    void Update()
    {

    }

    void OnInteract(GameObject interactor)
    {
        StartCoroutine(ShowMessages());
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
