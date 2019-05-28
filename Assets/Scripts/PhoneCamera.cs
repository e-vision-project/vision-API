using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PhoneCamera : MonoBehaviour
{
    private WebCamDevice[] devices;
    private WebCamTexture camTex;
    private int camIndex;
    public RawImage display;
    public Text camNumber;
    public Quaternion baseRotation;

    private void Start()
    {
        devices = WebCamTexture.devices;
        camNumber.text = devices.Length.ToString();
        if(devices.Length > 0)
        {
            camIndex = 0;
        }
    }

    private void Update()
    {
        
    }

    public void StartCamera()
    {
        if (devices.Length == 0) return;

        WebCamTexture camTex = new WebCamTexture(devices[camIndex].name);
        display.texture = camTex;

        camNumber.text = camTex.videoRotationAngle.ToString();
        float antiRotate = -(360 - camTex.videoRotationAngle);
        Quaternion quatRot = new Quaternion();
        quatRot.eulerAngles = new Vector3(0, 0, antiRotate);
        display.transform.rotation = quatRot;
        
        camTex.Play();
    }

    public void StopCamera()
    {
        display.texture = null;
        camTex.Stop();
        camTex = null;
    }
}
