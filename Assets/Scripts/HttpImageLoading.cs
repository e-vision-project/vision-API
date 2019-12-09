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
    public Texture2D loadedTex;
    public string imageUrl;
    private string photosFolder = "http://192.168.1.254/DCIM/PHOTO/";
    public bool snapTaken = false;
    public bool textureLoaded = false;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public IEnumerator LoadTextureFromImage()
    {
        yield return StartCoroutine(SendTakePhotoRequest());
        var photoName = GetPhotos(photosFolder);
        imageUrl = photosFolder + photoName;
        yield return StartCoroutine(GetURLTexture(imageUrl));
        //yield return StartCoroutine(LoadURLTexture(imageUrl));
        //yield return StartCoroutine(SendRemovePhotoRequest(imageUrl));
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
            var y = GameObject.FindGameObjectWithTag("DISPLAY_IMAGE").GetComponent<RawImage>();
            y.texture = screenshotTex;
            Debug.Log("texture loaded");
            textureLoaded = true;
        }
    }

    public string GetPhotos(string url)
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
                return "empty folder";
            }
            List<string> Names = new List<string>();
            foreach (Match match in matches)
            {
                if (!match.Success) { continue; }
                Debug.Log("folder Match: " + match.Groups["name"]);
                var  name = match.Groups["name"].Value;
                Names.Add(name);
            }
            var x = Names[0].Replace("<b>", string.Empty);
            x = x.Replace("</b>", string.Empty);
            return x;
        }
    }

    public IEnumerator SendRemovePhotoRequest(string url)
    {
        var x = UnityWebRequest.Delete(url);
        x.certificateHandler = new BypassCertificate();
        yield return x.SendWebRequest();
        Debug.Log("remove");

    }

    public IEnumerator SendTakePhotoRequest()
    {
        var x = UnityWebRequest.Get("http://192.168.1.254/?custom=1&cmd=1001");
        yield return x.SendWebRequest();
        if (x.isNetworkError || x.isHttpError)
        {
            Debug.Log(x.error);
        }
        else
        {
            // Get downloaded asset bundle
            snapTaken = true;
            Debug.Log("photo taken");
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
            Debug.Log("texture loaded");
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
