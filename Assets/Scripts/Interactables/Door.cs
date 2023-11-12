using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Door : MonoBehaviour
{
    public GameObject ToCell;

    void OnInteract(GameObject interactor)
    {
        if (ToCell == null)
        {
            Debug.LogWarning("Interacted with empty door");
            return;
        }
    }
}
