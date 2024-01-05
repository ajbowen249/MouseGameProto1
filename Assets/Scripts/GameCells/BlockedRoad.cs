using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockedRoad : MonoBehaviour
{
    public GameObject SnowBank;

    private GameCell _cell;

    void Start()
    {
        _cell = gameObject.GetComponent<GameCell>();
        _cell.SetExitRequirement(attachPoint =>
        {
            if (SnowBank == null)
            {
                return true;
            }

            HUD.Instance.AddMessage("The snow and your neighbor block your path.");

            return false;
        });
    }

    private void Update()
    {
        if (SnowBank == null && !_cell.IsComplete)
        {
            _cell.OnCellComplete();
        }
    }
}
