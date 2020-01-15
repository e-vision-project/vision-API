using EVISION.Camera.plugin;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class HttpImageLoading : MonoBehaviour
{
    public Texture2D screenshotTex;
    public string imageUrl;
    private readonly string photosFolder = "http://192.168.1.254/DCIM/PHOTO/";
    public bool snapTaken = false;
    public bool textureLoaded = false;
    public bool cameraConnected = true;
    private bool firstConnection = true;
    public GameObject displayImage;

    // Start is called before the first frame update
    void Start()
    {
        screenshotTex = new Texture2D(1920, 1080);
        displayImage.SetActive(false);
        GameObject.FindGameObjectWithTag("DISPLAY_IMAGE_EXTERNAL").SetActive(true);
        cameraConnected = true;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public IEnumerator LoadTextureFromImage()
    {
        if(firstConnection){
            RemoveAllPhotos();
        }
        yield return StartCoroutine(TakePhotoRequest());
        var photoNames = GetPhotoNames(photosFolder); // get all photos contained in the server.
        var photoName = GetCapturedPhotoName(photoNames); // get the newest one.
        imageUrl = photosFolder + photoName;
        yield return StartCoroutine(GetURLTexture(imageUrl));
    }

    private void RemoveAllPhotos()
    {
        var savedPhotos = GetPhotoNames(photosFolder);
        savedPhotos.RemoveAll(s => s.Contains("Remove"));
        for (int i = 0; i < savedPhotos.Count; i++)
        {
            var x = savedPhotos[i].Replace("<b>", string.Empty);
            x = x.Replace("</b>", string.Empty);
            StartCoroutine(SendRemovePhotoRequest(photosFolder + x)); 
        }
        firstConnection = false;
    }

    public IEnumerator GetURLTexture(string url)
    {
        var x = UnityWebRequestTexture.GetTexture(url);
        yield return x.SendWebRequest();
        if (x.isNetworkError || x.isHttpError)
        {
            Debug.Log(x.error);
        }
        else if(x.isDone)
        {
            // Get downloaded asset bundle
            screenshotTex = DownloadHandlerTexture.GetContent(x);
            //screenshotTex = TextureTools.RotateTexture(screenshotTex, 180);
            displayImage.SetActive(true);
            var y = GameObject.FindGameObjectWithTag("DISPLAY_IMAGE_HTTP").GetComponent<RawImage>();
            y.texture = screenshotTex;
            Debug.Log("starting resolution: " + screenshotTex.width + "," + screenshotTex.height);
            textureLoaded = true;
        }
    }

    public string GetCapturedPhotoName(List<string> names)
    {
            // every photo has also a remove text on the client, therefore the last list item is a remove text.
            names.RemoveAll(s => s.Contains("Remove"));
            int index = names.Count - 1;
            var x = names[0].Replace("<b>", string.Empty);
            x = x.Replace("</b>", string.Empty);
            return x;
    }

    private List<string> GetPhotoNames(string url)
    {
        WebRequest request = WebRequest.Create(url);
        WebResponse response = request.GetResponse();
        Regex regex = new Regex("<a href=\".*\">(?<name>.*)</a>");
        using (var reader = new StreamReader(response.GetResponseStream()))
        {
            string result = reader.ReadToEnd();
            MatchCollection matches = regex.Matches(result);
            if (matches.Count == 0)
            {
                Debug.Log("No files inside the folder found");
            }
            List<string> names = new List<string>();
            foreach (Match match in matches)
            {
                if (!match.Success) { continue; }
                //Debug.Log("folder Match: " + match.Groups["name"]);
                var name = match.Groups["name"].Value;
                names.Add(name);
            }

            return names;
        }
    }

    public IEnumerator SendRemovePhotoRequest(string url)
    {
        var x = UnityWebRequest.Delete(url);
        x.certificateHandler = new BypassCertificate();
        yield return x.SendWebRequest();
    }

    public IEnumerator TakePhotoRequest()
    {
        var x = UnityWebRequest.Get("http://192.168.1.254/?custom=1&cmd=1001");
        yield return x.SendWebRequest();
        if (x.isNetworkError || x.isHttpError)
        {
            cameraConnected = false;
            EventCamManager.onExternalCamError?.Invoke();
        }
        else
        {
            snapTaken = true;
            cameraConnected = true;
            Handheld.Vibrate();
        }
    }

    public IEnumerator SetCameraModeRequest(int mode)
    {
        if(mode > 1 && mode < 0) { Debug.Log("valid mode input is 0 or 1."); yield return null; }
        var x = UnityWebRequest.Get(String.Format("http://192.168.1.254/?custom=1&cmd=3001&par={0}",mode));
        yield return x.SendWebRequest();
        if (x.isNetworkError || x.isHttpError)
        {
            Debug.Log(x.error);
        }
        else
        {
            // Get downloaded asset bundle
        }
    }

    public IEnumerator SetRecordingRequest(int mode)
    {
        if (mode > 1 && mode < 0) { Debug.Log("valid mode input is 0 or 1."); yield return null; }
        var x = UnityWebRequest.Get(String.Format("http://192.168.1.254/?custom=1&cmd=2001&par={0}", mode));
        yield return x.SendWebRequest();
        if (x.isNetworkError || x.isHttpError)
        {
            Debug.Log(x.error);
        }
    }

    public IEnumerator GetConnectionStatus()
    {
        var x = UnityWebRequest.Get("http://192.168.1.254/?custom=1&cmd=3027");
        yield return x.SendWebRequest();
        if (x.isHttpError || x.isNetworkError)
        {
            Debug.Log("camera connection error :" + x.error);
            cameraConnected = false;
            EventCamManager.onExternalCamError?.Invoke();
        }
        else if (x.isDone)
        {
            Debug.Log("camera connected");
            cameraConnected = true;
        }
    }

    public IEnumerator LoadURLTexture(string url)
    {
        UnityWebRequest x = UnityWebRequestTexture.GetTexture(url);
        yield return x.SendWebRequest();
        if (!x.isNetworkError)
        {
            Texture2D texture = DownloadHandlerTexture.GetContent(x);
            screenshotTex.LoadImage(x.downloadHandler.data);
        }
    }
    public Texture2D GetUrlTextureObsolete(string url)
    {
        WWW www = new WWW(url);
        Debug.Log("start");
        while (www.isDone == false)
        {
            continue;
        }
        var texture = new Texture2D(1, 1, TextureFormat.ARGB32, false);
        www.LoadImageIntoTexture(texture);
        Debug.Log("Done");
        return texture;
    }
}
