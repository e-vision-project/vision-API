using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace EVISION.Camera.plugin
{
    public class WifiCamera : MonoBehaviour, IExternalCamera
    {
        public UniversalMediaPlayer UMP;
        public RawImage displayImg;
        private Texture2D screenshotTex;

        #region IExternalCamera callbacks

        public Texture2D GetScreenShot()
        {
            Texture2D screenshotTex = GenericUtils.RenderTexToTex2D(displayImg.texture);
            return screenshotTex;
        }

        public void SaveScreenshot()
        {
            throw new System.NotImplementedException();
        }

        public void SetCamera(string url)
        {
            UMP.Play();
        }

        #endregion

        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}
