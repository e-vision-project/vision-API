using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using FrostweepGames.Plugins.GoogleCloud.Vision;
using EVISION.Camera.plugin;

public class OCRController : MonoBehaviour
{
    private GCVision _gcVision;
    public RawImage imageRaw;

    // Start is called before the first frame update
    void Start()
    {
        _gcVision = GCVision.Instance;
        _gcVision.AnnotateSuccessEvent += _gcVision_AnnotateSuccessEvent;
        _gcVision.AnnotateFailedEvent += _gcVision_AnnotateFailedEvent;
    }


    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E)) {
            string _selectedImageData = ImageConvert.Convert(ImageConvert.GetTextureFromPath("https://ageliesergasias.gr/wp-content/uploads/2016/05/masoutis-thessnews-kouponi.jpg"));
            AnnotateImageFromURL(_selectedImageData);
        }
    }

    public void SelectImage()
    {
        imageRaw.texture = DeviceCameraController.screenshot_tex;
        string _selectedImageData = ImageConvert.Convert(DeviceCameraController.screenshot_tex);
        AnnotateImage(_selectedImageData);
    }

    public void AnnotateImage(string imageData)
    {
        var features = new List<Feature>();
        features.Add(new Feature() { maxResults = 50, type = Enumerators.FeatureType.TEXT_DETECTION.ToString() });

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


    public void AnnotateImageFromURL(string url)
    {
        var features = new List<Feature>();
        features.Add(new Feature() { maxResults = 50, type = Enumerators.FeatureType.TEXT_DETECTION.ToString() });

        _gcVision.Annotate(new List<AnnotateRequest>()
            {
                new AnnotateRequest()
                {
                    image = new FrostweepGames.Plugins.GoogleCloud.Vision.Image()
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
        Debug.Log("e-vision platform logs:" + "Annotation failed");
    }

    private void _gcVision_AnnotateSuccessEvent(VisionResponse arg1, long arg2)
    {
        Debug.Log("e-vision platform logs: " + "Annotation successfull");
        Debug.Log("e-vision platform logs: " + arg1.responses[0].fullTextAnnotation);
    }

}
