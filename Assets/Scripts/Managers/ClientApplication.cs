using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Threading.Tasks;

namespace EVISION.Camera.plugin
{

    public class ClientApplication : MonoBehaviour
    {
        #region properties
 
        public IDeviceCamera cam;
        private IAnnotate OCRAnnotator;
        private ITextToVoice voiceSynthesizer;
        private IModelPrediction _classifier;
        private IModelPrediction featureExtractor;
        private IModelPrediction svmClassifier;

        //[SerializeField] private DeviceCamera.Cameras cameraDevice;
        [SerializeField] private TextAsset DLModel;
        [SerializeField] private TextAsset LabelsFile;
        [SerializeField] private string image_name;

        private Texture2D camTexture;
        public DeviceCamera.Cameras cameraDevice;
        private MasoutisItem masoutis_obj;
        private string annotationText;
        public string AnnotationText { get; }
        public bool annotationProccessBusy { get; set; }
        private float OCRtime;
        private float Majoritytime;


        #endregion


        private void Awake()
        {
            cam = GetComponent<IDeviceCamera>();
            OCRAnnotator = GetComponent<IAnnotate>();
            voiceSynthesizer = GetComponent<ITextToVoice>();
            
            //Get feautures from model
            featureExtractor = new TFFeatureExtraction("input_1", "block_15_project/convolution", 224, 224, 127.5f, 127.5f, DLModel, LabelsFile, 180, 0.01f);
            
            // set and load svm model
            SVMClassification svm_model = new SVMClassification();
            svm_model.SetModelParameters("Model_SVM");
            svmClassifier = svm_model;
        }

        // Start is called before the first frame update
        void Start()
        {
            annotationProccessBusy = false;
            cam.SetCamera(cameraDevice);
        }

        // Update is called once per frame
        void Update()
        {
            cam.Tick();
        }

        public void TakeScreenShot()
        {
            if (annotationProccessBusy)
            {
                return;
            }
            StartCoroutine(TakeScreenshotSingleThreaded());
        }

        public void ClassifyScreenShot()
        {
            if (annotationProccessBusy)
            {
                return;
            }
            StartCoroutine(ClassifyScreenshotAsync());
        }

        public IEnumerator ClassifyScreenshotAsync()
        {
            // lock the process so the user cannot access it.
            annotationProccessBusy = true;

            // Get camera texture.
            if (Application.isEditor)
            {
                camTexture = Resources.Load<Texture2D>("Textures/Masoutis/" + image_name);
                //camTexture = cam.TakeScreenShot();
            }
            else
            {
                camTexture = cam.TakeScreenShot();
                ApplicationView.SaveImageFile(camTexture);

            }

            int category = ClassifyCategory(camTexture);

            string cat = "κενό";
            if (category == 0) { cat = "διάδρομος"; }
            if (category == 1) { cat = "ράφι"; }
            if (category == 2) { cat = "προϊόν"; }
            if (category == 3) { cat = "άλλο"; }
            ApplicationView.classText.text = "κατηγορία " + cat;

            yield return StartCoroutine(GetCategoryDescription(category));

            ApplicationView.TimeText.text = "Full process costed : " + (OCRtime + Majoritytime).ToString() + "\nOCRtime: " + OCRtime.ToString()
                + "\nMajorityTime: " + Majoritytime.ToString();

            annotationProccessBusy = false;
            
        }

        private int ClassifyCategory(Texture2D input_tex)
        {
            var featureVector = featureExtractor.FetchOutput<List<float>, Texture2D>(input_tex);

            var probs = svmClassifier.FetchOutput<List<float>, List<float>>(featureVector);

            float maxValue = probs.Max();

            int category_index = probs.IndexOf(maxValue);

            return category_index;
        }

        public IEnumerator GetCategoryDescription(int category)
        {
            float startOCRt = Time.realtimeSinceStartup;
            
            //wait until the annotation process returns
            yield return StartCoroutine(OCRAnnotator.PerformAnnotation(camTexture));
            annotationText = OCRAnnotator.GetAnnotationText();

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

                ////save to file
                //ApplicationView.SaveTXT("\nclass: " + category.ToString() + "\nOCR: " + ApplicationView.wordsText.text  + "\nMAJ: " +
                //                 ApplicationView.MajorityText.text + "\ntrail: " + majVoting.masoutis_item.category_2 + ", shelf: " + majVoting.masoutis_item.category_3 + ", product: " + majVoting.masoutis_item.category_4
                //                 + "\n" + "Image_name :" + ApplicationView.capture_name + "\n=========================================");

                switch (category)
                {
                    case (int)Enums.MasoutisCategories.trail:
                        Debug.Log("category 2 : Trail");
                        yield return StartCoroutine(voiceSynthesizer.PerformSpeechFromText("διάδρομος, " + majVoting.masoutis_item.category_2));
                        break;
                    case (int)Enums.MasoutisCategories.shelf:
                        Debug.Log("category 3 : shelf");
                        yield return StartCoroutine(voiceSynthesizer.PerformSpeechFromText("ράφι, " + majVoting.masoutis_item.category_3));
                        break;
                    case (int)Enums.MasoutisCategories.product:
                        Debug.Log("category 4 : product");
                        yield return StartCoroutine(voiceSynthesizer.PerformSpeechFromText("προϊόν, " + majVoting.masoutis_item.category_4));
                        break;
                    case (int)Enums.MasoutisCategories.other:
                        Debug.Log("non reckognizable : other");
                        yield return StartCoroutine(voiceSynthesizer.PerformSpeechFromText("άλλο, " + "μη αναγνωρίσιμο"));
                        break;
                    default:
                        break;
                }
            }
            else
            {
                string cat = "κενό";
                if(category == 0) { cat = "διάδρομος";}
                if(category == 1) { cat = "ράφι";}
                if(category == 2) { cat = "προϊόν"; }
                if(category == 3) { cat = "άλλο"; }

                ApplicationView.wordsText.text = "κενό";
                ApplicationView.MajorityFinalText.text = "κενό";
                ApplicationView.MajorityValidText.text = "κενό";
                ApplicationView.classText.text = "κενό";

                //ApplicationView.SaveTXT("\nclass: " + category.ToString() + "\nOCR: " + "OCR_EMPTY"
                //                 + "\n" + "Image_name :" + ApplicationView.capture_name + "\n=========================================");

                yield return StartCoroutine(voiceSynthesizer.PerformSpeechFromText("κατηγορία " + cat + ", Δεν αναγνωρίστηκαν διαθέσιμες λέξεις"));
            }
        }

        public IEnumerator TakeScreenshotSingleThreaded()
        {
            // lock the process so the user cannot access it.
            annotationProccessBusy = true;
            
            // Get camera texture.
            //camTexture = cam.TakeScreenShot();
            camTexture = Resources.Load<Texture2D>("Textures/Masoutis/" + image_name);

            //wait until the annotation process returns
            yield return StartCoroutine(OCRAnnotator.PerformAnnotation(camTexture));
            annotationText = OCRAnnotator.GetAnnotationText();

            // Perform majority voting
            if (annotationText != null)
            {
                List<string> OCR_List = GenericUtils.SplitStringToList(annotationText);
                MajorityVoting majVoting = new MajorityVoting();
                yield return majVoting.PerformMajorityVoting(OCR_List);
                masoutis_obj = majVoting.masoutis_item;
                // Text to speech
                voiceSynthesizer.PerformSpeechFromText(masoutis_obj.category_4);
            }
            else
            {
                voiceSynthesizer.PerformSpeechFromText("μη αναγνωρίσιμο");
            }

            
            annotationProccessBusy = false;
        }

        public void SaveScreenShot()
        {
            Texture2D tex = cam.TakeScreenShot();
            cam.SaveScreenShot(tex);
        }

        public void CLassify()
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
                ApplicationView.SaveImageFile(camTexture);

            }

            string cat = "κενό";
            if (category == 0) { cat = "διάδρομος"; }
            if (category == 1) { cat = "ράφι"; }
            if (category == 2) { cat = "προϊόν"; }
            if (category == 3) { cat = "άλλο"; }
            ApplicationView.classText.text = "κατηγορία " + cat;
            ApplicationView.wordsText.text = "κενό";
            ApplicationView.MajorityFinalText.text = "κενό";
            ApplicationView.MajorityValidText.text = "κενό";
        }

        public float CalculateTimeDifference(float start, float end)
        {
            float timeToCompleteSec = 0;

            timeToCompleteSec = (float)System.Math.Round(end - start,2);

            return timeToCompleteSec;
        }
    }
}

