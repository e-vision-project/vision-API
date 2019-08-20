using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace EVISION.Camera.plugin
{

    public class ClientApplication : MonoBehaviour
    {
        #region properties

        public IDeviceCamera cam;
        private IAnnotate OCRAnnotator;
        private ITextToVoice voiceSynthesizer;
        [SerializeField] private DeviceCamera.Cameras cameraDevice;
        public Texture2D camTexture;
        //public Texture2D CamTexture { get { return camTexture;} }
        private string annotationText { get; set; }
        public bool annotationProccessBusy { get; set; }

        public TextAsset DLModel;
        public TextAsset LabelsFile;
        TFSharpClassification classifier;

        #endregion


        private void Awake()
        {
            cam = GetComponent<IDeviceCamera>();
            OCRAnnotator = GetComponent<IAnnotate>();
            voiceSynthesizer = GetComponent<ITextToVoice>();
            classifier = new TFSharpClassification("input_1", "Logits/Softmax", 224, 224, 127.5f, 127.5f, DLModel, LabelsFile, 180, 0.05f);
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

        public IEnumerator TakeScreenshotSingleThreaded()
        {
            // lock the process so the user cannot access it.
            annotationProccessBusy = true;

            // Get and rotate camera texture.
            camTexture = cam.TakeScreenShot();
            camTexture = GenericUtils.RotateTexture(camTexture, true);

            //wait until the annotation process returns
            yield return StartCoroutine(OCRAnnotator.PerformAnnotation(camTexture));
            annotationText = OCRAnnotator.GetAnnotationText();

            // Perform majority voting
            if (annotationText != null)
            {
                List<string> OCR_List = GenericUtils.SplitStringToList(annotationText);
                MasoutisItem product = GenericUtils.PerformMajorityVoting(OCR_List);
                // Text to speech
                voiceSynthesizer.PerformSpeechFromText(product.category_4);
            }
            else
            {
                voiceSynthesizer.PerformSpeechFromText("μη αναγνωρίσιμο");
            }

            //unlock process
            annotationProccessBusy = false;
        }

        public void SaveScreenShot()
        {
            cam.SaveScreenShot(camTexture);
        }

        public void CLassify()
        {
            camTexture = cam.TakeScreenShot();
            camTexture = GenericUtils.RotateTexture(camTexture, true);
            var output = classifier.FetchOutput(camTexture);
            foreach (KeyValuePair<string, float> value in output)
            {
                Debug.Log("class :" + value.Key + "_" + value.Value);
            }
        }
    }
}
