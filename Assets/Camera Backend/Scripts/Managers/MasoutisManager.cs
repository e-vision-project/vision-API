using System.Collections;
using UnityEngine;
using static EVISION.Camera.plugin.MasoutisView;
using System.Collections.Generic;
using System.Linq;
using static EVISION.Camera.plugin.GenericUtils;
using static LogManager;
using System;

namespace EVISION.Camera.plugin
{
    
    public class MasoutisManager : CameraClient
    {
        // INTERFACES
        private IModelPrediction featureExtractor;
        private IModelPrediction svmClassifier;

        // INSPECTOR PROPERTIES
        [SerializeField] private TextAsset DLModel;
        [SerializeField] private TextAsset LabelsFile;
        [SerializeField] private bool logging;

        // PRIVATE PROPERTIES
        private MasoutisItem masoutis_obj;
        private SVMClassification svm_model;
        private string annotationText;
        private MajorityVoting majVoting;

        // PUBLIC PROPERTIES
        public static int category;
        public bool DB_LoadProccessBusy;

        // LOGGING
        private string itemName;
        private float OCRtime;
        private float Majoritytime;
        private float classificationTime;
        public static Texture2D saveTex;

        public void ScreenshotButtonListener()
        {
            if (annotationProccessBusy)
            {
                return;
            }
            StartCoroutine(ProcessScreenshotAsync());
        }

        #region CameraClient Callbacks

        public override IEnumerator ProcessScreenshotAsync()
        {
            // lock the process so the user cannot access it.
            annotationProccessBusy = true;

            // Get camera texture.
            yield return StartCoroutine(GetScreenshot());

            //category = ClassifyCategory(camTexture);
            category = 1;

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
            SetResultLogs();
            //Used to set the main UI
            EventCamManager.onProcessEnded?.Invoke();
            annotationProccessBusy = false;
            Resources.UnloadUnusedAssets();
        }

        public override void SaveScreenshot(Texture2D camTexture)
        {
            throw new System.NotImplementedException();
        }

        #endregion

        #region MonoBehaviour Callbacks

        private void Awake()
        {
            currentCam = GetComponent<IDeviceCamera>();
            annotator = GetComponent<IAnnotate>();
            voiceSynthesizer = GetComponent<ITextToVoice>();
            httpLoader = GetComponent<HttpImageLoading>();
            majVoting = new MajorityVoting();
            SetSVM();
        }

        // Start is called before the first frame update
        void Start()
        {
            StartCoroutine(LoadDatabase());
            DB_LoadProccessBusy = true;
            annotationProccessBusy = false;

            //native camera
            if (!externalCamera && currentCam != null)
            {
                Debug.Log("Natice camera exists");
                ConnectNativeCamera();
            }
        }

        // Update is called once per frame
        void Update()
        {
            if (currentCam != null && !externalCamera) { currentCam.Tick(); }
            if (MajorityVoting.database_ready && DB_LoadProccessBusy == true) { DB_LoadProccessBusy = false; }
        }

        #endregion

        #region Initializers
  
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

        #region Logging

        public override void SetResultLogs()
        {
            
            if (logging)
            {
                var sum = captureTime + OCRtime + classificationTime + Majoritytime;
                string fileName = string.Format("masoutis_{0}", System.DateTime.Now.ToString());
                fileName = fileName.Replace(" ", "_");
                fileName = fileName.Replace(":", "_");
                fileName = fileName.Replace("/", "_");
                string imageName = GetResultLogs("Image Name", fileName);
                string response =  GetResponseTime(captureTime.ToString(), OCRtime.ToString(), classificationTime.ToString(), 
                    Majoritytime.ToString(), sum.ToString());
                string ocrResults = GetResultLogs("OCR Results", OCRWordsText);
                string classificationResults = GetResultLogs("Classification", category.ToString());
                string validWordsResults = GetResultLogs("Valid Words", MajorityValidText);
                string finalResult = GetResultLogs("ReturnedResult", majorityFinal);
                string logText = imageName + "\n" + response + "\n" + ocrResults + "\n" + classificationResults + "\n" + validWordsResults + "\n" + 
                    finalResult + "\n\n";
                SaveResultLogs(logText);
                if (currentCam != null)
                {
                    currentCam.SaveScreenShot(camTexture, fileName + ".png");
                }
            }
        }

        private void SetTimeText()
        {
            if (TimeText != null)
            {
                TimeText = "Full process costed : " + (OCRtime + Majoritytime + classificationTime).ToString() + "\nOCRtime: " + OCRtime.ToString()
                    + "\nMajorityTime: " + Majoritytime.ToString() + "\nClassificationTime: " + classificationTime.ToString();
            }
        }

        #endregion

        /// <summary>  
        /// This methods aims to classify a 2D texture based on the svm model that has been initialized 
        /// in the start method of this class.  
        /// </summary>
        /// <param name="input_Tex"> Texture2D obj</param>  
        /// <returns>Integer type</returns>
        private int ClassifyCategory(Texture2D input_tex)
        {
            float startclass = Time.realtimeSinceStartup;

            // extract feautures from network.
            var featureVector = featureExtractor.FetchOutput<List<float>, Texture2D>(input_tex);

            // normalize feature vector
            var output_array = ConvertToDouble(featureVector.ToArray()); // convert to double format.
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

        public IEnumerator GetProductDescriptionFromDb()
        {
            // output message to user.

            float startOCRt = Time.realtimeSinceStartup;

            //ocr annotation.
            yield return StartCoroutine(annotator.PerformAnnotation(camTexture));
            annotationText = annotator.GetAnnotationResults<string>();

            float endOCRt = Time.realtimeSinceStartup;
            OCRtime = CalculateTimeDifference(startOCRt, endOCRt);

            if (!string.IsNullOrEmpty(annotationText))
            {
                string product_formatted;
                float startMajt = Time.realtimeSinceStartup;
                var wordsOCR = SplitStringToList(annotationText);
                var product_desc = MajorityVoting.GetProductDesciption(wordsOCR);
                product_formatted = FormatDescription(product_desc);
                float endMajt = Time.realtimeSinceStartup;
                Majoritytime = CalculateTimeDifference(startMajt, endMajt);
                text_result = product_formatted.ToLower();
                yield return StartCoroutine(voiceSynthesizer.PerformSpeechFromText(text_result));
            }
            else
            {
                yield return StartCoroutine(voiceSynthesizer.PerformSpeechFromText("Δεν αναγνωρίστηκαν λέξεις"));
            }
        }

        public IEnumerator GetProductDescription()
        {
            // output message to user.
            if (voiceSynthesizer != null) { yield return StartCoroutine(voiceSynthesizer.PerformSpeechFromText("Αναγνώριση προϊόντος")); }

            float startOCRt = Time.realtimeSinceStartup;

            //ocr annotation.
            yield return StartCoroutine(annotator.PerformAnnotation(camTexture));
            annotationText = annotator.GetAnnotationResults<string>();

            float endOCRt = Time.realtimeSinceStartup;
            OCRtime = CalculateTimeDifference(startOCRt, endOCRt);

            if (!string.IsNullOrEmpty(annotationText))
            {
                string product_formatted;
                float startMajt = Time.realtimeSinceStartup;
                var wordsOCR = SplitStringToList(annotationText);
                var valid_words = MajorityVoting.GetValidWords(wordsOCR);
                product_formatted = FormatDescription(ListToString(valid_words));
                if (MajorityValidText != null && MajorityFinalText != null)
                {
                    MajorityValidText = product_formatted;
                    MajorityFinalText = product_formatted;
                }
                majorityFinal = product_formatted;
                float endMajt = Time.realtimeSinceStartup;
                Majoritytime = CalculateTimeDifference(startMajt, endMajt);
                text_result = product_formatted.ToLower();
                if (voiceSynthesizer != null) { yield return StartCoroutine(voiceSynthesizer.PerformSpeechFromText(text_result)); }
            }
            else
            {
                if (voiceSynthesizer != null) { yield return StartCoroutine(voiceSynthesizer.PerformSpeechFromText("Δεν αναγνωρίστηκαν λέξεις")); }
                text_result = "Δεν αναγνωρίστηκαν λέξεις";
            }
        }

        private string FormatDescription(string product)
        {
            var edit = SplitStringToList(product);
            edit = MajorityVoting.KeepElementsWithLen(edit, 4);
            var description = ListToString(edit);
            return description;
        }

        /// <summary>  
        /// This methods based on the category given as input, finds the description of the trail, shelf, inner shelf  
        /// database based on the Majority Voting algorithm of the homonymous class.
        /// </summary>
        /// <param name="category"> int </param>  
        /// <returns>IEnumarator object</returns>
        public IEnumerator GetTrailShelfDescription(int category)
        {
            // output message to user.
            if (voiceSynthesizer != null) { yield return StartCoroutine(voiceSynthesizer.PerformSpeechFromText("Παρακαλώ περιμένετε")); }
            float startOCRt = Time.realtimeSinceStartup;

            //ocr annotation.
            yield return StartCoroutine(annotator.PerformAnnotation(camTexture));
            annotationText = annotator.GetAnnotationResults<string>();

            float endOCRt = Time.realtimeSinceStartup;

            OCRtime = CalculateTimeDifference(startOCRt, endOCRt);

            if (!string.IsNullOrEmpty(annotationText))
            {
                float startMajt = Time.realtimeSinceStartup;

                List<string> OCR_List = SplitStringToList(annotationText);
                yield return StartCoroutine(majVoting.PerformMajorityVoting(OCR_List));
                OCR_List = null;
                float endMajt = Time.realtimeSinceStartup;
                Majoritytime = CalculateTimeDifference(startMajt, endMajt);

                if (!verboseMode)
                {
                    switch (category)
                    {
                        case (int)Enums.MasoutisCategories.trail:
                            text_result = majVoting.masoutis_item.category_2;
                            if (voiceSynthesizer != null) { yield return StartCoroutine(voiceSynthesizer.PerformSpeechFromText("διάδρομος, " + text_result)); }
                            break;
                        case (int)Enums.MasoutisCategories.shelf:
                            text_result = majVoting.masoutis_item.category_4;
                            if (voiceSynthesizer != null) { yield return StartCoroutine(voiceSynthesizer.PerformSpeechFromText("ράφι, " + text_result)); }
                            break;
                        case (int)Enums.MasoutisCategories.other:
                            if (voiceSynthesizer != null) { yield return StartCoroutine(voiceSynthesizer.PerformSpeechFromText("άλλο, " + "μη αναγνωρίσιμο")); }
                            break;
                        default:
                            break;
                    }
                }
                else
                {
                    text_result = "διάδρομος " + majVoting.masoutis_item.category_2 + ", " +
                                   "ράφι " + majVoting.masoutis_item.category_3 + ", " +
                                   "υποκατηγορία ραφιού " + majVoting.masoutis_item.category_4;
                    if (voiceSynthesizer != null) { yield return StartCoroutine(voiceSynthesizer.PerformSpeechFromText(text_result)); }
                }
            }
            else
            {
                switch (category)
                {
                    case (int)Enums.MasoutisCategories.trail:
                        text_result = "Δεν αναγνωρίστηκαν διαθέσιμες λέξεις";
                        yield return StartCoroutine(voiceSynthesizer.PerformSpeechFromText("διάδρομος, " + text_result));
                        break;
                    case (int)Enums.MasoutisCategories.shelf:
                        text_result = "Δεν αναγνωρίστηκαν διαθέσιμες λέξεις";
                        yield return StartCoroutine(voiceSynthesizer.PerformSpeechFromText("ράφι, " + text_result));
                        break;
                    case (int)Enums.MasoutisCategories.other:
                        yield return StartCoroutine(voiceSynthesizer.PerformSpeechFromText("άλλο, " + "μη αναγνωρίσιμο"));
                        break;
                    default:
                        break;
                }
                if (MajorityFinalText != null)
                {
                    MajorityFinalText = "Δεν αναγνωρίστηκαν διαθέσιμες λέξεις";
                }
                if (MajorityValidText != null)
                {
                    MajorityValidText = "κενό";
                }

                yield return StartCoroutine(voiceSynthesizer.PerformSpeechFromText("Δεν αναγνωρίστηκαν διαθέσιμες λέξεις"));
            }
        }

        public override void CancelButton()
        {
            if (!annotationProccessBusy && !DB_LoadProccessBusy)
            {
                return;
            }

            StopAllCoroutines();
            annotationProccessBusy = false;
        }
    }
}
