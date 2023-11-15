using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SnowBank : MonoBehaviour
{
    void OnInteraction(GameObject interactor)
    {
        var player = interactor.GetComponent<MouseController>();
        if (player == null)
        {
            Debug.LogError("Interactor is not player");
            return;
        }

        int shovels;
        if (player.Inventory.TryGetValue("Shovel", out shovels))
        {
            player.ExpendResources(new ActionCost
            {
                description = "You clear the snow with your shovel",
                energy = 10,
                time = 5f / 60f,
            });
        }
        else
        {
            player.ExpendResources(new ActionCost
            {
                description = "You clear the snow with your bare hands",
                energy = 60,
                time = 2,
            });
        }

        Destroy(gameObject);
    }
}
