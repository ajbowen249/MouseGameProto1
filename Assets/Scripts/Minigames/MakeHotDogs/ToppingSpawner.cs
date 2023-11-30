using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ToppingSpawner : MonoBehaviour
{
    public GameObject FallingToppingPrefab;

    public float SpawnRate = 1;
    public float Range = 5;

    private float _lastSpawn = 0;

    void Start()
    {
        _lastSpawn = Time.time;
    }

    // Update is called once per frame
    void Update()
    {
        if (_lastSpawn == 0)
        {
            return;
        }

        if (Time.time >= _lastSpawn + SpawnRate)
        {
            Instantiate(
                FallingToppingPrefab,
                transform.position + new Vector3(Random.Range(-1 * Range, Range), 0f, 0f),
                transform.rotation,
                transform
            );

            _lastSpawn = Time.time;
        }
    }
}
