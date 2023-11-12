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
        HUD.Instance.AddMessage("My love!");
        HUD.Instance.AddMessage("We are out of Mouserella cheese!");
        HUD.Instance.AddMessage("You must get more. Be careful, the city is dangerous!");
    }
}
