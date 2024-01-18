using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarExitSpawner : MonoBehaviour
{
    public GameObject CarExitPrefab;
    public bool EnableAutoContinue = false;

    Exit _exit;

    void Start()
    {
        _exit = gameObject.GetComponent<Exit>();
    }

    public GameObject SpawnCarExit()
    {
        var carObject = Instantiate(CarExitPrefab, transform.position, transform.rotation);

        var car = carObject.GetComponent<CarExit>();
        car.OnCreated(_exit);

        return carObject;
    }

    void OnInteraction(GameObject interactor)
    {
        _exit.AttemptExit(interactor);
    }

    public GridLocation? AutoContinuePoint(GameObject fromCell)
    {
        if (!EnableAutoContinue)
        {
            return null;
        }

        var cell = _exit.FromCell.GetComponent<GameCell>();
        var generatedPath = GameGenerator.Instance.WCF.GeneratedPath;
        var pathIndex = generatedPath.IndexOf(cell.Location);
        if (pathIndex >= 0 && pathIndex < generatedPath.Count - 1)
        {
            return generatedPath[pathIndex + 1];
        }

        return null;
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;

        var rotation = transform.rotation;
        rotation.eulerAngles = rotation.eulerAngles + new Vector3(-90, 0, 0);

        Gizmos.DrawMesh(
            CarExitPrefab.GetComponentInChildren<SkinnedMeshRenderer>().sharedMesh,
            transform.position + new Vector3(0f, 1.51f, 0f),
            rotation
        );
    }
}
