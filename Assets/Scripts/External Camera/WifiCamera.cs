using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

namespace EVISION.Camera.plugin
{
    public class WifiCamera : MonoBehaviour, IDeviceCamera
    {
        public UniversalMediaPlayer UMP;
        public RawImage displayImg;

        private Texture2D screenshotTex;
        [SerializeField] private bool logging;
        [SerializeField] private bool emptyStream;
        [SerializeField] private bool prev_connected;

        public delegate void OnEncounteredError();
        public static event OnEncounteredError onError;

        #region IDeviceCamera callbacks

        public void ConnectCamera()
        {
            Debug.Log("Connecting wifi cam");
            UMP.Play();
        }

        public Texture2D TakeScreenShot()
        {
            StartCoroutine(GetURLTexture());
            return screenshotTex;
        }

        public void SaveScreenShot(Texture2D snap)
        {
            throw new System.NotImplementedException();
        }

        public void SwitchCamera()
        {
            throw new System.NotImplementedException();
        }

        public void Tick()
        {
            if (UMP.IsPlaying && emptyStream == true)
            {
                emptyStream = false;
                Debug.Log(displayImg.texture.width + "," + displayImg.texture.height);
                Debug.Log(UMP.VideoWidth + "," + UMP.VideoHeight);
            }
        }

        #endregion

        #region MonoBehaviour Callbacks

        void Awake()
        {
            EventCamManager.onNatCamConnect += DisableCameraView;
        }

        void Update()
        {
            Tick();
        }
        #endregion

        public void DisableCameraView()
        {
            displayImg.enabled = false;
        }

        public void Reconnect(string url)
        {
            if (prev_connected)
            {
                Debug.Log("Reconnecting..");
                UMP.Path = url;
                UMP.Play();
            }
        }

        public void OnCameraError()
        {
            if (!prev_connected)
            {
                UMP.EventManager.RemoveAllEvents();
                EventCamManager.onNatCamConnect();
            }
            else if (!emptyStream && prev_connected)
            {
                UMP.EventManager.RemoveAllEvents();
                EventCamManager.onNatCamConnect();
            }
        }

        public void LogMessage(string msg)
        {
            if (logging)
            {
                Debug.Log(msg);
            }
        }

        public void SetEmptyStream(bool value)
        {
            emptyStream = value;
        }

        public void SetConnection(bool value)
        {
            prev_connected = value;
        }

        public IEnumerator GetURLTexture()
        {
            UnityWebRequest www = UnityWebRequestTexture.GetTexture("http://www.my-server.com/image.png");
            yield return www.SendWebRequest();

            if (www.isNetworkError || www.isHttpError)
            {
                Debug.Log(www.error);
            }
            else
            {
                screenshotTex = ((DownloadHandlerTexture)www.downloadHandler).texture;
            }
        }
    }
}
