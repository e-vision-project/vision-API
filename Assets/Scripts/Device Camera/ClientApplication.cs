﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class ClientApplication : MonoBehaviour
{
    private IDeviceCamera cam;
    private IAnnotate annotator;
    private ITextToVoice voiceSync;
    private Texture2D camTexture;
    private string annotationText { get; set; }
    private bool annotationProccessBusy = false;

    public delegate void OnAnnotationCompleted();
    public static OnAnnotationCompleted onAnnotationCompleted;

    private void Awake()
    {
        cam = GetComponent<IDeviceCamera>();
        annotator = GetComponent<IAnnotate>();
        voiceSync = GetComponent<ITextToVoice>();
    }

    // Start is called before the first frame update
    void Start()
    {
        annotationProccessBusy = false;
        //cam.SetCamera(DeviceCamera.Cameras.Back);
    }

    // Update is called once per frame
    void Update()
    {
        cam.Tick();
    }

    public void TakeScreenShot()
    {
        if (annotationProccessBusy) { Debug.Log("Process busy"); return; }
        StartCoroutine(TakeScreenshotSingleThreaded());
    }

    public IEnumerator TakeScreenshotSingleThreaded()
    {
        // lock the process so the user cannot access it.
        annotationProccessBusy = true;
        // Get and rotate camera texture
        camTexture = cam.TakeScreenShot();
        camTexture = Utils.RotateTexture(camTexture, true);
        // wait until the annotation process returns
        yield return StartCoroutine(annotator.PerformAnnotation(camTexture));
        try
        {
            annotationText = annotator.GetAnnotationText();
        }
        catch (System.Exception)
        {
            Debug.LogError("annotationText in ClientApplication dropped an exception error");
        }
        Debug.Log("Annotation from Client : " + annotationText[0]);
        //unlock process
        annotationProccessBusy = false;
    }

    public void SaveScreenShot()
    {
        //cam.SaveScreenShot(camTexture);
        voiceSync.PerformSpeechFromText();
    }
}
