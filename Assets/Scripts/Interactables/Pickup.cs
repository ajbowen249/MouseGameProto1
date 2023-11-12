using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pickup : MonoBehaviour
{
    public string Name = "object";
    public int Quantity = 1;

    void OnInteraction(GameObject interactor)
    {
        var player = interactor.GetComponent<MouseController>();
        if (player == null)
        {
            Debug.LogError("Interactor is not player");
            return;
        }

        player.AddInventoryItem(Name, Quantity);
        Destroy(gameObject);
    }
}
