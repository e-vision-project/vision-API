using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace EVISION.Camera.plugin
{
    public abstract class CameraClient : MonoBehaviour
    {
        // INTERFACES
        protected IAnnotate annotator;
        protected ITextToVoice voiceSynthesizer;
        [SerializeField] protected IDeviceCamera currentCam;
        protected HttpImageLoading httpLoader;
        [SerializeField] protected bool verboseMode;

        // PRIVATE PROPERTIES
        public static Texture2D camTexture;
        protected float captureTime;

        // PUBLIC PROPERTIES
        public bool externalCamera;
        public RawImage displayImage;
        public string AnnotationText { get; }
        public bool annotationProccessBusy { get; set; }
        public string imageName;
        public static string text_result = "";

        #region Event Listeners

        public void AnnotationFailedHandler()
        {
            StopAllCoroutines();
            StartCoroutine(voiceSynthesizer.PerformSpeechFromText("Η σύνδεση στο δίκτυο είναι απενεργοποιημένη. Παρακαλώ, ενεργοποιήστε την σύνδεση στο διαδύκτιο."));
            annotationProccessBusy = false;
        }

        public virtual void CancelButton()
        {
            if (!annotationProccessBusy)
            {
                return;
            }
            StopAllCoroutines();
            StopCoroutine(ProcessScreenshotAsync());
            camTexture = null;
            annotationProccessBusy = false;
        }
 
        public void ConnectNativeCamera()
        {
            Debug.Log("connecting native cam");
            currentCam.ConnectCamera();
            annotationProccessBusy = false;
        }

        #endregion

        protected IEnumerator GetScreenshot()
        {
            float startCapture = Time.realtimeSinceStartup;
            if (Application.isEditor)
            {
                if (externalCamera)
                {
                    yield return StartCoroutine(httpLoader.LoadTextureFromImage());
                    while (!httpLoader.textureLoaded)
                    {
                        yield return null;
                    }
                    SetDisplayImageAndUnloadDownloadedTexture();
                    yield return StartCoroutine(httpLoader.SendRemovePhotoRequest(httpLoader.imageUrl));
                }
                else
                {
                    camTexture = Resources.Load<Texture2D>("Products_UnitTests/" + imageName);
                    SetDisplayImageAndUnloadDownloadedTexture();
                }
            }
            else
            {
                if (externalCamera)
                {
                    yield return StartCoroutine(httpLoader.LoadTextureFromImage());
                    while (!httpLoader.textureLoaded)
                    {
                        yield return null;
                    }
                    SetDisplayImageAndUnloadDownloadedTexture();
                    Debug.Log("capture");
                    yield return StartCoroutine(httpLoader.SendRemovePhotoRequest(httpLoader.imageUrl));
                }
                else
                {
                    camTexture = currentCam.TakeScreenShot();
                    SetDisplayImageAndUnloadDownloadedTexture();
                    Handheld.Vibrate();
                }
            }
            float endCapture = Time.realtimeSinceStartup;
            captureTime = GenericUtils.CalculateTimeDifference(startCapture, endCapture);
        }

        private void SetDisplayImageAndUnloadDownloadedTexture()
        {
            displayImage.texture = camTexture;
        }

        public void SetVerbosity(bool value)
        {
            verboseMode = value;
        }

        public abstract IEnumerator ProcessScreenshotAsync();
 
        public abstract void SaveScreenshot(Texture2D camTexture);

        public abstract void SetResultLogs();
    }

}