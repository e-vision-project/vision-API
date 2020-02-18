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
    [SerializeField] private CameraClient client;


    // Start is called before the first frame update
    IEnumerator Start()
    {
        client = gameObject.GetComponent<CameraClient>();
        if (client.externalCamera)
        {
            //set to photo mode
            yield return StartCoroutine(SetRecordingRequest(1));
            StartCoroutine(TakePhotoRequest());
            RemoveAllPhotos();
        }
    }

    /// <summary>
    /// This method aims to communicate with an http server and load the newest added image in the server
    /// to a texture 2D.
    /// </summary>
    /// <returns>IEnumarator gameobject</returns>
    public IEnumerator LoadTextureFromImage()
    {
        yield return StartCoroutine(TakePhotoRequest());
        firstConnection = true;
        var photoNames = GetPhotoNames(photosFolder); // get all photos contained in the server.
        var photoName = GetCapturedPhotoName(photoNames); // get the newest one.
        imageUrl = photosFolder + photoName;
        yield return StartCoroutine(GetURLTexture(imageUrl));
    }

    /// <summary>
    /// This method finds the addresses of the files contained in the specified http server and 
    /// removes them.
    /// </summary>
    private void RemoveAllPhotos()
    {
        var savedPhotos = GetPhotoNames(photosFolder);
        if (savedPhotos.Count != 0)
        {
            savedPhotos.RemoveAll(s => s.Contains("Remove"));
            for (int i = 0; i < savedPhotos.Count; i++)
            {
                var x = savedPhotos[i].Replace("<b>", string.Empty);
                x = x.Replace("</b>", string.Empty);
                StartCoroutine(SendRemovePhotoRequest(photosFolder + x));
            }
            firstConnection = false;
        }
    }

    /// <summary>
    /// This method load an image file from a specified address of an http server
    /// to a texture 2D object.
    /// </summary>
    /// <param name="url">The url address of the file in the server</param>
    /// <returns>Ienumarator gameobject</returns>
    public IEnumerator GetURLTexture(string url)
    {
        using (var x = UnityWebRequestTexture.GetTexture(url))
        {
            yield return x.SendWebRequest();
            if (x.isNetworkError || x.isHttpError)
            {
                Debug.Log("Get url texture error" + x.error);
            }
            else if (x.isDone)
            {
                // Get downloaded asset bundle
                Texture2D temp_tex;
                temp_tex = DownloadHandlerTexture.GetContent(x);
                CameraClient.camTexture = GenericUtils.Resize(temp_tex, temp_tex.width, temp_tex.height);
                DestroyImmediate(temp_tex);
                textureLoaded = true;
            }
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

    /// <summary>
    /// This methods find and returns a list with all the file names contained in the specified url address.
    /// </summary>
    /// <param name="url">The url address to the http server</param>
    /// <returns>List of strings</returns>
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
            response.Dispose();
            return names;
        }
    }

    /// <summary>
    /// Sends a Delete request to the specified address of the http server.
    /// </summary>
    /// <param name="url">The url address to the http server</param>
    /// <returns>IEnumarator gameobject</returns>
    public IEnumerator SendRemovePhotoRequest(string url)
    {
        var x = UnityWebRequest.Delete(url);
        x.certificateHandler = new BypassCertificate();
        yield return x.SendWebRequest();
    }

    /// <summary>
    /// This method sends a predifined Get request to the http server. The address sent address
    /// can work only with the Gogloo E7-E9 wifi glasses.
    /// </summary>
    /// <returns></returns>
    public IEnumerator TakePhotoRequest()
    {
        var request = UnityWebRequest.Get("http://192.168.1.254/?custom=1&cmd=1001");
        float startTime = Time.realtimeSinceStartup;
        request.timeout = 10;

        yield return request.SendWebRequest();
        if (request.isNetworkError || request.isHttpError)
        {
            if (!firstConnection)
            {
                Debug.Log("Connection failed");
            }
        }
        else
        {
            snapTaken = true;
            cameraConnected = true;
            Handheld.Vibrate();
            request.Dispose();
        }
    }

    private void DisableHttpCamera()
    {
        EventCamManager.onExternalCamError?.Invoke();

        cameraConnected = false;
        if (GameObject.FindGameObjectWithTag("DISPLAY_IMAGE_HTTP") != null)
        {
            GameObject.FindGameObjectWithTag("DISPLAY_IMAGE_HTTP").SetActive(false);
        }
        if (GameObject.FindGameObjectWithTag("DISPLAY_IMAGE_EXTERNAL") != null)
        {
            GameObject.FindGameObjectWithTag("DISPLAY_IMAGE_EXTERNAL").SetActive(false);
        }
    }

    public IEnumerator SetCameraModeRequest(int mode)
    {
        if(mode > 1 && mode < 0) { Debug.Log("valid mode input is 0 or 1."); yield return null; }
        Debug.Log("camera mode");
        var x = UnityWebRequest.Get(String.Format("http://192.168.1.254/?custom=1&cmd=3001&par={0}",mode));
        yield return x.SendWebRequest();
        if (x.isNetworkError || x.isHttpError)
        {
            Debug.Log(x.error);
        }
        else
        {
            x.Dispose();
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
        else
        {
            cameraConnected = true;
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

    public Texture2D GetUrlTextureObsolete(string url)
    {
        WWW www = new WWW(url);
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
