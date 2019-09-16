using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FrostweepGames.Plugins.GoogleCloud.Vision;

namespace EVISION.Camera.plugin
{
    public class CloudVisionAnnotation : MonoBehaviour, IAnnotate
    {
        private GCVision _gcVision;
        private string textAnnotation = null;
        private bool annotationCompleted = false;

        #region IAnnotate callbacks

        public IEnumerator PerformAnnotation(Texture2D snap)
        {
            string _selectedImageData = ImageConvert.Convert(snap);
            AnnotateImage(_selectedImageData);
            while (annotationCompleted != true)
            {
                yield return null;
            }
        }

        public string GetAnnotationText()
        {
            return textAnnotation;
        }

        #endregion

        // Start is called before the first frame update
        void Start()
        {
            _gcVision = GCVision.Instance;
            _gcVision.AnnotateSuccessEvent += _gcVision_AnnotateSuccessEvent;
            _gcVision.AnnotateFailedEvent += _gcVision_AnnotateFailedEvent;
        }

        public void AnnotateImage(string imageData)
        {
            //flag showing if annotation process has ended.
            annotationCompleted = false;

            var features = new List<Feature>();
            features.Add(new Feature() { maxResults = 100, type = Enumerators.FeatureType.TEXT_DETECTION });

            var img = new FrostweepGames.Plugins.GoogleCloud.Vision.Image();
            if (string.IsNullOrEmpty(imageData))
            {
                img.source = new ImageSource()
                {
                    imageUri = string.Empty,
                    gcsImageUri = string.Empty,
                };

                img.content = string.Empty;
            }
            else
            {
                img.source = new ImageSource()
                {
                    imageUri = string.Empty,
                    gcsImageUri = string.Empty,
                };

                img.content = imageData;
            }

            _gcVision.Annotate(new List<AnnotateRequest>()
            {
                new AnnotateRequest()
                {
                    image = img,
                    context = new ImageContext()
                    {
                        cropHintsParams = new CropHintsParams()
                        {
                            aspectRatios = new double[] { 1, 2 }
                        },
                        languageHints = new string[]
                        {
                            "english"
                        },
                        latLongRect = new LatLongRect()
                        {
                            maxLatLng = new LatLng()
                            {
                                latitude = 0,
                                longitude = 0
                            },
                            minLatLng = new LatLng()
                            {
                                latitude = 0,
                                longitude = 0
                            }
                        }
                    },
                    features = features
                }
            });
        }


        private void _gcVision_AnnotateFailedEvent(string arg1, long arg2)
        {
            Debug.Log("Error: " + arg1 + " - " + arg2);
            Debug.Log("e-vision platform logs: " + "Annotation failed. Check internet connection");
            annotationCompleted = true;
        }

        private void _gcVision_AnnotateSuccessEvent(VisionResponse arg1, long arg2)
        {
            Debug.Log("e-vision platform logs: " + "Annotation successfull");

            // ADD TRY CATCH HANDLING
            try
            {
                textAnnotation = arg1.responses[0].fullTextAnnotation.text;
                annotationCompleted = true;
            }
            catch (System.Exception)
            {
                Debug.LogError("Annotation was successfull but no responses were found");
                annotationCompleted = true;
                textAnnotation = string.Empty;
            }
        }
    }
}
