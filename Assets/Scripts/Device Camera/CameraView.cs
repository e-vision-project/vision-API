using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace EVISION.MobileCamera.API
{
    public class CameraView : ApplicationElement
    {
        private Texture2D screenshotTex;
        public Texture2D ScreenshotTex { get { return screenshotTex; } }

        // Canvas UI components
        public RawImage displayImage;
        public RectTransform imageParent;
        public AspectRatioFitter imageFitter;

        // Camera Textures
        private WebCamTexture frontCameraTexture;
        private WebCamTexture backCameraTexture;
        private WebCamTexture activeCameraTexture;

        private void Start()
        {
            if( _app.camCtlr.GetNumberOfDevices() == 0)
            {
                Debug.Log("No device cameras found");
                return;
            }

            // Set camera devices
            _app.camModel.frontCameraDevice = _app.camCtlr.GetCameraDevice(1);
            _app.camModel.backCameraDevice =  _app.camCtlr.GetCameraDevice(0);
            _app.camModel.activeCameraDevice = _app.camModel.backCameraDevice;

            // Set camera textures
            frontCameraTexture = new WebCamTexture(_app.camModel.frontCameraDevice.name);
            backCameraTexture = new WebCamTexture(_app.camModel.backCameraDevice.name);


            // Set camera filter modes for a smoother looking image
            frontCameraTexture = _app.camCtlr.SetCamFilterMode(frontCameraTexture);
            backCameraTexture = _app.camCtlr.SetCamFilterMode(backCameraTexture);

            // Set active camera texture
            activeCameraTexture = _app.camCtlr.SetActiveCameraTexture(backCameraTexture, activeCameraTexture, _app.camModel.activeCameraDevice);
            // Set screen device image to camTexture
            SetMainCamView(activeCameraTexture);
        }

        private void Update()
        {
            if(_app.camCtlr.GetNumberOfDevices() == 0) { Debug.Log("No device cameras found"); return; }

            // Skip making adjustment for incorrect camera data
            if (activeCameraTexture.width < 100)
            {
                Debug.Log("Still waiting another frame for correct info...");
                return;
            }

            // Rotate image to show correct orientation
            _app.camModel.rotationVector.z = -activeCameraTexture.videoRotationAngle;
            displayImage.rectTransform.localEulerAngles = _app.camModel.rotationVector;

            // Set AspectRatioFitter's ratio
            float videoRatio =
                (float)activeCameraTexture.width / (float)activeCameraTexture.height;
            imageFitter.aspectRatio = videoRatio;

            // Unflip if vertically flipped
            displayImage.uvRect =
                activeCameraTexture.videoVerticallyMirrored ? _app.camModel.fixedRect : _app.camModel.defaultRect;

            // Mirror front-facing camera's image horizontally to look more natural
            imageParent.localScale =
                _app.camModel.activeCameraDevice.isFrontFacing ? _app.camModel.fixedScale : _app.camModel.defaultScale;
        }

        private void SetMainCamView(WebCamTexture activeCamTexture)
        {
            displayImage.texture = activeCameraTexture;
            displayImage.material.mainTexture = activeCameraTexture;
            activeCameraTexture.Play();
        }

        public void CaptureScreenshot()
        {
            screenshotTex = _app.camCtlr.GetCameraTexture(activeCameraTexture);
        }

        public void SaveScreenshot()
        {
            _app.camCtlr.SaveTextureToGallery(screenshotTex, "Albanis", "filename");
        }
    }
}
