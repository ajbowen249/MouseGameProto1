using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FallingTopping : MonoBehaviour
{
    public float LifeTime = 20;

    public bool IsDog { get; private set; }
    public bool IsKetchup { get; private set; }
    public bool IsMustard { get; private set; }

    private float _startTime = 0;

    void Start()
    {
        _startTime = Time.time;

        var topping = Random.Range(0, 3);
        IsDog = topping == 0;
        IsKetchup = topping == 1;
        IsMustard = topping == 2;

        var graphic = transform.Find("HotDogGraphic").gameObject;

        graphic.transform.Find("Bun").gameObject.SetActive(false);
        graphic.transform.Find("Dog").gameObject.SetActive(IsDog);
        graphic.transform.Find("Toppings/Ketchup").gameObject.SetActive(IsKetchup);
        graphic.transform.Find("Toppings/Mustard").gameObject.SetActive(IsMustard);
    }

    void Update()
    {
        transform.position += new Vector3(0f, -1f * Time.deltaTime, 0f);

        if (Time.time >= _startTime + LifeTime)
        {
            Destroy(gameObject);
        }
    }
}
