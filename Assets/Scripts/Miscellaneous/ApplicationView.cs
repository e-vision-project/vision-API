using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;

namespace EVISION.Camera.plugin
{

    public class ApplicationView : MonoBehaviour
    {
        public GameObject image;
        public GameObject imageDd;
        public GameObject button;
        public Text text;
        public ClientApplication clientApp;
        public ARCameraController ARCamera;
        public DeviceCamera cam;
        public static Text wordsText;
        public static Text MajorityValidText;
        public static Text MajorityFinalText;
        public Text resolutionText;
        public RawImage helperImage;
        public static int capture_count = 0;
        public static string capture_name = "";
        [SerializeField] private CameraType cameraType;


        // Start is called before the first frame update
        void Start()
        {
            wordsText = GameObject.FindGameObjectWithTag("OCR_TEXT").GetComponent<Text>();
            MajorityValidText = GameObject.FindGameObjectWithTag("MAJORITY_TEXT").GetComponent<Text>();
            MajorityFinalText = GameObject.FindGameObjectWithTag("MAJORITY_FINAL").GetComponent<Text>();
            image.SetActive(false);
        }

        public void OnButtonPressed()
        {
            //Texture2D tex = ARCamera.TakeScreenShot();
            //helperImage.texture = tex;
        }

        // Update is called once per frame
        void Update()
        {
            if (clientApp.annotationProccessBusy)
            {
                image.SetActive(true);
                button.SetActive(false);
                //Debug.Log("Annotating");
            }
            else if (!clientApp.annotationProccessBusy)
            {
                image.SetActive(false);
                button.SetActive(true);
            }
            //if (cameraType == CameraType.Webcam)
            //{
            //    deviceNumberText.text = WebCamTexture.devices.Length.ToString();
            //    resolutionText.text = cam.GetCamTextureWidthHeight().x.ToString() + " "
            //        + cam.GetCamTextureWidthHeight().y.ToString();
            //}
        }

        public static void SaveTXT(string text)
        {
            string path = "";
            string imagePath = "";

            #if UNITY_EDITOR_WIN
            path = Application.dataPath + "/e-vision-Results.txt";
            imagePath = Application.persistentDataPath + "/captured_images";
            #endif

            #if UNITY_ANDROID
            path = Application.persistentDataPath + "/e-vision-Results.txt";
            imagePath = Application.persistentDataPath + "/captured_images";
            #endif

            if (!File.Exists(path))
            {
                File.WriteAllText(path,"Debbuging app \n");
                Debug.Log("File created" + path);
            }

            File.AppendAllText(path, text);
            Debug.Log("appended in" + path);
        }

        public static void SaveImageFile(Texture2D tex)
        {
            string imagePath = "";

            imagePath = Application.persistentDataPath + "/captured_images";

            if (!Directory.Exists(imagePath))
            {
                Directory.CreateDirectory(imagePath);
                Debug.Log("images directory created");
            }

            var bytes = tex.EncodeToPNG();
            Destroy(tex);
            capture_name = string.Format("/{0}_Capture{1}.png", Application.productName, capture_count.ToString());
            Debug.Log("Image saved");
            System.IO.File.WriteAllBytes(imagePath + capture_name, bytes);
            capture_count++;
        }
    }
}
