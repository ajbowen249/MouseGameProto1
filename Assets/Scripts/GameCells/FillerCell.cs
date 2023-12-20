using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FillerCell : MonoBehaviour
{
    public List<GameObject> FillerGraphicPrefabs;

    void Start()
    {
        if (FillerGraphicPrefabs.Count == 0)
        {
            return;
        }

        var graphic = RandomUtil.RandomElement(FillerGraphicPrefabs, RandomInstances.Names.Generator);
        Instantiate(graphic, transform);
    }
}
