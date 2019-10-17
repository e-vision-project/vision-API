using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using FrostweepGames.Plugins.GoogleCloud.Vision;
using System.Linq;
using FrostweepGames.Plugins.GoogleCloud.Vision.Helpers;

namespace EVISION.Camera.plugin
{

    /// <summary>
    /// Make calls to the Cloud vision API for getting the OCR words.
    /// For optimization check :
    /// a) GenericUtils.ScaleTexture in Perform annotation, faster with slight loss in accuracy.
    /// b) Max results in AnnotateImage, set to 50 as default.
    /// </summary>


    public class ObjectLocalization : MonoBehaviour
    {
        private GCVision _gcVision;
        private string textAnnotation = null;
        private bool localiazationCompleted = false;
        public static List<Vertex> biggestBoxCoord = new List<Vertex>();
        public static Texture2D temp_image;

        #region IAnnotate callbacks

        public IEnumerator PerformAnnotation(Texture2D snap)
        {
            // Convert to base64 encoding.
            temp_image = snap;
            string _selectedImageData = ImageConvert.Convert(snap);

            AnnotateImage(_selectedImageData);
            while (localiazationCompleted != true)
            {
                yield return null;
            }
        }

        public List<Vertex> GetAnnotationResults()
        {
            return biggestBoxCoord;
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
            localiazationCompleted = false;

            var features = new List<Feature>();
            features.Add(new Feature() { maxResults = 50, type = Enumerators.FeatureType.OBJECT_LOCALIZATION.ToString() });

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
            localiazationCompleted = true;
        }

        private void _gcVision_AnnotateSuccessEvent(VisionResponse arg1, long arg2)
        {
            Debug.Log("obj localization logs : " + "Annotation successfull");

            try
            {
                var objects = arg1.responses[0].localizedObjectAnnotations;
                localiazationCompleted = true;


                foreach (var vert in FindBiggestBoundingBox(arg1).normalizedVertices)
                {
                    //Vector2 _vertex = new Vector2((float)vert.x * temp_image.width, (float)vert.y * temp_image.height);

                    Vertex _v = new Vertex
                    {
                        x = vert.x * temp_image.width,
                        y = vert.y * temp_image.height
                    };
                    biggestBoxCoord.Add(_v);

                }

                //DisplayBoundingBox();

            }
            catch (System.Exception)
            {
                Debug.LogError("obj localization logs: Annotation was successfull but no responses were found");
                localiazationCompleted = true;
                textAnnotation = string.Empty;
            }

        }

        private static void DisplayBoundingBox()
        {
            InternalTools.ProcessImage(biggestBoxCoord.ToArray(), ref temp_image, UnityEngine.Color.green);
            var display_img = GameObject.FindGameObjectWithTag("DISPLAY_IMAGE").GetComponent<RawImage>();
            display_img.texture = temp_image;
        }

        public static BoundingPoly FindBiggestBoundingBox(VisionResponse arg1)
        {
            List<double> areas = new List<double>();
            List<LocalizedObjectAnnotation> entities = new List<LocalizedObjectAnnotation>();
            foreach (var response in arg1.responses)
            {
                foreach (var entity in response.localizedObjectAnnotations)
                {
                    var b = entity.boundingPoly.normalizedVertices[2].x - entity.boundingPoly.normalizedVertices[0].x;
                    var h = entity.boundingPoly.normalizedVertices[2].y - entity.boundingPoly.normalizedVertices[0].y;
                    var area = b * h;
                    areas.Add(area);
                    entities.Add(entity);
                }
            }

            Debug.Log(entities.Count);
            var max_Area = areas.Max();
            Debug.Log("index: " + areas.IndexOf(max_Area));
            var maxEntity = entities[areas.IndexOf(max_Area)];
            return maxEntity.boundingPoly;
        }
    }
}

