using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using StarterAssets;

public class HotDogCatcher : MonoBehaviour
{
    public GameObject HotDogPrefab;

    private StarterAssetsInputs _input;

    private bool _hasDog = false;
    private bool _hasKetchup = false;
    private bool _hasMustard = false;

    private GameObject _hotDogInstance;

    void Start()
    {
        _input = gameObject.GetComponent<StarterAssetsInputs>();

        var rotation = Quaternion.Euler(0f, 0f, 0f);
        rotation.eulerAngles = transform.rotation.eulerAngles + new Vector3(0f, -90, 0f);

        _hotDogInstance = Instantiate(
            HotDogPrefab,
            transform.position,
            rotation,
            transform
        );

        UpdateGraphic();
    }

    void UpdateGraphic()
    {
        _hotDogInstance.transform.Find("Dog").gameObject.SetActive(_hasDog);
        _hotDogInstance.transform.Find("Toppings/Ketchup").gameObject.SetActive(_hasKetchup);
        _hotDogInstance.transform.Find("Toppings/Mustard").gameObject.SetActive(_hasMustard);
    }
}
