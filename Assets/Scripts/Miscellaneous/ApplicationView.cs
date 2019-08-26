using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace EVISION.Camera.plugin
{

    public class ApplicationView : MonoBehaviour
    {
        public GameObject image;
        public GameObject imageDd;
        public Text text;
        public ClientApplication clientApp;
        public ARCameraController ARCamera;
        public DeviceCamera cam;
        public Text deviceNumberText;
        public Text resolutionText;
        public RawImage helperImage;
        [SerializeField] private CameraType cameraType;


        // Start is called before the first frame update
        void Start()
        {
            image.SetActive(false);
        }

        public void OnButtonPressed()
        {
            Texture2D tex = ARCamera.TakeScreenShot();
            helperImage.texture = tex;
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
            if (cameraType == CameraType.Webcam)
            {
                deviceNumberText.text = WebCamTexture.devices.Length.ToString();
                resolutionText.text = cam.GetCamTextureWidthHeight().x.ToString() + " "
                    + cam.GetCamTextureWidthHeight().y.ToString();
            }
        }
    }
}
