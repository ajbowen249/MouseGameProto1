using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarExit : MonoBehaviour
{
    public GameObject DriverSeat;
    public GameObject ExitPoint;

    Exit _exit;

    public void OnCreated(Exit exit)
    {
        _exit = exit;
        _exit.SetExitHandler((interactor, attachPoint) => {
            var toCell = attachPoint.toCell.GetComponent<GameCell>();
            var neighborCar = toCell.GetComponentInChildren<CarExitSpawner>();

            // TODO: Give cells a way to guide the car out/in. For now, this just goes straight
            var pathName = $"generated_path_{exit.FromCell.name}_to_{toCell.gameObject.name}";
            var exitPath = new GameObject(pathName);

            var pathOffset = new Vector3(0f, 0.0756f, 0f);

            var startNode = new GameObject($"{pathName}_0");
            startNode.transform.position = transform.position + pathOffset;
            startNode.transform.SetParent(exitPath.transform);
            startNode.AddComponent<PathNode>();

            var endNode = new GameObject($"{pathName}_1");
            endNode.transform.position = neighborCar.transform.position + pathOffset;
            endNode.transform.SetParent(exitPath.transform);
            endNode.AddComponent<PathNode>();

            Destroy(gameObject.GetComponentInChildren<InteractionVolume>());

            var driver = interactor.GetComponent<MouseController>();
            driver.AttachTo(gameObject, DriverSeat.transform);

            PathFollower.SendObjectAlongPath(gameObject, exitPath, () => {
                var maybeSpawner = toCell.GetComponentInChildren<CarExitSpawner>();
                if (maybeSpawner == null)
                {
                    Debug.LogError("Destination cell has no car spawner.");
                }

                toCell.PlayerEntered(driver.gameObject);

                var autoContinue = maybeSpawner.ShouldAutoContinue(exit.FromCell);

                var detachLocation = autoContinue ? DriverSeat.transform : ExitPoint.transform;
                driver.DetachFrom(detachLocation);
                Destroy(gameObject);

                var nextExitObject = maybeSpawner.SpawnCarExit();
                var nextExit = nextExitObject.GetComponent<CarExit>();

                if (autoContinue)
                {
                    nextExit.AutoContinue(driver.gameObject, exit.FromCell);
                }
            });
        });
    }

    public void AutoContinue(GameObject driver, GameObject fromCell)
    {
        var cell = _exit.FromCell.GetComponent<GameCell>();
        _exit.AttemptExit(driver, fromCell);
    }
}
