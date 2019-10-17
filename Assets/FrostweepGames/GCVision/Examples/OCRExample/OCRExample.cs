using FrostweepGames.Plugins.GoogleCloud.Vision.Helpers;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

namespace FrostweepGames.Plugins.GoogleCloud.Vision.Examples
{
    public class OCRExample : MonoBehaviour
    {
        private GCVision _gcVision;

        private Texture2D _loadedTexture;

        public UnityEngine.Color color;

        public RawImage workingTexureImage;
        public Button annotateButton;
        public InputField urlInputField;
        public UnityEngine.UI.Image statusImage;

        private void Start()
        {
            _gcVision = GCVision.Instance;

            _gcVision.AnnotateSuccessEvent += _gcVision_AnnotateSuccessEvent;
            _gcVision.AnnotateFailedEvent += _gcVision_AnnotateFailedEvent;


            annotateButton.onClick.AddListener(AnnotateButtonOnClickHandler);

            urlInputField.text = "http://fabricjs.com/article_assets/2_7.png"; // dafault value     
        }

        private void AnnotateButtonOnClickHandler()
        {
            statusImage.color = UnityEngine.Color.yellow;
            StartCoroutine(LoadImageFromLink(urlInputField.text));
        }

        public void AnnotateImage(string url)
        {
            var features = new List<Feature>();
            features.Add(new Feature() { maxResults = 50, type = Enumerators.FeatureType.TEXT_DETECTION.ToString() });

            _gcVision.Annotate(new List<AnnotateRequest>()
            {
                new AnnotateRequest()
                {
                    image = new Image()
                    {
                        source = new ImageSource()
                        {
                            imageUri = url,
                            gcsImageUri = string.Empty
                        },
                        content = string.Empty
                    },
                    context = new ImageContext()
                    {
                        languageHints = new string[]
                        {
                            "english"
                        },
                    },
                    features = features
                }
            });
        }

        private IEnumerator LoadImageFromLink(string uri)
        {
            UnityWebRequest _webRequest = UnityWebRequest.Get(uri);

            // _webRequest.SendWebRequest(); // use it in the new Unity versions instead of:
            var asyncOperation = _webRequest.SendWebRequest();

            while (!asyncOperation.isDone)
                yield return asyncOperation;

            if (_loadedTexture != null)
                Destroy(_loadedTexture);

            _loadedTexture = new Texture2D(2, 2, TextureFormat.ARGB32,  false, true);
            _loadedTexture.LoadImage(_webRequest.downloadHandler.data);
            _loadedTexture.Apply();


            workingTexureImage.texture = _loadedTexture;

            workingTexureImage.SetNativeSize();

            AnnotateImage(uri);
        }

        private void _gcVision_AnnotateFailedEvent(string arg1, long arg2)
        {
            Debug.Log("Error: " + arg1 + " - " + arg2);

            statusImage.color = UnityEngine.Color.red;
        }

        private void _gcVision_AnnotateSuccessEvent(VisionResponse arg1, long arg2)
        {
            statusImage.color = UnityEngine.Color.green;

            foreach (var response in arg1.responses)
                foreach (var entity in response.textAnnotations)
                    InternalTools.ProcessImage(entity.boundingPoly.vertices, ref _loadedTexture, color);            
        }
    }
}