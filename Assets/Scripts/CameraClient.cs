using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EVISION.Camera.plugin
{
    public abstract class CameraClient : MonoBehaviour
    {
        // INTERFACES
        protected IAnnotate annotator;
        protected ITextToVoice voiceSynthesizer;
        protected IDeviceCamera[] cams;
        protected IDeviceCamera currentCam;
        protected HttpImageLoading httpLoader;

        // PRIVATE PROPERTIES
        protected Texture2D camTexture;
        protected float captureTime;

        // PUBLIC PROPERTIES
        public string AnnotationText { get; }
        public bool annotationProccessBusy { get; set; }
        public string imageName;


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
            voiceSynthesizer.PerformSpeechFromText("Εξωτερική κάμερα απενεργοποιήθηκε... κάμερα κινητού ενεργοποιημένη");
            currentCam.ConnectCamera();
            GameObject.FindGameObjectWithTag("DISPLAY_IMAGE_EXTERNAL").SetActive(false);
            annotationProccessBusy = false;
        }

        #endregion


        protected IEnumerator GetScreenshot()
        {
            float startCapture = Time.realtimeSinceStartup;
            if (Application.isEditor)
            {
                if (httpLoader.cameraConnected)
                {
                    yield return StartCoroutine(httpLoader.LoadTextureFromImage());
                    while (!httpLoader.textureLoaded)
                    {
                        yield return null;
                    }
                    camTexture = httpLoader.screenshotTex;
                    yield return StartCoroutine(httpLoader.SendRemovePhotoRequest(httpLoader.imageUrl));
                }
                else
                {
                    camTexture = Resources.Load<Texture2D>("Products_UnitTests/" + imageName);
                }
            }
            else
            {
                if (httpLoader.cameraConnected)
                {
                    yield return StartCoroutine(httpLoader.LoadTextureFromImage());
                    while (!httpLoader.textureLoaded)
                    {
                        yield return null;
                    }
                    camTexture = httpLoader.screenshotTex;
                    Debug.Log("final resolution: " + camTexture.width + "," + camTexture.height);
                    yield return StartCoroutine(httpLoader.SendRemovePhotoRequest(httpLoader.imageUrl));
                }
                else
                {
                    camTexture = currentCam.TakeScreenShot();
                }
            }
            float endCapture = Time.realtimeSinceStartup;
            captureTime = GenericUtils.CalculateTimeDifference(startCapture, endCapture);
        }

        public abstract IEnumerator ProcessScreenshotAsync();
        public abstract void SaveScreenshot(Texture2D camTexture);
        public abstract void SetResultLogs();
    }

}