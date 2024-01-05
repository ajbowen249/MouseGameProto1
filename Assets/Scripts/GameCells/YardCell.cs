using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class YardCell : MonoBehaviour
{
    public GameObject SnowBank;

    private GameCell _cell;

    void Start()
    {
        _cell = gameObject.GetComponent<GameCell>();
        _cell.SetExitRequirement(attachPoint => {
            if (SnowBank == null)
            {
                return true;
            }

            HUD.Instance.AddMessage("The snow blocks your path.");

            return false;
        });
    }

    void OnPlayerEnter(GameObject player)
    {
        var spawner = GetComponentInChildren<CarExitSpawner>();
        spawner.SpawnCarExit();
    }

    private void Update()
    {
        if (SnowBank == null && !_cell.IsComplete)
        {
            _cell.OnCellComplete();
        }
    }
}
