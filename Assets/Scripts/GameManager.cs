using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    /// <summary>
    /// The player object this game instance is following. Will be null in the scene generator sandbox.
    /// Note: Only use this to specifically get the active player object. Event methods that are passed the interacting
    ///       object allow possibility for non-player objects to interact with eachother at some point.
    /// </summary>
    public GameObject FollowingPlayer;

    public static GameManager Instance {  get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
}
