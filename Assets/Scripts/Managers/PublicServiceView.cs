using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PublicServiceView : CameraClientView
{
    public static Text resultText;
    public static Text TimeText;
    public PublicServiceManager clientApp;

    private void Start()
    {
        resultText = GameObject.FindGameObjectWithTag("PS_TEXT").GetComponent<Text>();
        TimeText = GameObject.FindGameObjectWithTag("TIME").GetComponent<Text>();
        clientApp = gameObject.GetComponent<PublicServiceManager>();
        annotatingImage.SetActive(false);
    }

    private void Update()
    {
        if (clientApp.annotationProccessBusy)
        {
            annotatingImage.SetActive(true);
            ScreenshotButton.SetActive(false);
            cancelButton.SetActive(true);
            tapImage.SetActive(false);
        }
        else if (!clientApp.annotationProccessBusy)
        {
            annotatingImage.SetActive(false);
            ScreenshotButton.SetActive(true);
            cancelButton.SetActive(false);
            tapImage.SetActive(true);
        }
    }
}
