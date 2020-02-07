using System.Collections;
using UnityEngine;
using static EVISION.Camera.plugin.MasoutisView;
using System.Collections.Generic;
using System.Linq;
using static EVISION.Camera.plugin.GenericUtils;


namespace EVISION.Camera.plugin
{
    public class ParaliaManager : CameraClient
    {
        public override IEnumerator ProcessScreenshotAsync()
        {
            throw new System.NotImplementedException();
        }

        public override void SaveScreenshot(Texture2D camTexture)
        {
            throw new System.NotImplementedException();
        }

        public override void SetResultLogs()
        {
            throw new System.NotImplementedException();
        }

        /// <summary>
        /// This method utilizes the computer vision models of the class for detecting objects
        /// in a given image (texture2D format) derived from the base(parent) class.
        /// </summary>
        /// <returns>IEnumarator gameobject</returns>
        public IEnumerator DetectObjects()
        {
            yield return null;
        } 

        // Use this for initialization
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}
