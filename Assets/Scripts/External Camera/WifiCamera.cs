using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace EVISION.Camera.plugin
{
    public class WifiCamera : MonoBehaviour, IDeviceCamera
    {
        public UniversalMediaPlayer UMP;
        public RawImage displayImg;
        private Texture2D screenshotTex;


        #region IDeviceCamera callbacks

        public void ConnectCamera()
        {
            UMP.Play();
        }

        public Texture2D TakeScreenShot()
        {
            Texture2D screenshotTex = GenericUtils.RenderTexToTex2D(displayImg.texture);
            return screenshotTex;
        }

        public void SaveScreenShot(Texture2D snap)
        {
            throw new System.NotImplementedException();
        }

        public void SwitchCamera()
        {
            throw new System.NotImplementedException();
        }

        public void Tick()
        {
            Debug.Log("update in wifi camera");
        }

        #endregion

        public void DisableCameraView()
        {
            displayImg.enabled = false;
        }
    }
}
