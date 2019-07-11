using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class ClientApplication : MonoBehaviour
{
    #region properties

    public IDeviceCamera cam;
    private IAnnotate annotator;
    private ITextToVoice voiceSynthesizer;
    private Texture2D camTexture;
    private string annotationText { get; set;}
    public bool annotationProccessBusy { get; set;}

    // FOR test only remember to remove
    public TestTensorflow tensorflow;

    #endregion


    private void Awake()
    {
        cam = GetComponent<IDeviceCamera>();
        annotator = GetComponent<IAnnotate>();
        voiceSynthesizer = GetComponent<ITextToVoice>();
    }

    // Start is called before the first frame update
    void Start()
    {
        annotationProccessBusy = false;
        cam.SetCamera(DeviceCamera.Cameras.Back);
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
            Debug.Log("Process busy");
            return;
        }
        StartCoroutine(TakeScreenshotSingleThreaded());
    }

    public IEnumerator TakeScreenshotSingleThreaded()
    {
        // lock the process so the user cannot access it.
        annotationProccessBusy = true;
        // Get and rotate camera texture
        camTexture = cam.TakeScreenShot();
        camTexture = Utils.RotateTexture(camTexture, true);
        tensorflow.ProcessImage(camTexture);
        yield return null; 
        // wait until the annotation process returns
        //yield return StartCoroutine(annotator.PerformAnnotation(camTexture));
        //try
        //{
        //    annotationText = annotator.GetAnnotationText();
        //}
        //catch (System.Exception)
        //{
        //    Debug.LogError("annotationText in ClientApplication dropped an exception error");
        //}
        //// Perform majority voting
        //List<string> OCR_List = Utils.SplitStringToList(annotationText);
        //yield return StartCoroutine(Utils.PerformMajorityVoting(OCR_List));
        //// Text to speech
        ////voiceSynthesizer.PerformSpeechFromText(product);
        ////unlock process
        annotationProccessBusy = false;
    }

    public void SaveScreenShot()
    {
        cam.SaveScreenShot(camTexture);
    }
}
