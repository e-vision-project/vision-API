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
        //public static Text OCRWordsText;
        //public static Text MajorityValidText;
        //public static Text MajorityFinalText;
        //public static string majorityFinal;
        //public static Text TimeText;
        public MasoutisManager clientApp;


        // Start is called before the first frame update
        void Start()
        {
            //MajorityValidText = GameObject.FindGameObjectWithTag("MAJORITY_TEXT").GetComponent<Text>();
            //MajorityFinalText = GameObject.FindGameObjectWithTag("MAJORITY_FINAL").GetComponent<Text>();
            //TimeText = GameObject.FindGameObjectWithTag("TIME").GetComponent<Text>();
            //OCRWordsText = GameObject.FindGameObjectWithTag("OCR_TEXT").GetComponent<Text>();
            clientApp = gameObject.GetComponent<MasoutisManager>();
            annotatingImage.SetActive(false);
        }

        public void OnButtonPressed()
        {
            //MajorityFinalText.text = "";
        }

        // Update is called once per frame
        void Update()
        {
            if (clientApp == null)
            {
                return;
            }
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
    }
}
