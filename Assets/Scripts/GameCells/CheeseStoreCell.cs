using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheeseStoreCell : MonoBehaviour
{
    public GameObject Hideables;

    void OnPlayerEnter(GameObject player)
    {
        var cell = gameObject.GetComponent<GameCell>();
        Hideables.SetActive(false);
    }

    void OnPlayerExit(GameObject player)
    {
        Hideables.SetActive(true);
    }
}
