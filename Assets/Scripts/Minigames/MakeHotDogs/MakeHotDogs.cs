using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using StarterAssets;
using TMPro;

public class MakeHotDogs : MonoBehaviour
{
    public GameObject CatcherObject;
    public TMP_Text DogsRemainingText;
    public TMP_Text TimeTakenText;

    private int _dogsRemaining = 4;
    private float _startTime;
    private MouseController _player;

    void Start()
    {
        _startTime = Time.time;
    }

    public void OnMadeHotDog()
    {
        _dogsRemaining--;
    }

    void Update()
    {
        DogsRemainingText.text = $"{_dogsRemaining}";
        var timeSpent = (Time.time - _startTime) / 100f;
        TimeTakenText.text = $"{TimeUtils.FormatHours(timeSpent)}";

        if (_dogsRemaining <= 0)
        {
            var cost = new ActionCost
            {
                description = "You help whip up tha dawgs.",
                energy = 5,
                time = timeSpent,
            };

            _player.ExpendResources(cost);

            MinigameManager.Instance.EndMinigame();
        }
    }

    public void OnStartMinigame(GameObject player)
    {
        _player = player.GetComponent<MouseController>();
        var catcher = CatcherObject.GetComponent<HotDogCatcher>();

        catcher.Input = player.GetComponent<StarterAssetsInputs>();
        catcher.Minigame = this;
    }
}
