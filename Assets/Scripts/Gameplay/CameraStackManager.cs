using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class CameraStackManager : MonoBehaviour
{
    public static CameraStackManager Instance;

    public GameObject NativeCameraObject;

    private UniversalAdditionalCameraData _cameraData;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }

        Instance = this;
    }

    void Start()
    {
        _cameraData = NativeCameraObject.GetComponent<Camera>().GetUniversalAdditionalCameraData();
    }

    public void AddCamera(Camera camera)
    {
        _cameraData.cameraStack.Add(camera);
    }

    public void RemoveCamera(Camera camera)
    {
        _cameraData.cameraStack.Remove(camera);
    }
}
