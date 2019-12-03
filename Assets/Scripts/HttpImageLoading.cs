using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class HttpImageLoading : MonoBehaviour
{
    public Texture2D screenshotTex;
    [SerializeField] private string imageUrl;

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
        yield return StartCoroutine(GetURLTexture());
    }

    public IEnumerator GetURLTexture()
    {
        var x = UnityWebRequestTexture.GetTexture(imageUrl);
        x.certificateHandler = new BypassCertificate();

        yield return x.SendWebRequest();
        Debug.Log("Done");
        screenshotTex = DownloadHandlerTexture.GetContent(x);
    }

    public Texture2D GetUrlTextureObsolete()
    {
        WWW www = new WWW(imageUrl);
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
