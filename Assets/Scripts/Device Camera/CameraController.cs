using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

namespace EVISION.MobileCamera.API
{
    public class CameraController : ApplicationElement
    {
        #region Camera handling methods

        public WebCamTexture SetActiveCameraTexture(WebCamTexture camTexture, WebCamTexture activeCamTexture, WebCamDevice activeCamDevice)
        {
            if(activeCamTexture != null) { activeCamTexture.Stop(); }

            activeCamTexture = camTexture;
            activeCamDevice = WebCamTexture.devices.FirstOrDefault(device =>
              device.name == camTexture.deviceName);

            return activeCamTexture;
        }

        public WebCamTexture SwitchCamera(WebCamTexture frontCam, WebCamTexture backCam, WebCamTexture activeCamTexture, WebCamDevice activeCamDevice)
        {
            WebCamTexture camTexture = activeCamDevice.Equals(frontCam) ? backCam : frontCam;
            return SetActiveCameraTexture(camTexture, activeCamTexture, activeCamDevice);
        }

        public Texture2D GetCameraTexture(WebCamTexture activeCamTexture)
        {
            Texture2D snap = new Texture2D(activeCamTexture.width, activeCamTexture.height);
            snap.SetPixels(activeCamTexture.GetPixels());
            snap.Apply();
            return snap;
        }

        public void SaveTextureToGallery(Texture2D screenshot, string album, string filename)
        {
            Debug.Log("Permission result: " + NativeGallery.SaveImageToGallery(screenshot, album, filename));
        }

        #endregion

        public int GetNumberOfDevices()
        {
            return WebCamTexture.devices.Length;
        }

        public WebCamDevice GetCameraDevice(int deviceNumber)
        {
            return WebCamTexture.devices[deviceNumber];
        }

        public WebCamTexture SetCamFilterMode(WebCamTexture camTexture)
        {
            camTexture.filterMode = FilterMode.Trilinear;
            return camTexture;
        }
    }
}
