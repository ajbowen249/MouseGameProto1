using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using StarterAssets;

public class HotDogCatcher : MonoBehaviour
{
    public StarterAssetsInputs Input;
    public MakeHotDogs Minigame;

    private bool _hasDog = false;
    private bool _hasKetchup = false;
    private bool _hasMustard = false;

    private GameObject _hotDogInstance;

    private float _moveSpeed = 3f;

    private Vector3 _startLocation;

    void Start()
    {
        _hotDogInstance = transform.Find("HotDogGraphic").gameObject;
        _startLocation = transform.position;
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

    private void Clear()
    {
        _hasDog = false;
        _hasKetchup = false;
        _hasMustard = false;
        UpdateGraphic();
    }

    private void OnBadDog(string message)
    {
        Clear();
        HUD.WithInstance(hud => hud.AddMessage(message));
    }

    private void OnDogComplete()
    {
        Clear();
        Minigame?.OnMadeHotDog();
    }

    private void OnTriggerEnter(Collider other)
    {
        var topping = other.GetComponent<FallingTopping>();
        if (topping == null)
        {
            return;
        }

        ConsumeTopping(topping);
        Destroy(topping.gameObject);
        UpdateGraphic();

        if (_hasDog && _hasKetchup && _hasMustard)
        {
            OnDogComplete();
        }
    }

    private void ConsumeTopping(FallingTopping topping)
    {
        if (topping.IsDog)
        {
            if (_hasDog)
            {
                OnBadDog("Duplicate dog");
                return;
            }

            _hasDog = true;
        }

        if (topping.IsKetchup || topping.IsMustard)
        {
            if (!_hasDog)
            {
                OnBadDog("Can't add topping before dog");
                return;
            }

            if (topping.IsKetchup)
            {
                if (_hasKetchup)
                {
                    OnBadDog("Extra ketchup");
                    return;
                }

                _hasKetchup = true;
            }

            if (topping.IsMustard)
            {
                if (_hasMustard)
                {
                    OnBadDog("Extra mustard");
                    return;
                }

                _hasMustard = true;
            }
        }
    }
}
