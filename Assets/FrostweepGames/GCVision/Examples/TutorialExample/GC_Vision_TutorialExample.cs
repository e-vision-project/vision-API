using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;

namespace FrostweepGames.Plugins.GoogleCloud.Vision.Examples
{
    public class GC_Vision_TutorialExample : MonoBehaviour
    {
        private GCVision _gcVision;

        private void Start()
        {
            _gcVision = GCVision.Instance;

            _gcVision.AnnotateSuccessEvent += _gcVision_AnnotateSuccessEvent;
            _gcVision.AnnotateFailedEvent += _gcVision_AnnotateFailedEvent;


            AnnotateImage("https://i.stack.imgur.com/vrkIj.png");

            /*

            // encoding from texture2d from resources
            AnnotateImage(content: Convert.ToBase64String(Resources.Load<Texture2D>("image").EncodeToPNG()));

            // encoding from file from streaming assets
            AnnotateImage(content: Convert.ToBase64String(File.ReadAllBytes(Path.Combine(Application.streamingAssetsPath,"image.png"))));

            // encoding from file from persistent data path
            AnnotateImage(content: Convert.ToBase64String(File.ReadAllBytes(Path.Combine(Application.persistentDataPath, "image.png"))));



            Texture2D texture = new Texture2D(2, 2, TextureFormat.ARGB32, false, false);

            // save file to persistent data path
            File.WriteAllBytes(Path.Combine(Application.persistentDataPath, "image.png"), texture.EncodeToPNG());

            // save file to streaming assets
            File.WriteAllBytes(Path.Combine(Application.streamingAssetsPath, "image.png"), texture.EncodeToPNG());


            */
        }

        public void AnnotateImage(string imageUri = "", string content = "", string gcsImageUri = "")
        {
            var features = new List<Feature>();
            features.Add(new Feature() { maxResults = 50, type = Enumerators.FeatureType.DOCUMENT_TEXT_DETECTION.ToString() });

            _gcVision.Annotate(new List<AnnotateRequest>()
            {
                new AnnotateRequest()
                {
                    image = new Image()
                    {
                        source = new ImageSource()
                        {
                            imageUri = imageUri,
                            gcsImageUri = gcsImageUri
                        },
                        content = content
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


        private void _gcVision_AnnotateFailedEvent(string arg1, long arg2)
        {
            Debug.Log("Error: " + arg1 + " - " + arg2);
        }

        private void _gcVision_AnnotateSuccessEvent(VisionResponse arg1, long arg2)
        {
            foreach (var response in arg1.responses)
            {
                foreach (var annotation in response.textAnnotations)
                {
                    Debug.Log(annotation.description);
                }
            }
        }
    }
}