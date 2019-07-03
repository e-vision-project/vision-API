using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

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

    public override void SwitchCamera()
    {
        throw new System.NotImplementedException();
    }

    public override Texture2D TakeScreenShot()
    {
        Texture2D snap = new Texture2D(activeCameraTexture.width, activeCameraTexture.height);
        snap.SetPixels(activeCameraTexture.GetPixels());
        snap.Apply();
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

    #endregion
}
