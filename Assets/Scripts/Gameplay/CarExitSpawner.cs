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

    void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;

        var rotation = transform.rotation;
        rotation.eulerAngles = rotation.eulerAngles + new Vector3(-90, 0, 0);

        Gizmos.DrawMesh(
            CarExitPrefab.GetComponentInChildren<MeshFilter>().sharedMesh,
            transform.position + new Vector3(0f, 1.51f, 0f),
            rotation
        );
    }
}
