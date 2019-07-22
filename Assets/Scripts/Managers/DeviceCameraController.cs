using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using System.Collections;
using System.IO;

namespace EVISION.Camera.plugin
{
    public class DeviceCameraController : MonoBehaviour
    {

        #region Properties

        public static Texture2D screenshot_tex;

        // Canvas UI components
        public RawImage displayImage;
        public RectTransform imageParent;
        public AspectRatioFitter imageFitter;

        // Device cameras
        WebCamDevice frontCameraDevice;
        WebCamDevice backCameraDevice;
        WebCamDevice activeCameraDevice;

        WebCamTexture frontCameraTexture;
        WebCamTexture backCameraTexture;
        WebCamTexture activeCameraTexture;

        // Image rotation
        Vector3 rotationVector = new Vector3(0f, 0f, 0f);

        // Image uvRect
        Rect defaultRect = new Rect(0f, 0f, 1f, 1f);
        Rect fixedRect = new Rect(0f, 1f, 1f, -1f);

        // Image Parent's scale
        Vector3 defaultScale = new Vector3(1f, 1f, 1f);
        Vector3 fixedScale = new Vector3(-1f, 1f, 1f);

        #endregion

        #region Monobehaviour Methods

        void Start()
        {
            // Check for device cameras
            if (WebCamTexture.devices.Length == 0)
            {
                Debug.Log("No device cameras found");
                return;
            }

            // Get the device's cameras and create WebCamTextures with them
            frontCameraDevice = WebCamTexture.devices.Last();
            backCameraDevice = WebCamTexture.devices.First();

            frontCameraTexture = new WebCamTexture(frontCameraDevice.name);
            backCameraTexture = new WebCamTexture(backCameraDevice.name);

            // Set camera filter modes for a smoother looking image
            frontCameraTexture.filterMode = FilterMode.Trilinear;
            backCameraTexture.filterMode = FilterMode.Trilinear;

            // Set the camera to use by default
            SetActiveCamera(backCameraTexture);
        }

        // Make adjustments to image every frame to be safe, since Unity isn't 
        // guaranteed to report correct data as soon as device camera is started
        void Update()
        {
            if (WebCamTexture.devices.Length == 0)
            {
                print(WebCamTexture.devices.Length);
                return;
            }

            // Skip making adjustment for incorrect camera data
            if (activeCameraTexture.width < 100)
            {
                Debug.Log("Still waiting another frame for correct info...");
                return;
            }

            // Rotate image to show correct orientation 
            rotationVector.z = -activeCameraTexture.videoRotationAngle;
            displayImage.rectTransform.localEulerAngles = rotationVector;

            // Set AspectRatioFitter's ratio
            float videoRatio =
                (float)activeCameraTexture.width / (float)activeCameraTexture.height;
            imageFitter.aspectRatio = videoRatio;

            // Unflip if vertically flipped
            displayImage.uvRect =
                activeCameraTexture.videoVerticallyMirrored ? fixedRect : defaultRect;

            // Mirror front-facing camera's image horizontally to look more natural
            imageParent.localScale =
                activeCameraDevice.isFrontFacing ? fixedScale : defaultScale;
        }

        #endregion

        #region Device Camera Methods

        // Set the device camera to use and start it
        public void SetActiveCamera(WebCamTexture cameraToUse)
        {
            if (activeCameraTexture != null)
            {
                activeCameraTexture.Stop();
            }

            activeCameraTexture = cameraToUse;
            activeCameraDevice = WebCamTexture.devices.FirstOrDefault(device =>
                device.name == cameraToUse.deviceName);

            displayImage.texture = activeCameraTexture;
            displayImage.material.mainTexture = activeCameraTexture;

            activeCameraTexture.Play();
        }

        // Switch between the device's front and back camera
        public void SwitchCamera()
        {
            SetActiveCamera(activeCameraTexture.Equals(frontCameraTexture) ?
                backCameraTexture : frontCameraTexture);
        }

        public void TakeScreenShot(bool saveToGallery)
        {
            Debug.Log("e-vision platform logs:" + "Remember to open wifi for mobile device");
            StartCoroutine(SaveScreenShot(saveToGallery));
        }

        private IEnumerator SaveScreenShot(bool saveToGallery)
        {
            yield return new WaitForEndOfFrame();

            Texture2D snap = new Texture2D(activeCameraTexture.width, activeCameraTexture.height);
            snap.SetPixels(activeCameraTexture.GetPixels());
            snap.Apply();
            snap = rotateTexture(snap, true);
            
            //add to static texture
            screenshot_tex = snap;

            // Save the screenshot to Gallery/Photos
            if(saveToGallery)
            {
                string name = string.Format("{0}_Capture{1}.png", Application.productName, "{0}");
                Debug.Log("Permission result: " + NativeGallery.SaveImageToGallery(snap, "e-vision", name));
            }
        }

        private Texture2D rotateTexture(Texture2D originalTexture, bool clockwise)
        {
            Color32[] original = originalTexture.GetPixels32();
            Color32[] rotated = new Color32[original.Length];
            int w = originalTexture.width;
            int h = originalTexture.height;

            int iRotated, iOriginal;

            for (int j = 0; j < h; ++j)
            {
                for (int i = 0; i < w; ++i)
                {
                    iRotated = (i + 1) * h - j - 1;
                    iOriginal = clockwise ? original.Length - 1 - (j * w + i) : j * w + i;
                    rotated[iRotated] = original[iOriginal];
                }
            }

            Texture2D rotatedTexture = new Texture2D(h, w);
            rotatedTexture.SetPixels32(rotated);
            rotatedTexture.Apply();
            return rotatedTexture;
        }

        #endregion

    }
}