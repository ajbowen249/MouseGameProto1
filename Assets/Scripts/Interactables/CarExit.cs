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
        if (!FromCell.GetComponent<GameCell>().CanExit())
        {
            return;
        }

        Destroy(gameObject.GetComponentInChildren<InteractionVolume>());

        if (ExitPath != null)
        {
            _driver = interactor.GetComponent<MouseController>();
            _driver.AttachTo(gameObject, DriverSeat.transform);

            PathFollower.SendObjectAlongPath(gameObject, ExitPath, () => {
                _driver.DetachFrom(ExitPoint.transform);
            });
        }
    }
}
