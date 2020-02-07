using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace FrostweepGames.Plugins.GoogleCloud.Vision
{

    #region image annotate API

    #region request model

    public class AnnotateRequest
    {
        public List<Feature> features;
        public Image image;
        public ImageContext context;

    }

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
        public string type;
        public double maxResults;
        public string model; // can be builtin/stable (the default if unset) or builtin/latest
    }

    [Serializable]
    public class ImageContext
    {
        public LatLongRect latLongRect;
        public string[] languageHints;
        public CropHintsParams cropHintsParams;
        public ProductSearchParams productSearchParams;
        public WebDetectionParams webDetectionParams;
    }

    [Serializable]
    public class ProductSearchParams
    {
        public BoundingPoly boundingPoly;
        public string productSet;
        public string[] productCategories;
        public string filter;
    }

    [Serializable]
    public class WebDetectionParams
    {
        public bool includeGeoResults;
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
        public LocalizedObjectAnnotation[] localizedObjectAnnotations;
        public EntityAnnotation[] textAnnotations;
        public TextAnnotation fullTextAnnotation;
        public SafeSearchAnnotation safeSearchAnnotation;
        public ImageProperties imagePropertiesAnnotation;
        public CropHintsAnnotation cropHintsAnnotation;
        public WebDetection webDetection;
        public ProductSearchResults productSearchResults;
        public Status error;
        public ImageAnnotationContext context;

        public AnnotateImageResponse() { }
    }

    [Serializable]
    public class ProductSearchResults
    {
        public string indexTime;
        public Result[] results;
        public GroupedResult[] productGroupedResults;
    }


    [Serializable]
    public class Result
    {
        public Product product;
        public double score;
        public string image;
    }

    [Serializable]
    public class GroupedResult
    {
        public BoundingPoly boundingPoly;
        public Result[] results;
    }

    [Serializable]
    public class Product
    {
        public string name;
        public string displayName;
        public string description;
        public string productCategory;
        public KeyValue[] productLabels;
    }

    [Serializable]
    public class KeyValue
    {
        public string key;
        public string value;
    }

    [Serializable]
    public class LocalizedObjectAnnotation
    {
        public string mid,
                      languageCode,
                      name;

        public double score;

        public BoundingPoly boundingPoly;
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
        public NormalizedVertex[] normalizedVertices;
    }

    [Serializable]
    public class Vertex
    {
        public double x,
                      y;
    }

    [Serializable]
    public class NormalizedVertex
    {
        public double x,
                      y;
    }

    [Serializable]
    public class Landmark
    {
        public string type;
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

        public double confidence; // Confidence of the OCR results on the page. Range [0, 1]. 
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
        public string type;
        public bool isPrefix;
    }

    [Serializable]
    public class Block
    {
        public TextProperty property;
        public BoundingPoly boundingBox;
        public Paragraph[] paragraphs;
        public string blockType;
        public double confidence;
    }

    [Serializable]
    public class Paragraph
    {
        public TextProperty property;
        public BoundingPoly boundingBox; // http://dl4.joxi.net/drive/2018/12/21/0019/1302/1250582/82/290f0cda43.jpg
        public Word[] words;
        public double confidence;
    }

    [Serializable]
    public class Word
    {
        public TextProperty property;
        public BoundingPoly boundingBox;
        public Symbol[] symbols;
        public double confidence;
    }

    [Serializable]
    public class Symbol
    {
        public TextProperty property;
        public BoundingPoly boundingBox;
        public string text;
        public double confidence;
    }

    [Serializable]
    public class SafeSearchAnnotation
    {
        public string adult,
                                      spoof,
                                      medical,
                                      violence,
                                      racy;
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
                      importanceFraction;
    }

    [Serializable]
    public class WebDetection
    {
        public WebEntity[] webEntities;

        public WebImage[] fullMatchingImages,
                          partialMatchingImages,
                          visuallySimilarImages;

        public WebPage[] pagesWithMatchingImages;

        public WebLabel[] bestGuessLabels;
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
        public string url,
                      pageTitle;

        public double score;

        public WebImage[] fullMatchingImages,
                          partialMatchingImages;
    }

    [Serializable]
    public class WebLabel
    {
        public string label,
                      languageCode;

    }

    [Serializable]
    public class Status
    {
        public double code;
        public string message;
        public object[] details;
    }

    [Serializable]
    public class ImageAnnotationContext
    {
        public string uri;
        public double pageNumber;
    }
    #endregion

    #endregion


    #region files API

    #region request model

    [Serializable]
    public class FilesRequest
    {
        public AsyncAnnotateFileRequest requests;
    }

    [Serializable]
    public class AsyncAnnotateFileRequest
    {
        public InputConfig inputConfig;
        public Feature[] features;
        public ImageContext imageContext;
        public OutputConfig outputConfig;
    }

    [Serializable]
    public class InputConfig
    {
        public GcsSource gcsSource;
        public string mimeType;
    }

    [Serializable]
    public class OutputConfig
    {
        public GcsDestination gcsDestination;
        public double batchSize;
    }

    [Serializable]
    public class GcsSource
    {
        public string uri;
    }

    [Serializable]
    public class GcsDestination
    {
        public string uri;
    }

    #endregion

    #endregion


    #region operations API

    [Serializable]
    public class Operation
    {
        public string name;
        public object metadata;
        public bool done;
        public object error;
        public object response;
    }

    #endregion
}