using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarExitSpawner : MonoBehaviour
{
    public GameObject CarExitPrefab;

    public GameObject FromCell;
    public GameObject ToCell;
    public GameObject ExitPath;

    public GameObject SpawnCarExit()
    {
        var carObject = Instantiate(CarExitPrefab, transform.position, transform.rotation);
        var car = carObject.GetComponent<CarExit>();

        car.FromCell = FromCell;
        car.ToCell = ToCell;
        car.ExitPath = ExitPath;

        return carObject;
    }
}
