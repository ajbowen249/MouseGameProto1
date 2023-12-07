using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarExitSpawner : MonoBehaviour
{
    public GameObject CarExitPrefab;

    public GameObject SpawnCarExit()
    {
        var carObject = Instantiate(CarExitPrefab, transform.position, transform.rotation);

        var car = carObject.GetComponent<CarExit>();
        car.OnCreated(gameObject.GetComponent<Exit>());

        return carObject;
    }

    void OnInteraction(GameObject interactor)
    {
        gameObject.GetComponent<Exit>().AttemptExit(interactor);
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
