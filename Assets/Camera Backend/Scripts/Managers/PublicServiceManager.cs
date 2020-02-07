using EVISION.Camera.plugin;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using static EVISION.Camera.plugin.GenericUtils;
using System;

public class PublicServiceManager : CameraClient
{
    // INTERFACES
    private IModelPrediction featureExtractor;
    private IModelPrediction svmClassifier;

    // INSPECTOR PROPERTIES
    [SerializeField] private TextAsset DLModel;
    [SerializeField] private TextAsset LabelsFile;
    [SerializeField] private bool logging;

    private SVMClassification svm_model;
    private string annotationText;

    // PUBLIC PROPERTIES
    public static int category;

    // LOGGING
    private float OCRtime;
    private float classificationTime;

    public void ScreenshotButtonListener()
    {
        if (annotationProccessBusy)
        {
            return;
        }

        StartCoroutine(ProcessScreenshotAsync());
    }


    public override IEnumerator ProcessScreenshotAsync()
    {
        if (currentCam != null)
        {
            // lock the process so the user cannot access it.
            annotationProccessBusy = true;

            // Get camera texture.
            yield return StartCoroutine(GetScreenshot());

            category = ClassifyCategory(camTexture);
            category = 2;

            switch (category)
            {
                case (int)Enums.PServiceCategories.document:
                    yield return StartCoroutine(ReadDocument());
                    break;
                case (int)Enums.PServiceCategories.sign:
                    yield return StartCoroutine(ReadSign());
                    break;
                case (int)Enums.PServiceCategories.face:
                    yield return StartCoroutine(ReadFace());
                    break;
                case (int)Enums.PServiceCategories.other:
                    break;
            }

            annotationProccessBusy = false;
        }

        SetTimeText();
    }

    /// <summary>  
    /// This methods aims to perform OCR annotation in the text contained in a sign and returns that text to the user.  
    /// </summary>
    /// <returns>IEnumeratorr object</returns>
    public IEnumerator ReadSign()
    {
        // output message to user.
        yield return StartCoroutine(voiceSynthesizer.PerformSpeechFromText("Επεξεργασία Πινακίδας"));

        float startOCRt = Time.realtimeSinceStartup;

        //ocr annotation.
        yield return StartCoroutine(annotator.PerformAnnotation(camTexture));
        annotationText = annotator.GetAnnotationResults<string>();

        float endOCRt = Time.realtimeSinceStartup;

        OCRtime = CalculateTimeDifference(startOCRt, endOCRt);
        PublicServiceView.resultText.text = annotationText;
        yield return StartCoroutine(voiceSynthesizer.PerformSpeechFromText(annotationText));
    }

    /// <summary>  
    /// This methods aims to perform face detection in an image and return the emotions of the depicted individuals.  
    /// </summary>
    /// <returns>IEnumeratorr object</returns>
    public IEnumerator ReadFace()
    {
        // output message to user.
        yield return StartCoroutine(voiceSynthesizer.PerformSpeechFromText("Επεξεργασία Προσώπων"));

        float startOCRt = Time.realtimeSinceStartup;

        //ocr annotation.
        yield return StartCoroutine(annotator.PerformAnnotation(camTexture));
        annotationText = annotator.GetAnnotationResults<string>();

        float endOCRt = Time.realtimeSinceStartup;

        OCRtime = CalculateTimeDifference(startOCRt, endOCRt);
        var faces = SplitStringToList(annotationText);
        var faceGroups = faces.GroupBy(x => x).ToDictionary(x => x.Key, x => x.ToList().Count);
        faceGroups.Remove("");
        foreach (var face in faceGroups)
        {
            text_result += ", " + face.Value.ToString() + " Πρόσωπα με συναισθήματα " + face.Key;
        }
        Debug.Log(text_result);
        yield return StartCoroutine(voiceSynthesizer.PerformSpeechFromText(text_result));
    }

    /// <summary>  
    /// This methods aims to perform OCR annotation in the text contained in a document and returns that text to the user.  
    /// </summary>
    /// <returns>IEnumeratorr object</returns>
    public IEnumerator ReadDocument()
    {
        // output message to user.
        yield return StartCoroutine(voiceSynthesizer.PerformSpeechFromText("Επεξεργασία Εγγράφου"));

        float startOCRt = Time.realtimeSinceStartup;

        //ocr annotation.
        yield return StartCoroutine(annotator.PerformAnnotation(camTexture));
        annotationText = annotator.GetAnnotationResults<string>();

        float endOCRt = Time.realtimeSinceStartup;

        OCRtime = CalculateTimeDifference(startOCRt, endOCRt);

        yield return StartCoroutine(voiceSynthesizer.PerformSpeechFromText(annotationText));
    }

    public override void SaveScreenshot(Texture2D camTexture)
    {
        throw new System.NotImplementedException();
    }

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

    #endregion

    #region MonoBehaviour Callbacks

    private void Awake()
    {
        cams = GetComponents<IDeviceCamera>();
        annotator = GetComponent<IAnnotate>();
        voiceSynthesizer = GetComponent<ITextToVoice>();
        httpLoader = GetComponent<HttpImageLoading>();

        SetSVM();
    }

    // Start is called before the first frame update
    void Start()
    {
        annotationProccessBusy = false;

        //native camera 
        currentCam = cams[0];

        //native camera
        if (!externalCamera)
        {
            ConnectNativeCamera();
        }

        EventCamManager.onExternalCamError += ConnectNativeCamera;
        EventCamManager.onAnnotationFailed += AnnotationFailedHandler;

    }

    // Update is called once per frame
    void Update()
    {
        if (currentCam != null) { currentCam.Tick(); }
    }

    #endregion

    private void SetTimeText()
    {
        if (PublicServiceView.TimeText != null)
        {
            PublicServiceView.TimeText.text = "Full process costed : " + (OCRtime + classificationTime).ToString() + "\nOCRtime: " + OCRtime.ToString()
                +  "\nClassificationTime: " + classificationTime.ToString();
        }
    }

    public override void SetResultLogs()
    {
        if (logging)
        {
            var sum = captureTime + OCRtime + classificationTime;
            string fileName = string.Format("dimarxeio_{0}", System.DateTime.Now.ToString());
            fileName = fileName.Replace(" ", "_");
            fileName = fileName.Replace(":", "_");
            fileName = fileName.Replace("/", "_");
            Debug.Log(fileName);
            string imageName = LogManager.GetResultLogs("Image Name", fileName);
            string response = LogManager.GetResponseTime(captureTime.ToString(), OCRtime.ToString(), classificationTime.ToString(), sum.ToString());
            string classificationResults = LogManager.GetResultLogs("Classification", category.ToString());
            string logText = imageName + "\n" + response + "\n" + classificationResults + "\n\n";
            LogManager.SaveResultLogs(logText);
            currentCam.SaveScreenShot(camTexture, fileName + ".png");
        }
    }
}
