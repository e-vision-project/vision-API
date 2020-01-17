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


    public class PublicServiceGCAnnotation : MonoBehaviour, IAnnotate
    {
        private GCVision _gcVision;
        private string textAnnotation = null;
        private bool annotationCompleted = false;
        private string imageData;

        [Header("Rescale Image")]
        public bool RescaleInput = false;
        [Tooltip("The resolution to be scaled")]
        public Vector2 scaleResolution;

        public bool displayBoundingBox;

        private Texture2D temp_image;

        #region IAnnotate callbacks

        public IEnumerator PerformAnnotation(Texture2D snap)
        {
            if (RescaleInput)
            {
                GenericUtils.ScaleTexture(snap, (int)scaleResolution.x, (int)scaleResolution.y);
                Debug.Log("rescaled final: " + snap.width + "," + snap.height);
            }
            //copy image
            temp_image = snap;
            // Convert to base64 encoding.
            string _selectedImageData = ImageConvert.Convert(snap);
            if (PublicServiceManager.category == (int)Enums.PServiceCategories.face)
            {
                // set features types
                var featureTypes = new List<Enumerators.FeatureType>()
                {
                    Enumerators.FeatureType.FACE_DETECTION
                };
                AnnotateImage(_selectedImageData, featureTypes);
            }
            else
            {
                // set features types
                var featureTypes = new List<Enumerators.FeatureType>()
                {
                    Enumerators.FeatureType.DOCUMENT_TEXT_DETECTION
                };
                AnnotateImage(_selectedImageData, featureTypes);
            }

            while (annotationCompleted != true)
            {
                yield return null;
            }
        }

        public T GetAnnotationResults<T>() where T : class
        {
            return textAnnotation as T;
        }

        #endregion


        #region Cloud vision callbacks

        public void AnnotateImage(string imageData, List<Enumerators.FeatureType> featureTypes)
        {
            //flag showing if annotation process has ended.
            annotationCompleted = false;

            var features = new List<Feature>();

            foreach (var feat in featureTypes)
            {
                features.Add(new Feature() { maxResults = 100, type = feat.ToString() });
            }

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
            textAnnotation = "GCFAILED";
            //Invoke onAnnotationFailed
            EventCamManager.onAnnotationFailed?.Invoke();
        }

        private void _gcVision_AnnotateSuccessEvent(VisionResponse arg1, long arg2)
        {
            try
            {
                switch (PublicServiceManager.category)
                {
                    case (int)Enums.PServiceCategories.document:
                        GetDocumentAnnotation(arg1);
                        break;
                    case (int)Enums.PServiceCategories.sign:
                        GetTextAnnotation(arg1);
                        break;
                    case (int)Enums.PServiceCategories.face:
                        GetFaceAnnotation(arg1);
                        break;
                    default:
                        break;
                }
                annotationCompleted = true;
            }
            catch (System.Exception)
            {
                Debug.LogError("Annotation was successfull but no responses were found");
                textAnnotation = string.Empty;
                annotationCompleted = true;
            }
        }

        private void RenderAnnotationResults(List<Vertex> Coords)
        {
            InternalTools.ProcessImage(Coords.ToArray(), ref temp_image, UnityEngine.Color.green);
            var display_img = GameObject.FindGameObjectWithTag("DISPLAY_IMAGE").GetComponent<RawImage>();
            display_img.texture = temp_image;
        }

        #endregion


        // Start is called before the first frame update
        void Start()
        {
            _gcVision = GCVision.Instance;
            _gcVision.AnnotateSuccessEvent += _gcVision_AnnotateSuccessEvent;
            _gcVision.AnnotateFailedEvent += _gcVision_AnnotateFailedEvent;
        }

        /// <summary>
        /// This method returns the OCR text located inside the biggest bounding box 
        /// received from the object_localization FeatureType.
        /// </summary>
        /// <param name="arg1">The response from google cloud vision</param>
        /// <param name="biggestBoxCoords"></param>
        /// <returns>string type</returns>
        private string GetTextAnnotation(VisionResponse arg1, List<Vertex> biggestBoxCoords)
        {
            var _desc = GetEntityInMaxBox(arg1, biggestBoxCoords);
            textAnnotation = _desc;
            return textAnnotation;
        }

        /// <summary>
        /// This method returns the full OCR text from the text_detection FeautureType.
        /// </summary>
        /// <param name="arg1">The response from google cloud vision</param>
        /// <returns>string type</returns>
        private string GetTextAnnotation(VisionResponse arg1)
        {
            textAnnotation = arg1.responses[0].fullTextAnnotation.text;
            return textAnnotation;
        }

        /// <summary>
        /// This method returns the full OCR text from the text_detection FeautureType
        /// of every ocr entity separetly.
        /// </summary>
        /// <param name="arg1">The response from google cloud vision</param>
        /// <returns>string type</returns>
        private string GetDocumentAnnotation(VisionResponse arg1)
        {
            var entities = FindNBiggestBoundingBoxes(arg1);
            foreach (var entity in entities)
            {
                textAnnotation += entity.description + ' ';
            }
            return textAnnotation;
        }

        /// <summary>
        /// This method returns the face emotions from the face_detection FeautureType
        /// of every facial detected entity separetly.
        /// </summary>
        /// <param name="arg1">The response from google cloud vision</param>
        /// <returns>string type</returns>
        private string GetFaceAnnotation(VisionResponse arg1)
        {
            textAnnotation = arg1.responses[0].faceAnnotations[0].sorrowLikelihood.ToString();
            return textAnnotation;
        }

        /// <summary>
        /// Επιστρέφει τις λέξεις / προτάσεις με τα μεγαλύτερα N bounding boxes στην εικόνα. 
        /// Σε αντίθεση με το FindBiggestBoundingBox όπου βρίσκουμε το μεγαλύτερο bounding box του αντικειμένου, 
        /// εδώ εντοπίζουμε τα μεγαλύτερα bounding boxes των αναγνωρισμένων λέξεων στην εικόνα.
        /// </summary>
        /// <param name="arg1"></param>
        /// <returns></returns>
        public List<EntityAnnotation> FindNBiggestBoundingBoxes(VisionResponse arg1)
        {
            List<double> areas = new List<double>();
            List<EntityAnnotation> entities = new List<EntityAnnotation>();
            foreach (var response in arg1.responses)
            {
                foreach (var entity in response.textAnnotations)
                {
                    var b = entity.boundingPoly.vertices[2].x - entity.boundingPoly.vertices[0].x;
                    var h = entity.boundingPoly.vertices[2].y - entity.boundingPoly.vertices[0].y;
                    var area = b * h;
                    areas.Add(area);
                    entities.Add(entity);
                }
            }

            var textEntities = new List<EntityAnnotation>();
            var max_Areas = areas.OrderByDescending(x => x).Take(10).ToList();
            for (int i = 0; i < max_Areas.Count; i++)
            {
                var entity = entities[areas.IndexOf(max_Areas[i])];
                i++;
                textEntities.Add(entity);
            }
            return textEntities;
        }


        /*
        Returns the text of every OCR entity separated with space, inside the area of the box. 
        */
        public string GetEntityInMaxBox(VisionResponse arg1, List<Vertex> verticesObj)
        {
            List<EntityAnnotation> entities = new List<EntityAnnotation>();
            string _description = "";
            foreach (var response in arg1.responses)
            {
                foreach (var entity in response.textAnnotations)
                {
                    //compare with bottom left (x,y) and top right
                    if (entity.boundingPoly.vertices[0].x > verticesObj[0].x && entity.boundingPoly.vertices[2].x < verticesObj[2].x
                        && entity.boundingPoly.vertices[0].y > verticesObj[0].y && entity.boundingPoly.vertices[2].y < verticesObj[2].y)
                    {
                        entities.Add(entity);

                        //Display Bounding box
                        if (displayBoundingBox)
                        {
                            InternalTools.ProcessImage(entity.boundingPoly.vertices, ref temp_image, UnityEngine.Color.red);
                            var display_img = GameObject.FindGameObjectWithTag("DISPLAY_IMAGE").GetComponent<RawImage>();
                            display_img.texture = temp_image;
                        }
                    }
                }
            }

            foreach (var item in entities)
            {
                _description += item.description + ' ';
            }

            return _description;
        }

        private void RescaleTexture(Texture2D snap)
        {
            GenericUtils.ScaleTexture(snap, (int)scaleResolution.x, (int)scaleResolution.y);
            Debug.Log(snap.width + " ," + snap.height);
        }
    }
}
