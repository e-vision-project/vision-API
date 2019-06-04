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

    public void SwapCamera()
    {
        if(devices.Length > 0)
        {
            camIndex += 1;
            camIndex %= devices.Length;
        }
    }

    public void StartCamera()
    {
        if (devices.Length == 0) return;

        WebCamTexture camTex = new WebCamTexture(devices[camIndex].name,1280,720,30);
        display.texture = camTex;

        float antiRotate = -(360 - camTex.videoRotationAngle);
        Quaternion quatRot = new Quaternion();
        quatRot.eulerAngles = new Vector3(0, 0, -90);
        display.transform.rotation = quatRot;

        camTex.filterMode = FilterMode.Trilinear;

        camTex.Play();

        camNumber.text = camTex.width.ToString() +"_"+camTex.height.ToString();
    }

    public void StopCamera()
    {
        display.texture = null;
        camTex.Stop();
        camTex = null;
    }
}
