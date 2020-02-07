using UnityEngine;
using UnityEngine;
using System;
using System.Collections;

public interface IDeviceCamera
{
    Texture2D TakeScreenShot();
    void SwitchCamera();
    void SaveScreenShot(Texture2D snap, string fileName);
    void ConnectCamera();
    void Tick();
}
