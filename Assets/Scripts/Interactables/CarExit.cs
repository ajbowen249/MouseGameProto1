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

    public delegate bool ExitRequirement();
    private ExitRequirement _exitRequirement;

    private MouseController _driver;

    void OnInteraction(GameObject interactor)
    {
        var canExit = _exitRequirement == null ? true : _exitRequirement();
        if (!canExit)
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

    public void SetExitRequirement(ExitRequirement requirement)
    {
        _exitRequirement = requirement;
    }
}
