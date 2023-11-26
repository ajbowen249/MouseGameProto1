using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using StarterAssets;

public class HotDogCatcher : MonoBehaviour
{
    public GameObject HotDogPrefab;

    public StarterAssetsInputs Input;

    private bool _hasDog = false;
    private bool _hasKetchup = false;
    private bool _hasMustard = false;

    private GameObject _hotDogInstance;

    private float _moveSpeed = 3f;
    private float _maxDistance = 5f;

    private Vector3 _startLocation;

    void Start()
    {
        _startLocation = transform.position;

        var rotation = Quaternion.Euler(0f, 0f, 0f);
        rotation.eulerAngles = transform.rotation.eulerAngles + new Vector3(0f, -90, 0f);

        _hotDogInstance = Instantiate(
            HotDogPrefab,
            transform.position,
            rotation,
            transform
        );

        _hotDogInstance.transform.localScale = new Vector3(0.2f, 0.2f, 0.2f);

        UpdateGraphic();
    }

    void Update()
    {
        if (Input == null)
        {
            return;
        }

        if (Input.move.x != 0)
        {
            var moveSpeed = Input.move.x * _moveSpeed * Time.deltaTime * -1f;
            transform.position += new Vector3(moveSpeed, 0f, 0f);
        }
    }

    void UpdateGraphic()
    {
        _hotDogInstance.transform.Find("Dog").gameObject.SetActive(_hasDog);
        _hotDogInstance.transform.Find("Toppings/Ketchup").gameObject.SetActive(_hasKetchup);
        _hotDogInstance.transform.Find("Toppings/Mustard").gameObject.SetActive(_hasMustard);
    }
}
