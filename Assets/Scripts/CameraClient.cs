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
        /// <summary>  
        /// Event Listener που διαχειρίζεται την αποτυχία επικοινωνίας με το τρέχον annotation service
        /// του πεδίου της διεπαφής IAnnotate.  
        /// </summary>  
        public void AnnotationFailedHandler()
        {
            StopAllCoroutines();
            StartCoroutine(voiceSynthesizer.PerformSpeechFromText("Η σύνδεση στο δίκτυο είναι απενεργοποιημένη. Παρακαλώ, ενεργοποιήστε την σύνδεση στο διαδύκτιο."));
            annotationProccessBusy = false;
        }
        /// <summary>  
        /// Μέθοδος που αναλαμβάνει την ακύρωση όλων των κορουτινών και μεθόδων που είναι ενεργές.  
        /// </summary>  
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
        /// <summary>  
        /// Μέθοδος που αναλαμβάνει την ενεργοποίηση και σύνδεση της ενσωματωμένης κάμερας της συσκευής.
        /// Επίσης απενεργοποιεί όλες τις διεπάφές που χρησιμοποιούνται από την κλάση του πεδίου httpLoader.
        /// </summary>  
        public void ConnectNativeCamera()
        {
            voiceSynthesizer.PerformSpeechFromText("Εξωτερική κάμερα απενεργοποιήθηκε... κάμερα κινητού ενεργοποιημένη");
            currentCam.ConnectCamera();
            GameObject.FindGameObjectWithTag("DISPLAY_IMAGE_EXTERNAL").SetActive(false);
            annotationProccessBusy = false;
        }

        #endregion

        /// <summary>  
        /// Μέθοδος που αναλαμβάνει την λήψη στιγμιοτύπου από την τρέχουσα ενεργοποιημένη κάμερα. Εάν η κάμερα είναι η ενσωματωμένη
        /// της συσκευής η διεπαφή IDeviceCamera χρησιμοπιείται. Αλλιώς γίνετε χρήση του αντικειμένου της κλάσης HttpImageLoading 
        /// και γίνετε λήψη από την προς χρήση εξωτερική κάμερα.  
        /// </summary>   
        /// <returns>Αντικείμενο τύπου IEnumerator</returns>
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
        /// <summary>  
        /// Κορουτίνα που αναλαμβάνει τις απαραίτητες κλήσεις στις αντίστοιχες μεθόδους, με σκοπό την λήψη
        /// στιγμιοτύπου, κατηγοριοποίηση εικόνας και αποστολή στο προς χρήση annotation service (e.g google cloud vision). 
        /// </summary>  
        /// <returns>Αντικείμενο τύπου IEnumerator</returns>
        public abstract IEnumerator ProcessScreenshotAsync();
        /// <summary>  
        /// Μέθοδος που αποθηκεύει το στιγμιότυπο της κάμερας. Η διαδικασία της αποθήκευσης γίνετε μέσω της 
        /// υλοποιημένης μεθόδου της κλάσης που χρησιμοποιεί την διεπαφή IDeviceCamera.
        /// </summary>  
        /// <param name="camTexture">Αντικείμενο τύπου Texture2D</param>  
        /// <returns>Αντικείμενο τύπου IEnumerator</returns>
        public abstract void SaveScreenshot(Texture2D camTexture);

        public abstract void SetResultLogs();
    }

}