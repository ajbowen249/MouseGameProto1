using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameCell : MonoBehaviour
{
    public GameObject EntryPoint;

    public delegate bool ExitRequirement();
    private ExitRequirement _exitRequirement;

    public void SetExitRequirement(ExitRequirement requirement)
    {
        _exitRequirement = requirement;
    }

    public bool CanExit()
    {
        return _exitRequirement == null ? true : _exitRequirement();
    }

    public void PlayerEntered(GameObject player)
    {
        BroadcastMessage("OnPlayerEnter", player, SendMessageOptions.DontRequireReceiver);
    }

    public void FadeOutAndDie()
    {
        StartCoroutine(FadeOutRoutine());
    }

    IEnumerator FadeOutRoutine()
    {
        foreach (var collider in gameObject.GetComponentsInChildren<Collider>())
        {
            collider.enabled = false;
        }

        var renderers = gameObject.GetComponentsInChildren<MeshRenderer>();
        float steps = 10.0f;

        for (float i = 0.0f; i < steps; i++)
        {
            var progress = (steps - (i / 2)) / steps;
            var color = new Color(progress, progress, progress, progress);

            foreach (var renderer in renderers)
            {
                renderer.material.color = color;
            }

            yield return new WaitForSeconds(0.05f);
        }

        Destroy(gameObject);
    }
}
