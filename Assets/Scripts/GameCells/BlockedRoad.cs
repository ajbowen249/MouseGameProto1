using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockedRoad : MonoBehaviour
{
    public GameObject SnowBank;

    void Start()
    {
        var cell = gameObject.GetComponent<GameCell>();
        cell.SetExitRequirement(attachPoint => {
            if (SnowBank == null)
            {
                return true;
            }

            HUD.Instance.AddMessage("The snow and your neighbor block your path.");

            return false;
        });
    }
}
