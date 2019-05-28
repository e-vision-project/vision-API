using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

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

            AnnotateImage("https://www.google.com/search?q=parrot+images&rlz=1C1GCEU_enGR822GR822&source=lnms&tbm=isch&sa=X&ved=0ahUKEwj7pZTTpIniAhVGUxoKHQy_AQ0Q_AUIDigB&biw=1920&bih=937#imgrc=DdYqMkOVt1DSIM:");
        }

        public void AnnotateImage(string url)
        {
            var features = new List<Feature>();
            features.Add(new Feature() { maxResults = 50, type = Enumerators.FeatureType.TEXT_DETECTION });

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


        private void _gcVision_AnnotateFailedEvent(string arg1, long arg2)
        {
            Debug.Log("Error: " + arg1 + " - " + arg2);
        }

        private void _gcVision_AnnotateSuccessEvent(VisionResponse arg1, long arg2)
        {
            foreach (var response in arg1.responses)
            {
                Debug.Log(response.fullTextAnnotation.text);
            }
        }
    }
}