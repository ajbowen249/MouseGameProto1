using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using StarterAssets;

public class MakeHotDogs : MonoBehaviour
{
    public GameObject CatcherObject;

    public void OnStartMinigame(GameObject player)
    {
        CatcherObject.GetComponent<HotDogCatcher>().Input =
            player.GetComponent<StarterAssetsInputs>();
    }
}
