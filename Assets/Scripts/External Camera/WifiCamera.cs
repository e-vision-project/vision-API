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
        public UnityWebRequest x;

        private Texture2D screenshotTex;
        [SerializeField] private bool logging;
        [SerializeField] private bool emptyStream;
        [SerializeField] private bool prev_connected;
        [SerializeField] private string imageUrl;

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
            Debug.Log("return 2");
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
                //Debug.Log(displayImg.texture.width + "," + displayImg.texture.height);
                //Debug.Log(UMP.VideoWidth + "," + UMP.VideoHeight);
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
            x = UnityWebRequestTexture.GetTexture(imageUrl);
            x.SendWebRequest();
            while (x.isDone == false)
            {
                Debug.Log("return 1");
                yield return null;
            }
            
            screenshotTex = DownloadHandlerTexture.GetContent(x);
            Debug.Log("DONE");
        }

        public Texture2D GetUrlTextureObsolete()
        {
            WWW www = new WWW(imageUrl);

            while(www.isDone == false)
            {
                continue;
            }
            var texture = new Texture2D(1, 1, TextureFormat.ARGB32, false);
            www.LoadImageIntoTexture(texture);
            return texture;
        }

        public IEnumerator LoadFromWeb()
        {
            throw new System.NotImplementedException();
        }
    }
}
