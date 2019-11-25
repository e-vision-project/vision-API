using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IExternalCamera
{
    Texture2D GetScreenShot();
    void SaveScreenshot();
    void SetCamera(string url);
}
