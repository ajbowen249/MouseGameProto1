using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SnowBank : MonoBehaviour
{
    void OnInteract(GameObject interactor)
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
            player.ExpendEnergy(10);
            HUD.Instance.AddMessage("You expend 10 energy clearing the snow with your shovel.");
        }
        else
        {
            player.ExpendEnergy(60);
            HUD.Instance.AddMessage("You expend 60 energy clearing the snow with your bare hands.");
        }

        Destroy(gameObject);
    }
}
