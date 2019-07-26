using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace EVISION.Camera.plugin
{

    public class ApplicationView : MonoBehaviour
    {
        public GameObject image;
        public Text text;
        public ClientApplication clientApp;
        public AndroidCamera android;
        public DeviceCamera cam;
        public Text deviceNumberText;
        public Text resolutionText;
        public RawImage helperImage;
        

        // Start is called before the first frame update
        void Start()
        {
            image.SetActive(false);
        }

        public void OnButtonPressed()
        {
            helperImage.texture = android.TakeScreenShot();
        }

        // Update is called once per frame
        void Update()
        {
            if (clientApp.annotationProccessBusy)
            {
                image.SetActive(true);
                //Debug.Log("Annotating");
            }
            else if (!clientApp.annotationProccessBusy)
            {
                image.SetActive(false);
            }

            deviceNumberText.text = WebCamTexture.devices.Length.ToString();
            resolutionText.text = cam.GetCamTextureWidthHeight().x.ToString() +" " 
                + cam.GetCamTextureWidthHeight().y.ToString();
        }
    }
}
