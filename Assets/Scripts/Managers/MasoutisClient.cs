using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Threading.Tasks;
using static EVISION.Camera.plugin.ApplicationView;
using System.IO;

namespace EVISION.Camera.plugin
{

    public class MasoutisClient : MonoBehaviour
    {
        #region properties
        
        // INTERFACES
        public IAnnotate OCRAnnotator;
        private ITextToVoice voiceSynthesizer;
        private IModelPrediction featureExtractor;
        private IModelPrediction svmClassifier;
        private IDeviceCamera[] cams;
        private IDeviceCamera currentCam;
        private HttpImageLoading httpLoader;

        // INSPECTOR PROPERTIES
        [SerializeField] private TextAsset DLModel;
        [SerializeField] private TextAsset LabelsFile;
        [SerializeField] private string image_name;
        [SerializeField] private bool isExternalCamera;
        [SerializeField] private bool logging;
        [SerializeField] private bool httpLoading;

        // PRIVATE PROPERTIES
        private Texture2D camTexture;
        private MasoutisItem masoutis_obj;
        private string annotationText;
        private SVMClassification svm_model;

        // PUBLIC PROPERTIES
        public static int category;
        public bool cameraConnected = false;
        public string AnnotationText { get; }
        public bool annotationProccessBusy { get; set; }
        public bool DB_LoadProccessBusy { get; set; }
        public string process_result;

        //Testing / Logging
        private string itemName;
        private float OCRtime;
        private float Majoritytime;
        private float classificationTime;

        #endregion

        #region MonoBehaviour Callbacks

        private void Awake()
        {
            // first external camera, then android cam, then IOS cam.
            cams = GetComponents<IDeviceCamera>();
            OCRAnnotator = GetComponent<IAnnotate>();
            voiceSynthesizer = GetComponent<ITextToVoice>();

            SetSVM();
            
        }

        // Start is called before the first frame update
        void Start()
        {
            annotationProccessBusy = false;
            StartCoroutine(LoadDatabase());
            DB_LoadProccessBusy = true;

            //set the external camera
            currentCam = cams[0];
            currentCam.ConnectCamera();

            httpLoader = GetComponent<HttpImageLoading>();

            EventCamManager.onNatCamConnect += ConnectNativeCamera;
        }

        // Update is called once per frame
        void Update()
        {
            if(currentCam != null && !isExternalCamera) { currentCam.Tick(); } 
            if (MajorityVoting.database_ready && DB_LoadProccessBusy == true) { DB_LoadProccessBusy = false; }
        }

        #endregion


        #region Initializers

        // TODO : remove dependencies with feautre extraction and svm classification.
        private void SetSVM()
        {
            //Get feautures from model
            featureExtractor = new TFFeatureExtraction("input_1", "block_15_project/convolution", 224, 224, 127.5f, 127.5f, DLModel, LabelsFile, 180, 0.01f);
            // set and load svm model
            svm_model = new SVMClassification();
            svm_model.SetModelParameters("SVM_Weights", "mu", "sigma");
            svmClassifier = svm_model;
        }

        //Load database
        public IEnumerator LoadDatabase()
        {
            annotationProccessBusy = true;
            MajorityVoting.LoadDatabaseFiles(MajorityVoting.masoutisFiles);
            while (MajorityVoting.database_ready != true)
            {
                yield return null;
            }
            annotationProccessBusy = false;
        }

        #endregion

        #region Event Listeners

        public void ConnectExternalCam()
        {
            Debug.Log("external cam connected");
            isExternalCamera = true;
            SetCameraConnectionStatus(true);
        }

        public void ConnectNativeCamera()
        {
            voiceSynthesizer.PerformSpeechFromText("Εξωτερική κάμερα απενεργοποιήθηκε... κάμερα κινητού ενεργοποιημένη");
            currentCam = cams[1];
            currentCam.ConnectCamera();
            isExternalCamera = false;
            SetCameraConnectionStatus(true);
            Debug.Log("native cam connected");
        }

        public void SetCameraConnectionStatus(bool value)
        {
            cameraConnected = value;
        }

        #endregion

        public void ScreenshotButtonListener()
        {
            if (annotationProccessBusy)
            {
                return;
            }
            if (!cameraConnected)
            {
                return;
            }

            StartCoroutine(ClassifyScreenshotAsync());
        }

        public void SetResultLogs()
        {
            if (logging)
            {
                var results = image_name + "| " + MajorityFinalText.text + "| " + MajorityValidText.text + "|" + "\n" + distanceString + "\n";
                SaveTXT(results);
            }
        }

        public IEnumerator ClassifyScreenshotAsync()
        {
            if (currentCam != null)
            {
                // lock the process so the user cannot access it.
                annotationProccessBusy = true;

                // Get camera texture.
                yield return StartCoroutine(GetScreenshot());
                Debug.Log("3");
                category = ClassifyCategory(camTexture);

                // product case
                if (category == (int)Enums.MasoutisCategories.product)
                {
                    yield return StartCoroutine(GetProductDescription());
                }
                else
                {
                    yield return StartCoroutine(GetTrailShelfDescription(category));
                }

                SetTimeText();

                annotationProccessBusy = false;
            }
        }

        private IEnumerator GetScreenshot()
        {
            if (Application.isEditor)
            {
                if (httpLoading)
                {
                    yield return StartCoroutine(httpLoader.LoadTextureFromImage());
                    camTexture = httpLoader.screenshotTex;
                    Debug.Log("3");
                }
                else if (isExternalCamera && !httpLoading)
                {
                    camTexture = currentCam.TakeScreenShot();
                }
                else
                {
                    camTexture = Resources.Load<Texture2D>("Products_UnitTests/" + image_name);
                }
            }
            else
            {
                if (httpLoading)
                {
                    yield return StartCoroutine(httpLoader.LoadTextureFromImage());
                    camTexture = httpLoader.screenshotTex;
                    Debug.Log("3");
                }
                else
                {
                    camTexture = currentCam.TakeScreenShot();
                }
            }
        }

        private int ClassifyCategory(Texture2D input_tex)
        {
            float startclass = Time.realtimeSinceStartup;
            
            // extract feautures from network.
            var featureVector = featureExtractor.FetchOutput<List<float>, Texture2D>(input_tex);

            // normalize feature vector
            var output_array = GenericUtils.ConvertToDouble(featureVector.ToArray()); // convert to double format.
            var norm_fv = svm_model.NormalizeElements(output_array, svm_model.muData, svm_model.sigmaData);
            List<double> norm_fv_list = new List<double>(norm_fv);
            
            // calculate propabilities
            var probs = svmClassifier.FetchOutput<List<float>, List<double>>(norm_fv_list);
            
            // get max propability class.
            float maxValue = probs.Max();
            int category_index = probs.IndexOf(maxValue);

            float endclass = Time.realtimeSinceStartup;
            classificationTime = CalculateTimeDifference(startclass, endclass);

            return category_index;
        }

        public IEnumerator GetProductDescription()
        {
            // output message to user.
            yield return StartCoroutine(voiceSynthesizer.PerformSpeechFromText("Αναγνώριση προϊόντος"));

            float startOCRt = Time.realtimeSinceStartup;

            //ocr annotation.
            yield return StartCoroutine(OCRAnnotator.PerformAnnotation(camTexture));
            annotationText = OCRAnnotator.GetAnnotationResults<string>();

            float endOCRt = Time.realtimeSinceStartup;
            OCRtime = CalculateTimeDifference(startOCRt, endOCRt);

            if (!string.IsNullOrEmpty(annotationText))
            {
                float startMajt = Time.realtimeSinceStartup;

                var wordsOCR = GenericUtils.SplitStringToList(annotationText);

                var product_desc = MajorityVoting.GetProductDesciption(wordsOCR);

                var product_formatted = FormatDescription(product_desc);

                float endMajt = Time.realtimeSinceStartup;
                Majoritytime = CalculateTimeDifference(startMajt, endMajt);
                SetResultLogs();
                yield return StartCoroutine(voiceSynthesizer.PerformSpeechFromText(product_formatted.ToLower()));
            }
        }

        private string FormatDescription(string product)
        {
            var edit = GenericUtils.SplitStringToList(product);
            edit = MajorityVoting.KeepElementsWithLen(edit, 4);
            var description = GenericUtils.ListToString(edit);
            return description;
        }

        public IEnumerator GetTrailShelfDescription(int category)
        {
            // output message to user.
            yield return StartCoroutine(voiceSynthesizer.PerformSpeechFromText("Παρακαλώ περιμένετε"));

            float startOCRt = Time.realtimeSinceStartup;

            //ocr annotation.
            yield return StartCoroutine(OCRAnnotator.PerformAnnotation(camTexture));
            annotationText = OCRAnnotator.GetAnnotationResults<string>();

            float endOCRt = Time.realtimeSinceStartup;

            OCRtime = CalculateTimeDifference(startOCRt, endOCRt);

            // Perform majority voting
            if (!string.IsNullOrEmpty(annotationText))
            {
                float startMajt = Time.realtimeSinceStartup;

                List<string> OCR_List = GenericUtils.SplitStringToList(annotationText);
                MajorityVoting majVoting = new MajorityVoting();
                yield return StartCoroutine(majVoting.PerformMajorityVoting(OCR_List));

                float endMajt = Time.realtimeSinceStartup;
                Majoritytime = CalculateTimeDifference(startMajt, endMajt);

                switch (category)
                {
                    case (int)Enums.MasoutisCategories.trail:
                        process_result = majVoting.masoutis_item.category_2;
                        yield return StartCoroutine(voiceSynthesizer.PerformSpeechFromText("διάδρομος, " + process_result));
                        break;
                    case (int)Enums.MasoutisCategories.shelf:
                        process_result = majVoting.masoutis_item.category_4;
                        yield return StartCoroutine(voiceSynthesizer.PerformSpeechFromText("ράφι, " + process_result));
                        break;
                    case (int)Enums.MasoutisCategories.other:
                        yield return StartCoroutine(voiceSynthesizer.PerformSpeechFromText("άλλο, " + "μη αναγνωρίσιμο"));
                        break;
                    default:
                        break;
                }
            }
            else
            {
                if (ApplicationView.MajorityFinalText != null)
                {
                    ApplicationView.MajorityFinalText.text = "Δεν αναγνωρίστηκαν διαθέσιμες λέξεις";
                }
                if (ApplicationView.MajorityValidText != null)
                {
                    ApplicationView.MajorityValidText.text = "κενό";
                }

                yield return StartCoroutine(voiceSynthesizer.PerformSpeechFromText("Δεν αναγνωρίστηκαν διαθέσιμες λέξεις"));
            }
        }

        public void SaveScreenShot()
        {
            Texture2D tex = currentCam.TakeScreenShot();
            currentCam.SaveScreenShot(tex);
        }

        public IEnumerator CLassify()
        {
            int category = 0;

            if (Application.isEditor)
            {
                Debug.Log("Editor");
                camTexture = Resources.Load<Texture2D>("Textures/Masoutis/" + image_name);
                category = ClassifyCategory(camTexture);
            }
            else
            {
                camTexture = currentCam.TakeScreenShot();
                category = ClassifyCategory(camTexture);
            }

            string cat = "κενό";
            if (category == 0) { cat = "διάδρομος"; }
            if (category == 1) { cat = "ράφι"; }
            if (category == 2) { cat = "προϊόν"; }
            if (category == 3) { cat = "άλλο"; }

            yield return StartCoroutine(voiceSynthesizer.PerformSpeechFromText("Κατηγορία: " + cat ));
        }

        public float CalculateTimeDifference(float start, float end)
        {
            float timeToCompleteSec = 0;

            timeToCompleteSec = (float)System.Math.Round(end - start,2);

            return timeToCompleteSec;
        }

        private void SetTimeText()
        {
            if (ApplicationView.TimeText != null)
            {
                ApplicationView.TimeText.text = "Full process costed : " + (OCRtime + Majoritytime + classificationTime).ToString() + "\nOCRtime: " + OCRtime.ToString()
                    + "\nMajorityTime: " + Majoritytime.ToString() + "\nClassificationTime: " + classificationTime.ToString();
            }
        }
    }
}