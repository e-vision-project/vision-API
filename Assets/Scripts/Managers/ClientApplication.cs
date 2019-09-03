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

        private Texture2D camTexture;
        private MasoutisItem masoutis_obj;
        private string annotationText;
        public string AnnotationText { get; }
        public bool annotationProccessBusy { get; set; }
        #endregion


        private void Awake()
        {
            cam = GetComponent<IDeviceCamera>();
            OCRAnnotator = GetComponent<IAnnotate>();
            voiceSynthesizer = GetComponent<ITextToVoice>();
             
            _classifier = new TFClassification("input_1", "Logits/Softmax", 224, 224, 127.0f, 127.0f, DLModel, LabelsFile, 180, 0.05f);
            featureExtractor = new TFFeatureExtraction("input_1", "block_15_project/convolution", 224, 224, 127.5f, 127.5f, DLModel, LabelsFile, 180, 0.01f);
            
            // set svm model
            SVMClassification svm_model = new SVMClassification();
            svm_model.SetModelParameters("Model_SVM");
            svmClassifier = svm_model;
        }

        // Start is called before the first frame update
        void Start()
        {
            annotationProccessBusy = false;
            //cam.SetCamera(cameraDevice);
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

            while (annotationProccessBusy)
            {
                // Get camera texture.
                //camTexture = Resources.Load<Texture2D>("Textures/Masoutis/" + "product_2");
                camTexture = cam.TakeScreenShot();
                SaveScreenShot();

                int category = ClassifyCategory();

                yield return StartCoroutine(GetCategoryDescription(category));

                annotationProccessBusy = false;
            }
            
        }

        private int ClassifyCategory()
        {

            var featureVector = featureExtractor.FetchOutput<List<float>, Texture2D>(camTexture);

            var probs = svmClassifier.FetchOutput<List<float>, List<float>>(featureVector);

            float maxValue = probs.Max();

            int category_index = probs.IndexOf(maxValue);

            return category_index;
        }

        public IEnumerator GetCategoryDescription(int category)
        {
            //wait until the annotation process returns
            yield return StartCoroutine(OCRAnnotator.PerformAnnotation(camTexture));
            annotationText = OCRAnnotator.GetAnnotationText();

            // Perform majority voting
            if (!string.IsNullOrEmpty(annotationText))
            {
                Debug.Log("Non empty test");
                List<string> OCR_List = GenericUtils.SplitStringToList(annotationText);
                MajorityVoting majVoting = new MajorityVoting();
                yield return majVoting.PerformMajorityVoting(OCR_List);

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
                Debug.Log("empty test");
                category += 2;
                yield return StartCoroutine(voiceSynthesizer.PerformSpeechFromText("κατηγορία " + category.ToString() +" Δεν αναγνωρίστηκαν διαθέσιμες λέξεις"));
            }
        }

        public IEnumerator TakeScreenshotSingleThreaded()
        {
            // lock the process so the user cannot access it.
            annotationProccessBusy = true;

            // Get camera texture.
            camTexture = cam.TakeScreenShot();

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

            SaveScreenShot();
            annotationProccessBusy = false;
        }

        public void SaveScreenShot()
        {
            Texture2D tex = cam.TakeScreenShot();
            cam.SaveScreenShot(tex);
        }

        public void CLassify()
        {
            camTexture = cam.TakeScreenShot();
            var output = _classifier.FetchOutput<IList, Texture2D>(camTexture);
            foreach (KeyValuePair<string, float> value in output)
            {
                Debug.Log("class :" + value.Key + "_" + value.Value);
            }
        }
    }
}
