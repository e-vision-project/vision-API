using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using EVISION.Camera.plugin;

public class AndroidCamera : DeviceCamera
{
    #region IDeviceCamera Methods Implementation

    public override void SetCamera(Cameras camTex)
    {
        if(activeCameraTexture != null)
        {
            activeCameraTexture.Stop();
        }

        WebCamTexture textureToUse;
        if(camTex.Equals(Cameras.Back)) { textureToUse = backCameraTexture; }
        else if(camTex.Equals(Cameras.Front)) { textureToUse = frontCameraTexture; }
        else { textureToUse = frontCameraTexture; }
        
        activeCameraTexture = textureToUse;
        activeCameraDevice = WebCamTexture.devices.FirstOrDefault(device =>
            device.name == textureToUse.deviceName);

        displayImage.texture = activeCameraTexture;
        displayImage.material.mainTexture = activeCameraTexture;

        activeCameraTexture.Play();
    }

    public override void Tick()
    {
        UpdateTick_version2();
    }

    public override void SwitchCamera()
    {
        throw new System.NotImplementedException();
    }

    public override Texture2D TakeScreenShot()
    {
        Texture2D snap = new Texture2D(activeCameraTexture.width, activeCameraTexture.height);
        snap.SetPixels(activeCameraTexture.GetPixels());
        snap.Apply();

        switch (Screen.orientation)
        {
            case ScreenOrientation.Portrait:
                snap = TextureTools.RotateTexture(snap,-90);
                Debug.Log("Portrait");
                break;
            case ScreenOrientation.PortraitUpsideDown:
                snap = TextureTools.RotateTexture(snap, 90);
                break;
            case ScreenOrientation.LandscapeRight:
                snap = TextureTools.RotateTexture(snap, 180);
                Debug.Log("Landscape right");
                break;
            case ScreenOrientation.LandscapeLeft:
                //snap = TextureTools.RotateTexture(snap, -180);
                Debug.Log("Landscape left");
                break;
        }

        return snap;
    }

    public override void SaveScreenShot(Texture2D snap)
    {
        string name = string.Format("{0}_Capture{1}.png", Application.productName, "{0}");
        Debug.Log("Permission result: " + NativeGallery.SaveImageToGallery(snap, "e-vision", name));
    }

    #endregion

    #region MonoBehaviour Callbacks

    // Start is called before the first frame update
    void OnEnable()
    {
        SetCameraProperties();
    }

    private void UpdateTick()
    {
        if (WebCamTexture.devices.Length == 0)
        {
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

    private void UpdateTick_version2()
    {
        if (activeCameraTexture.width < 100)
        {
            Debug.Log("Still waiting another frame for correct info...");
            return;
        }

        // change as user rotates iPhone or Android:

        int cwNeeded = activeCameraTexture.videoRotationAngle;
        // Unity helpfully returns the _clockwise_ twist needed
        // guess nobody at Unity noticed their product works in counterclockwise:
        int ccwNeeded = -cwNeeded;

        // IF the image needs to be mirrored, it seems that it
        // ALSO needs to be spun. Strange: but true.
        if (activeCameraTexture.videoVerticallyMirrored) ccwNeeded += 180;

        // you'll be using a UI RawImage, so simply spin the RectTransform
        imageParent.localEulerAngles = new Vector3(0f, 0f, ccwNeeded);

        float videoRatio = (float)activeCameraTexture.height / (float)activeCameraTexture.width;

        // you'll be using an AspectRatioFitter on the Image, so simply set it
        imageFitter.aspectRatio = videoRatio;

        // alert, the ONLY way to mirror a RAW image, is, the uvRect.
        // changing the scale is completely broken.
        if (activeCameraTexture.videoVerticallyMirrored)
            displayImage.uvRect = new Rect(1, 0, -1, 1);  // means flip on vertical axis
        else
            displayImage.uvRect = new Rect(0, 0, 1, 1);  // means no flip

    }

    #endregion
}
