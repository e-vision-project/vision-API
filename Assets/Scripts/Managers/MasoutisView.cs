using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;

namespace EVISION.Camera.plugin
{

    public class MasoutisView : CameraClientView
    {
        [Header("Images/Panels")]
        public GameObject imageDB;
        [Header("Text")]
        public static Text wordsText;
        public static Text MajorityValidText;
        public static Text MajorityFinalText;
        public static Text TimeText;
        public MasoutisManager clientApp;
        public static int capture_count = 0;
        public static string capture_name = "";


        // Start is called before the first frame update
        void Start()
        {
            MajorityValidText = GameObject.FindGameObjectWithTag("MAJORITY_TEXT").GetComponent<Text>();
            MajorityFinalText = GameObject.FindGameObjectWithTag("MAJORITY_FINAL").GetComponent<Text>();
            TimeText = GameObject.FindGameObjectWithTag("TIME").GetComponent<Text>();
            clientApp = gameObject.GetComponent<MasoutisManager>();
            annotatingImage.SetActive(false);
        }

        public void OnButtonPressed()
        {
            MajorityFinalText.text = "";
        }

        // Update is called once per frame
        void Update()
        {
            if (clientApp.DB_LoadProccessBusy)
            {
                imageDB.SetActive(true);
                ScreenshotButton.SetActive(false);
                cancelButton.SetActive(false);
                tapImage.SetActive(false);
            }
            else if (!clientApp.DB_LoadProccessBusy)
            {
                imageDB.SetActive(false);
                ScreenshotButton.SetActive(true);
                tapImage.SetActive(true);
            }
            if (clientApp.annotationProccessBusy)
            {
                annotatingImage.SetActive(true);
                ScreenshotButton.SetActive(false);
                cancelButton.SetActive(true);
                tapImage.SetActive(false);
            }
            else if (!clientApp.annotationProccessBusy)
            {
                annotatingImage.SetActive(false);
                ScreenshotButton.SetActive(true);
                cancelButton.SetActive(false);
                tapImage.SetActive(true);
            }
        }

        public static void SaveTXT(string text)
        {
            string path = "";
            string imagePath = "";

            #if UNITY_EDITOR_WIN
            path = Application.dataPath + "/evision_Results_unsorted.txt";
            imagePath = Application.persistentDataPath + "/captured_images";
            #endif

            #if UNITY_ANDROID
            path = Application.persistentDataPath + "/evision_Results_unsorted.txt";
            imagePath = Application.persistentDataPath + "/captured_images";
            #endif

            if (!File.Exists(path))
            {
                File.WriteAllText(path,"Debbuging app \n");
            }

            File.AppendAllText(path, text);
            Debug.Log("saved as :" + path);
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

            var bytes = tex.EncodeToJPG();
            //Destroy(tex);
            capture_name = string.Format("/{0}_Capture{1}.png", Application.productName, capture_count.ToString());
            System.IO.File.WriteAllBytes(imagePath + capture_name, bytes);
            capture_count++;
        }
    }
}
