using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Door : MonoBehaviour
{
    public GameObject FromCell;
    public GameObject ToCell;

    void OnInteraction(GameObject interactor)
    {
        if (ToCell == null)
        {
            Debug.LogWarning("Interacted with empty door");
            return;
        }

        var gameCell = ToCell.GetComponent<GameCell>();
        var entryPoint = gameCell.EntryPoint.gameObject.transform.position;

        var mouseController = interactor.GetComponent<MouseController>();
        mouseController.Teleport(entryPoint);
        gameCell.PlayerEntered(mouseController.gameObject);

        if (FromCell != null)
        {
            StartCoroutine(FadeOutFrom());
        }
    }

    IEnumerator FadeOutFrom()
    {
        foreach (var collider in FromCell.GetComponentsInChildren<Collider>())
        {
            collider.enabled = false;
        }

        var renderers = FromCell.GetComponentsInChildren<MeshRenderer>();
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

        Destroy(FromCell);
    }
}
