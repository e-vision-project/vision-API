using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;


public class EventCamManager : MonoBehaviour
{
    public static EventCamManager current;

    private void Awake()
    {
        current = this;
    }

    public delegate void OnExternalCameraError();
    public event OnExternalCameraError onExternalCamError;

    public delegate void OnNativeCameraConnect(DeviceCamera.Cameras camType);
    public event OnNativeCameraConnect onNatCamConnect;
}
