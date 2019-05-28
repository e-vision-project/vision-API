using System;
using System.Collections.Generic;

namespace FrostweepGames.Plugins.GoogleCloud.Vision
{
    public class AnnotateRequest
    {
        public List<Feature> features;
        public Image image;
        public ImageContext context;

    }

    #region request model
    [Serializable]
    public class VisionRequest
    {
        public AnnotateImageRequest[] requests;
    }

    [Serializable]
    public class AnnotateImageRequest
    {
        public Image image;
        public Feature[] features;
        public ImageContext imageContext;
    }

    [Serializable]
    public class Image
    {
        public string content;
        public ImageSource source;
    }

    [Serializable]
    public class Feature
    {
        public Enumerators.FeatureType type;
        public double maxResults;
    }

    [Serializable]
    public class ImageContext
    {
        public LatLongRect latLongRect;
        public string[] languageHints;
        public CropHintsParams cropHintsParams;
    }

    [Serializable]
    public class ImageSource
    {
        public string gcsImageUri,
                      imageUri;
    }


    [Serializable]
    public class LatLongRect
    {
        public LatLng minLatLng,
                      maxLatLng;
    }


    [Serializable]
    public class LatLng
    {
        public double latitude,
                      longitude;
    }

    [Serializable]
    public class CropHintsParams
    {
        public double[] aspectRatios;
    }

    #endregion

    #region response model

    [Serializable]
    public class VisionResponse
    {
        public AnnotateImageResponse[] responses;
    }


    [Serializable]
    public class AnnotateImageResponse
    {
        public FaceAnnotation[] faceAnnotations;
        public EntityAnnotation[] landmarkAnnotations;
        public EntityAnnotation[] logoAnnotations;
        public EntityAnnotation[] labelAnnotations;
        public EntityAnnotation[] textAnnotations;
        public TextAnnotation fullTextAnnotation;
        public SafeSearchAnnotation safeSearchAnnotation;
        public ImageProperties imagePropertiesAnnotation;
        public CropHintsAnnotation cropHintsAnnotation;
        public WebDetection webDetection;
        public Status error;

        public AnnotateImageResponse() { }
    }

    [Serializable]
    public class FaceAnnotation
    {
        public BoundingPoly boundingPoly,
                            fdBoundingPoly;

        public Landmark[] landmarks;

        public double rollAngle,
                   panAngle,
                   tiltAngle,
                   detectionConfidence,
                   landmarkingConfidence;

        public Enumerators.Likelihood joyLikelihood,
                                      sorrowLikelihood,
                                      angerLikelihood,
                                      surpriseLikelihood,
                                      underExposedLikelihood,
                                      blurredLikelihood,
                                      headwearLikelihood;
    }

    [Serializable]
    public class BoundingPoly
    {
        public Vertex[] vertices;
    }

    [Serializable]
    public class Vertex
    {
        public double x,
                   y;
    }

    [Serializable]
    public class Landmark
    {
        public Enumerators.LandmarkType type;
        public Position position;
    }

    [Serializable]
    public class Position
    {
        public double x,
                   y,
                   z;
    }

    [Serializable]
    public class EntityAnnotation
    {
        public string mid, 
                      locale, 
                      description;
        public double score, 
                   confidence, 
                   topicality;

        public BoundingPoly boundingPoly;
        public LocationInfo[] locations;
        public Property[] properties;
    }

    [Serializable]
    public class LocationInfo
    {
        public LatLng latLng;
    }

    [Serializable]
    public class Property
    {
        public string name,
                      value,
                      uint64Value;
    }


    [Serializable]
    public class TextAnnotation
    {
        public Page[] pages;
        public string text;

    }

    [Serializable]
    public class Page
    {
        public TextProperty property;

        public double width,
                   height;

        public Block[] blocks;
    }

    [Serializable]
    public class TextProperty
    {
        public DetectedLanguage[] detectedLanguages;
        public DetectedBreak detectedBreak;
    }

    [Serializable]
    public class DetectedLanguage
    {
        public string languageCode;
        public double confidence;
    }

    [Serializable]
    public class DetectedBreak
    {
        public Enumerators.BreakType type;
        public bool isPrefix;
    }

    [Serializable]
    public class Block
    {
        public TextProperty property;
        public BoundingPoly boundingBox;
        public Paragraph[] paragraphs;
        public Enumerators.BlockType blockType;
    }

    [Serializable]
    public class Paragraph
    {
        public TextProperty property;
        public BoundingPoly boundingBox;
        public Word[] words;
    }

    [Serializable]
    public class Word
    {
        public TextProperty property;
        public BoundingPoly boundingBox;
        public Symbol[] symbols;
    }

    [Serializable]
    public class Symbol
    {
        public TextProperty property;
        public BoundingPoly boundingBox;
        public string text;
    }

    [Serializable]
    public class SafeSearchAnnotation
    {
        public Enumerators.Likelihood adult,
                                      spoof,
                                      medical,
                                      violence;
    }

    [Serializable]
    public class ImageProperties
    {
        public DominantColorsAnnotation dominantColors;
    }

    [Serializable]
    public class DominantColorsAnnotation
    {
        public ColorInfo[] colors;
    }

    [Serializable]
    public class ColorInfo
    {
        public Color color;

        public double score,
                   pixelFraction;
    }

    [Serializable]
    public class Color
    {
        public double red,
                   green,
                   blue,
                   alpha;
    }

    [Serializable]
    public class CropHintsAnnotation
    {
        public CropHint[] cropHints;
    }

    [Serializable]
    public class CropHint
    {
        public BoundingPoly boundingPoly;

        public double confidence,
                   mportanceFraction;
    }

    [Serializable]
    public class WebDetection
    {
        public WebEntity[] webEntities;

        public WebImage[] fullMatchingImages,
                          partialMatchingImages,
                          visuallySimilarImages;

        public WebPage[] pagesWithMatchingImages;
    }

    [Serializable]
    public class WebEntity
    {
        public string entityId,
                      description;

        public double score;
    }

    [Serializable]
    public class WebImage
    {
        public string url;
        public double score;
    }

    [Serializable]
    public class WebPage
    {
        public string url;
        public double score;
    }

    [Serializable]
    public class Status
    {
        public double code;
        public string message;
        public object[] details;
    }

    #endregion
}