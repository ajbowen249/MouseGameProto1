using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class YardCell : MonoBehaviour
{
    public GameObject Car;
    public GameObject SnowBank;

    void OnPlayerEnter(GameObject player)
    {
        var spawner = GetComponentInChildren<CarExitSpawner>();
        Car = spawner.SpawnCarExit();

        var carExit = Car.GetComponent<CarExit>();
        carExit.SetExitRequirement(() => {
            if (SnowBank == null)
            {
                return true;
            }

            HUD.Instance.AddMessage("The snow blocks your path.");

            return false;
        });
    }
}
