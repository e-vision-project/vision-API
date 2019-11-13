using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Threading.Tasks;

namespace EVISION.Camera.plugin
{

    public class MasoutisClient : MonoBehaviour
    {
        #region properties
        
        // INTERFACES
        public IDeviceCamera cam;
        public IAnnotate OCRAnnotator;
        private ITextToVoice voiceSynthesizer;
        private IModelPrediction featureExtractor;
        private IModelPrediction svmClassifier;

        // INSPECTOR PROPERTIES
        [SerializeField] private TextAsset DLModel;
        [SerializeField] private TextAsset LabelsFile;
        [SerializeField] private string image_name;

        // PRIVATE PROPERTIES
        private Texture2D camTexture;
        public DeviceCamera.Cameras cameraDevice;
        private MasoutisItem masoutis_obj;
        private string annotationText;
        private float OCRtime;
        private float Majoritytime;
        private float classificationTime;
        private SVMClassification svm_model;

        // PUBLIC PROPERTIES
        public static int category;
        public string AnnotationText { get; }
        public bool annotationProccessBusy { get; set; }
        public bool DB_LoadProccessBusy { get; set; }
        public string process_result;

        #endregion


        private void Awake()
        {
            cam = GetComponent<IDeviceCamera>();
            OCRAnnotator = GetComponent<IAnnotate>();
            voiceSynthesizer = GetComponent<ITextToVoice>();
            
            //Get feautures from model
            featureExtractor = new TFFeatureExtraction("input_1", "block_15_project/convolution", 224, 224, 127.5f, 127.5f, DLModel, LabelsFile, 180, 0.01f);
            
            // set and load svm model
            svm_model = new SVMClassification();
            svm_model.SetModelParameters("SVM_Weights", "mu", "sigma");
            svmClassifier = svm_model;
        }

        // Start is called before the first frame update
        void Start()
        {
            annotationProccessBusy = false;
            StartCoroutine(LoadDatabase());
            DB_LoadProccessBusy = true;
            cam.SetCamera(cameraDevice);
        }

        //Load database
        public IEnumerator LoadDatabase()
        {
            annotationProccessBusy = true;
            MajorityVoting.LoadDatabaseFiles(MajorityVoting.masoutisFiles);
            while(MajorityVoting.database_ready != true)
            {
                yield return null;
            }
            annotationProccessBusy = false;
        }

        // Update is called once per frame
        void Update()
        {
            cam.Tick();
            if (MajorityVoting.database_ready && DB_LoadProccessBusy == true) { DB_LoadProccessBusy = false; }
        }


        public void ScreenshotButtonListener()
        {
            if (annotationProccessBusy)
            {
                return;
            }

            StartCoroutine(ClassifyScreenshotAsync());

        }

        public IEnumerator MockTesting(string screenshot, int category)
        {
            if(!annotationProccessBusy)
            {
                annotationProccessBusy = true;

                camTexture = Resources.Load<Texture2D>(screenshot);

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

        public IEnumerator ClassifyScreenshotAsync()
        {
            // lock the process so the user cannot access it.
            annotationProccessBusy = true;

            // Get camera texture.
            if (Application.isEditor)
            {
                camTexture = Resources.Load<Texture2D>("Textures/Masoutis/" + image_name);
            }
            else
            {
                camTexture = cam.TakeScreenShot();
            }

            // get class based on SVM inference.
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
            Texture2D tex = cam.TakeScreenShot();
            cam.SaveScreenShot(tex);
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
                camTexture = cam.TakeScreenShot();
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