using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using StarterAssets;

public class DialogController : MonoBehaviour
{
    private PlayerMouse _player;
    private MouseController _mouseController;
    private StarterAssetsInputs _input;
    private NPC _talkingTo;
    private DialogNode _dialog;
    private int _dialogIndex;

    void Start()
    {
        _player = gameObject.GetComponent<PlayerMouse>();
        _mouseController = gameObject.GetComponent<MouseController>();
        _input = gameObject.GetComponent<StarterAssetsInputs>();
    }

    public void OnStartedDialog(GameObject talkingTo)
    {
        _talkingTo = talkingTo.GetComponent<NPC>();

        _dialog = _talkingTo.DialogTree;
        _dialogIndex = 0;

        HUD.WithInstance(hud => hud.SetDialog(_dialog, _dialogIndex));

        if (_talkingTo.TalkCamera != null)
        {
            _talkingTo.TalkCamera.SetActive(true);
        }

        // Movement overlaps dialog input
        _input.dialogDown = false;
        _input.dialogUp = false;
    }

    public void OnEndedDialog(GameObject talkingTo)
    {
        HUD.WithInstance(hud => hud.SetDialog(null, 0));

        if (_talkingTo.TalkCamera)
        {
            _talkingTo.TalkCamera.SetActive(false);
        }

        _talkingTo = null;
    }

    void Update()
    {
        if (_player.State != ControlState.DIALOG)
        {
            return;
        }

        if (_input.interact)
        {
            _input.interact = false;

            var option = _dialog.Options[_dialogIndex];

            if (!_mouseController.CanSelectDialogOption(option))
            {
                HUD.WithInstance(hud => hud.AddMessage("Missing requirements"));
                return;
            }

            if (option.Tag != "" && option.Tag != null)
            {
                _talkingTo.BroadcastMessage("OnDialogChoice", option, SendMessageOptions.DontRequireReceiver);
            }

            if (option.Node != null)
            {
                _dialog = option.Node;
                _dialogIndex = 0;
                HUD.WithInstance(hud => hud.SetDialog(_dialog, _dialogIndex));
            }
            else
            {
                var talkingTo = _talkingTo;
                BroadcastMessage("OnEndedDialog", _talkingTo.gameObject, SendMessageOptions.DontRequireReceiver);
                talkingTo?.BroadcastMessage("OnEndedDialog", gameObject, SendMessageOptions.DontRequireReceiver);
            }
        }

        var dialogDirection = 0;

        if (_input.dialogDown)
        {
            _input.dialogDown = false;
            dialogDirection = 1;
        }

        if (_input.dialogUp)
        {
            _input.dialogUp = false;
            dialogDirection = -1;
        }

        if (dialogDirection != 0)
        {
            _dialogIndex = (_dialogIndex + dialogDirection) % _dialog.Options.Count;
            if (_dialogIndex < 0)
            {
                _dialogIndex = _dialog.Options.Count - 1;
            }

            HUD.WithInstance(hud => hud.SetDialog(_dialog, _dialogIndex));
        }
    }
}
