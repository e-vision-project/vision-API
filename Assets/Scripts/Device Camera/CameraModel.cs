using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EVISION.MobileCamera.API
{
    public class CameraModel : ApplicationElement
    {
        // Device cameras
        public WebCamDevice frontCameraDevice;
        public WebCamDevice backCameraDevice;
        public WebCamDevice activeCameraDevice;

        // Image rotation
        public Vector3 rotationVector = new Vector3(0f, 0f, 0f);
        [Space]
        // Image uvRect
        public Rect defaultRect = new Rect(0f, 0f, 1f, 1f);
        public Rect fixedRect = new Rect(0f, 1f, 1f, -1f);
        [Space]
        // Image Parent's scale
        public Vector3 defaultScale = new Vector3(1f, 1f, 1f);
        public Vector3 fixedScale = new Vector3(-1f, 1f, 1f);
    }
}
