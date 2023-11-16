using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarExit : MonoBehaviour
{
    public GameObject DriverSeat;
    public GameObject ExitPoint;

    public GameObject FromCell;
    public GameObject ToCell;
    public GameObject ExitPath;

    private MouseController _driver;

    void OnInteraction(GameObject interactor)
    {
        var fromGameCell = FromCell.GetComponent<GameCell>();
        if (!fromGameCell.CanExit())
        {
            return;
        }

        fromGameCell.FadeOutAndDie();

        Destroy(gameObject.GetComponentInChildren<InteractionVolume>());

        if (ExitPath != null)
        {
            _driver = interactor.GetComponent<MouseController>();
            _driver.AttachTo(gameObject, DriverSeat.transform);

            PathFollower.SendObjectAlongPath(gameObject, ExitPath, () => {
                OnEndedPath();
            });
        }
    }

    void OnEndedPath()
    {
        _driver.DetachFrom(ExitPoint.transform);
        if (ToCell != null)
        {
            var maybeSpawner = ToCell.GetComponentInChildren<CarExitSpawner>();
            if (maybeSpawner != null)
            {
                Destroy(gameObject);
                maybeSpawner.SpawnCarExit();
            }

            var toGameCell = ToCell.GetComponent<GameCell>();
            toGameCell.PlayerEntered(_driver.gameObject);
        }
    }
}
