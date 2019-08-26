using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IDeviceCamera
{
    Texture2D TakeScreenShot();
    void SwitchCamera();
    void SaveScreenShot(Texture2D snap);
    void SetCamera(DeviceCamera.Cameras camTexture);
    void Tick();
}
