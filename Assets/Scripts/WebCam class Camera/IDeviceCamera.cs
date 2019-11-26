using UnityEngine;

public interface IDeviceCamera
{
    Texture2D TakeScreenShot();
    void SwitchCamera();
    void SaveScreenShot(Texture2D snap);
    void ConnectCamera();
    void Tick();
}
