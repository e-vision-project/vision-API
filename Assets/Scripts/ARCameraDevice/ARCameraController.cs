using System;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

/// <summary>
/// This component tests getting the latest camera image
/// and converting it to RGBA format. If successful,
/// it displays the image on the screen as a RawImage
/// and also displays information about the image.
///
/// This is useful for computer vision applications where
/// you need to access the raw pixels from camera image
/// on the CPU.
///
/// This is different from the ARCameraBackground component, which
/// efficiently displays the camera image on the screen. If you
/// just want to blit the camera texture to the screen, use
/// the ARCameraBackground, or use Graphics.Blit to create
/// a GPU-friendly RenderTexture.
///
/// In this example, we get the camera image data on the CPU,
/// convert it to an RGBA format, then display it on the screen
/// as a RawImage texture to demonstrate it is working.
/// This is done as an example; do not use this technique simply
/// to render the camera image on screen.
/// </summary>

namespace EVISION.Camera.plugin
{
    public class ARCameraController : MonoBehaviour, IDeviceCamera
    {
        [SerializeField]
        [Tooltip("The ARCameraManager which will produce frame events.")]
        ARCameraManager m_CameraManager;

        /// <summary>
        /// Get or set the <c>ARCameraManager</c>.
        /// </summary>
        public ARCameraManager cameraManager
        {
            get { return m_CameraManager; }
            set { m_CameraManager = value; }
        }

        Texture2D camTexture;

        public unsafe void GetScreenShot()
        {
            // Attempt to get the latest camera image. If this method succeeds,
            // it acquires a native resource that must be disposed (see below).
            XRCameraImage image;
            if (!cameraManager.TryGetLatestImage(out image))
            {
                return;
            }

            // Once we have a valid XRCameraImage, we can access the individual image "planes"
            // (the separate channels in the image). XRCameraImage.GetPlane provides
            // low-overhead access to this data. This could then be passed to a
            // computer vision algorithm. Here, we will convert the camera image
            // to an RGBA texture and draw it on the screen.

            // Choose an RGBA format.
            // See XRCameraImage.FormatSupported for a complete list of supported formats.
            var format = TextureFormat.RGBA32;

            if (camTexture == null || camTexture.width != image.width || camTexture.height != image.height)
            {
                camTexture = new Texture2D(image.width, image.height, format, false);
            }

            // Convert the image to format, flipping the image across the Y axis.
            // We can also get a sub rectangle, but we'll get the full image here.
            var conversionParams = new XRCameraImageConversionParams(image, format, CameraImageTransformation.MirrorY);

            // Texture2D allows us write directly to the raw texture data
            // This allows us to do the conversion in-place without making any copies.
            var rawTextureData = camTexture.GetRawTextureData<byte>();
            try
            {
                image.Convert(conversionParams, new IntPtr(rawTextureData.GetUnsafePtr()), rawTextureData.Length);
            }
            finally
            {
                // We must dispose of the XRCameraImage after we're finished
                // with it to avoid leaking native resources.
                image.Dispose();
            }

            // Apply the updated texture data to our texture
            camTexture.Apply();
        }

        public unsafe void GetCameraImage()
        {
            XRCameraImage image;
            if (!cameraManager.TryGetLatestImage(out image))
                return;

            var conversionParams = new XRCameraImageConversionParams
            {
                // Get the entire image
                inputRect = new RectInt(0, 0, image.width, image.height),

                // Downsample by 2
                outputDimensions = new Vector2Int(image.width / 2, image.height / 2),

                // Choose RGBA format
                outputFormat = TextureFormat.RGBA32,

                // Flip across the vertical axis (mirror image)
                transformation = CameraImageTransformation.MirrorY
            };

            // See how many bytes we need to store the final image.
            int size = image.GetConvertedDataSize(conversionParams);

            // Allocate a buffer to store the image
            var buffer = new NativeArray<byte>(size, Allocator.Temp);

            // Extract the image data
            image.Convert(conversionParams, new IntPtr(buffer.GetUnsafePtr()), buffer.Length);

            // The image was converted to RGBA32 format and written into the provided buffer
            // so we can dispose of the CameraImage. We must do this or it will leak resources.
            image.Dispose();

            // At this point, we could process the image, pass it to a computer vision algorithm, etc.
            // In this example, we'll just apply it to a texture to visualize it.

            // We've got the data; let's put it into a texture so we can visualize it.
            camTexture = new Texture2D(
                conversionParams.outputDimensions.x,
                conversionParams.outputDimensions.y,
                conversionParams.outputFormat,
                false);

            camTexture.LoadRawTextureData(buffer);
            camTexture.Apply();

            // Done with our temporary data
            buffer.Dispose();
        }

        public Texture2D TakeScreenShot()
        {
            GetScreenShot();

            switch (Screen.orientation)
            {
                case ScreenOrientation.Portrait:
                    camTexture = TextureTools.RotateTexture(camTexture, -90);
                    break;
                case ScreenOrientation.PortraitUpsideDown:
                    camTexture = TextureTools.RotateTexture(camTexture, 90);
                    break;
                case ScreenOrientation.LandscapeRight:
                    camTexture = TextureTools.RotateTexture(camTexture, 180);
                    break;
            }
            return camTexture;
        }

        public void SwitchCamera()
        {
            throw new NotImplementedException();
        }

        public void SaveScreenShot(Texture2D snap)
        {
            string name = string.Format("{0}_Capture{1}.png", Application.productName, "{0}");
            Debug.Log("Permission result: " + NativeGallery.SaveImageToGallery(snap, "e-vision", name));
        }

        public void SetCamera(DeviceCamera.Cameras camTexture)
        {
            //throw new NotImplementedException();
        }

        public void Tick()
        {
            //throw new NotImplementedException();
        }
    }

}
