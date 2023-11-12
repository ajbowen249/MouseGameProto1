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

        player.ExpendEnergy(60);

        HUD.Instance.AddMessage("You expend 60 energy clearing the snow with your bare hands.");

        Destroy(gameObject);
    }
}
