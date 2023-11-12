using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using StarterAssets;

public class NPC : MonoBehaviour
{
    public GameObject TalkCamera;

    public GameObject _talkingTo;
    private StarterAssetsInputs _input;

    private bool _isInDialog = false;

    void Start()
    {
        TalkCamera.SetActive(false);
    }

    public void InitiateDialog(GameObject talkingTo)
    {
        _talkingTo = talkingTo;
        _input = _talkingTo.GetComponent<MouseController>().GetInputController();
        _talkingTo.SendMessage("OnStartedDialog", gameObject);
        TalkCamera.SetActive(true);
        _isInDialog = true;
    }

    void Update()
    {
        if (_input == null)
        {
            return;
        }

        if (_isInDialog)
        {
            if (_input.interact)
            {
                _input.interact = false;
                _input = null;
                _talkingTo.SendMessage("OnEndedDialog", gameObject);
                TalkCamera.SetActive(false);
                _isInDialog = false;
            }
        }
    }
}
