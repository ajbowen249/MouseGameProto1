using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HotDogStand : MonoBehaviour
{
    public GameObject Crowd;

    void Start()
    {
        var cell = gameObject.GetComponent<GameCell>();
        cell.SetExitRequirement(() => {
            if (Crowd == null)
            {
                return true;
            }

            HUD.Instance.AddMessage("The crowd, ravenous for Hot Dogs, blocks your path.");

            return false;
        });
    }
}
