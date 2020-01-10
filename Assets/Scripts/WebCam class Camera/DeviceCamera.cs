using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public abstract class DeviceCamera : MonoBehaviour, IDeviceCamera
{
    #region properties 

    // Canvas UI components
    [SerializeField] protected RawImage displayImage;
    [SerializeField] protected RectTransform imageParent;
    [SerializeField] protected AspectRatioFitter imageFitter;

    // Device cameras
    protected WebCamDevice backCameraDevice;
    protected WebCamDevice activeCameraDevice;
    protected WebCamDevice frontCameraDevice;

    // WebCamTextures
    protected WebCamTexture frontCameraTexture;
    protected WebCamTexture backCameraTexture;
    protected WebCamTexture activeCameraTexture;

    // Image rotation
    protected Vector3 rotationVector = new Vector3(0f, 0f, 0f);

    // Image uvRect
    protected Rect defaultRect = new Rect(0f, 0f, 1f, 1f);
    protected Rect fixedRect = new Rect(0f, 1f, 1f, -1f);

    // Image Parent's scale
    protected Vector3 defaultScale = new Vector3(1f, 1f, 1f);
    protected Vector3 fixedScale = new Vector3(-1f, 1f, 1f);

    public enum Cameras { Front, Back, Extra };

    #endregion

    public void SetCameraProperties()
    {
        displayImage = GameObject.FindGameObjectWithTag("DISPLAY_IMAGE").GetComponent<RawImage>();
        imageParent = displayImage.GetComponentInParent<RectTransform>();
        imageFitter = displayImage.GetComponent<AspectRatioFitter>();

        // Get the device's cameras and create WebCamTextures with them
        frontCameraDevice = WebCamTexture.devices.Last();
        backCameraDevice = WebCamTexture.devices.First();

        frontCameraTexture = new WebCamTexture(frontCameraDevice.name, 1024, 768);
        backCameraTexture = new WebCamTexture(backCameraDevice.name, 1024, 768);

        // Set camera filter modes for a smoother looking image
        frontCameraTexture.filterMode = FilterMode.Trilinear;
        backCameraTexture.filterMode = FilterMode.Trilinear;

        // subscribe to camera events
        //EventCamManager.current.onNatCamConnect += SetCamera(Cameras.Back);
    }

    public abstract void SaveScreenShot(Texture2D snap, string name);
    public abstract void SetCamera(Cameras camTexture);
    public abstract void Tick();
    public abstract void SwitchCamera();
    public abstract Texture2D TakeScreenShot();
    public abstract void ConnectCamera();

    public virtual Vector2 GetCamTextureWidthHeight()
    {
        Vector2 resolution = new Vector2(activeCameraTexture.width, activeCameraTexture.height);
        return resolution;
    }
}
