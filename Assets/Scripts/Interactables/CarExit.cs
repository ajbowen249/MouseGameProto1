using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarExit : MonoBehaviour
{
    public GameObject FromCell;
    public GameObject ToCell;

    void OnInteraction(GameObject interactor)
    {
        Debug.Log("interacted with car");
    }
}
