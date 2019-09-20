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
        public static RawImage helperImage;
        public GameObject button;
        //public Text text;
        public MasoutisClient clientApp;
        public DeviceCamera cam;
        public static Text wordsText;
        public static Text MajorityValidText;
        public static Text MajorityFinalText;
        public static Text classText;
        public static Text TimeText;
        public Text resolutionText;
        public Text devicesText;
        public static int capture_count = 0;
        public static string capture_name = "";


        // Start is called before the first frame update
        void Start()
        {
            MajorityValidText = GameObject.FindGameObjectWithTag("MAJORITY_TEXT").GetComponent<Text>();
            MajorityFinalText = GameObject.FindGameObjectWithTag("MAJORITY_FINAL").GetComponent<Text>();
            classText = GameObject.FindGameObjectWithTag("CLASS").GetComponent<Text>();
            TimeText = GameObject.FindGameObjectWithTag("TIME").GetComponent<Text>();
            image.SetActive(false);
        }

        public void OnButtonPressed()
        {

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

            devicesText.text = WebCamTexture.devices.Length.ToString();
           
            resolutionText.text = cam.GetCamTextureWidthHeight().x.ToString() + " "
                + cam.GetCamTextureWidthHeight().y.ToString();  
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
            }

            File.AppendAllText(path, text);
        }

        public static void SaveImageFile(Texture2D tex)
        {
            string imagePath = "";

            if (Application.isEditor)
            {
                imagePath = Application.dataPath + "/captured_images";
            }
            else
            {
                imagePath = Application.persistentDataPath + "/captured_images";
            }

            if (!Directory.Exists(imagePath))
            {
                Directory.CreateDirectory(imagePath);
            }

            var bytes = tex.EncodeToPNG();
            Destroy(tex);
            capture_name = string.Format("/{0}_Capture{1}.png", Application.productName, capture_count.ToString());
            System.IO.File.WriteAllBytes(imagePath + capture_name, bytes);
            capture_count++;
        }
    }
}
