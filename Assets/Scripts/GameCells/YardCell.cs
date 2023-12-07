using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class YardCell : MonoBehaviour
{
    public GameObject Car;
    public GameObject SnowBank;

    void Start()
    {
        var cell = gameObject.GetComponent<GameCell>();
        cell.SetExitRequirement(attachPoint => {
            if (SnowBank == null)
            {
                return true;
            }

            HUD.Instance.AddMessage("The snow blocks your path.");

            return false;
        });
    }

    void OnPlayerEnter(GameObject player)
    {
        var spawner = GetComponentInChildren<CarExitSpawner>();
        spawner.SpawnCarExit();
    }
}
