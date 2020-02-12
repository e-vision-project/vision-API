using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;


public class EventCamManager : MonoBehaviour
{
    public delegate void OnExternalCameraError();
    public static OnExternalCameraError onExternalCamError;

    public delegate void OnNativeCameraConnect();
    public static OnNativeCameraConnect onNatCamConnect;

    public delegate void OnHTTPDownload();
    public static OnHTTPDownload onHTTPDownload;

    public delegate void OnAnnotationFailed();
    public static OnAnnotationFailed onAnnotationFailed;

    public delegate void OnProcessScreenshotEnded();
    public static OnProcessScreenshotEnded onProcessEnded;
}
