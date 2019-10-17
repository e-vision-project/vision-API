using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;

namespace FrostweepGames.Plugins.GoogleCloud.VideoIntelligence
{

    #region video annotate API

    #region request model
   
    public class AnnotateVideoRequest
    {
        public List<Enumerators.Feature> features;

        /// <summary>
        /// Additional video context and/or feature-specific parameters. 
        /// </summary>
        public VideoContext videoContext;

        /// <summary>
        /// Optional location where the output (in JSON format) should be stored. 
        /// Currently, only Google Cloud Storage URIs are supported, 
        /// which must be specified in the following format: gs://bucket-id/object-id 
        /// (other URI formats return google.rpc.Code.INVALID_ARGUMENT)
        /// </summary>
        public string outputUri;
        /// <summary>
        /// Optional cloud region where annotation 
        /// should take place. Supported cloud regions: us-east1, us-west1,
        /// europe-west1, asia-east1. If no region is specified, 
        /// a region will be determined based on video file location. 
        /// </summary>
        public string locationId;
    }

    public class UriAnnotateVideoRequest : AnnotateVideoRequest
    {
        /// <summary>
        /// nput video location. Currently, only Google Cloud Storage URIs are supported, 
        /// which must be specified in the following format: gs://bucket-id/object-id (other URI formats return 
        /// </summary>
        public string inputUri;
    }

    public class ContentAnnotateVideoRequest : AnnotateVideoRequest
    {
        /// <summary>
        /// The video data bytes. If unset, the input video(s) should be specified via inputUri. If set, inputUri should be unset.
        /// </summary>
        public string inputContent;
    }


    [Serializable]
    public class VideoContext
    {
        public VideoSegment[] segments;
        public LabelDetectionConfig labelDetectionConfig;
        public ShotChangeDetectionConfig shotChangeDetectionConfig;
        public ExplicitContentDetectionConfig explicitContentDetectionConfig;
        public SpeechTranscriptionConfig speechTranscriptionConfig;
        public TextDetectionConfig textDetectionConfig;
    }

    [Serializable]
    public class VideoSegment
    {
        public string startTimeOffset;
        public string endTimeOffset;
    }

    [Serializable]
    public class LabelDetectionConfig
    {
        public Enumerators.LabelDetectionMode labelDetectionMode;

        /// <summary>
        ///  Should be used with SHOT_AND_FRAME_MODE enabled. 
        /// </summary>
        public bool stationaryCamera;

        /// <summary>
        /// Supported values: "builtin/stable" (the default if unset) and "builtin/latest".
        /// </summary>
        public string model;

        /// <summary>
        /// The confidence threshold we perform filtering on the labels from frame-level detection. 
        /// If not set, it is set to 0.4 by default. 
        /// The valid range for this threshold is [0.1, 0.9]. 
        /// Any value set outside of this range will be clipped. 
        /// Note: for best results please follow the default threshold. 
        /// We will update the default threshold everytime when we release a new model. 
        /// </summary>
        public double frameConfidenceThreshold;

        /// <summary>
        /// The confidence threshold we perform filtering on the labels from video-level and shot-level detections. 
        /// If not set, it is set to 0.3 by default. 
        /// The valid range for this threshold is [0.1, 0.9]. Any value set outside of this range will be clipped. 
        /// Note: for best results please follow the default threshold. 
        /// We will update the default threshold everytime when we release a new model. 
        /// </summary>
        public double videoConfidenceThreshold;
    }

    [Serializable]
    public class ShotChangeDetectionConfig
    {
        /// <summary>
        /// Supported values: "builtin/stable" (the default if unset) and "builtin/latest".
        /// </summary>
        public string model;
    }

    [Serializable]
    public class ExplicitContentDetectionConfig
    {
        /// <summary>
        /// Supported values: "builtin/stable" (the default if unset) and "builtin/latest".
        /// </summary>
        public string model;
    }

    [Serializable]
    public class SpeechTranscriptionConfig
    {
        public string languageCode;

        /// <summary>
        /// Valid values are 0-30. A value of 0 or 1 will return a maximum of one. If omitted, will return a maximum of one. 
        /// </summary>
        public int maxAlternatives;
        public bool filterProfanity;
        public SpeechContext[] speechContexts;

        /// <summary>
        /// Optional If 'true', adds punctuation to recognition result hypotheses. 
        /// This feature is only available in select languages. Setting this for requests in other languages has no effect at all. 
        /// The default 'false' value does not add punctuation to result hypotheses. NOTE: "This is currently offered as an experimental service, complimentary to all users. 
        /// In the future this may be exclusively available as a premium feature." 
        /// </summary>
        public bool enableAutomaticPunctuation;
        /// <summary>
        /// Optional For file formats, such as MXF or MKV, supporting multiple audio tracks, specify up to two tracks. Default: track 0. 
        /// </summary>
        public double[] audioTracks;
        public bool enableSpeakerDiarization;
        public int diarizationSpeakerCount;
        public bool enableWordConfidence;
    }

    [Serializable]
    public class TextDetectionConfig
    {
        public string[] languageHints;
    }

    [Serializable]
    public class SpeechContext
    {
        public string[] phrases;
    }

    #endregion

    #region response model


    public class AnnotateResponse
    {
        public string name;
    }

    [Serializable]
    public class Status
    {
        public double code;
        public string message;
        public object[] details;
    }

    #endregion

    #endregion

    #region operations API

    [Serializable]
    public class Operation
    {
        public string name;
        public AnnotateVideoProgress metadata;
        public bool done;
        public Status error;
        public AnnotateVideoResponse response;
    }

    [Serializable]
    public class ListOperationResponse
    {
        public Operation[] operations;
        public string nextPageToken;
    }

    #endregion

    #region operation response types

    [Serializable]
    public class AnnotateVideoProgress
    {
        public VideoAnnotationProgress[] annotationProgress;
    }

    [Serializable]
    public class AnnotateVideoResponse
    {
        public VideoAnnotationResults[] annotationResults;
    }

    [Serializable]
    public class VideoAnnotationProgress
    {
        public string inputUri;
        public double progressPercent;
        public string startTime;
        public string updateTime;
    }

    [Serializable]
    public class VideoAnnotationResults
    {
        public string input_uri;
        public LabelAnnotation[] segmentLabelAnnotations;
        public LabelAnnotation[] shotLabelAnnotations;
        public LabelAnnotation[] frameLabelAnnotations;
        public VideoSegment[] shotAnnotations;
        public ExplicitContentAnnotation explicitAnnotation;
        public SpeechTranscription[] speechTranscriptions;
        public TextAnnotation[] textAnnotations;
        public ObjectTrackingAnnotation[] objectAnnotations;
        public Status error;
    }

    [Serializable]
    public class LabelAnnotation
    {
        public Entity entity;
        public Entity[] categoryEntities;
        public LabelSegment[] segments;
        public LabelFrame[] frames;
    }

    [Serializable]
    public class ExplicitContentAnnotation
    {
        public ExplicitContentFrame[] frames;
    }

    [Serializable]
    public class SpeechTranscription
    {
        public SpeechRecognitionAlternative[] alternatives;
        public string languageCode;
    }

    [Serializable]
    public class TextAnnotation
    {
        public string text;
        public TextSegment[] segments;
    }

    [Serializable]
    public class ObjectTrackingAnnotation
    {
        public Entity entity;
        public float confidence;
        public ObjectTrackingFrame[] frames;
        public VideoSegment segment;
        public long trackId;
    }

    [Serializable]
    public class Entity
    {
        public string entityId;
        public string description;
        public string languageCode;
    }

    [Serializable]
    public class LabelSegment
    {
        public VideoSegment segment;
        public float confidence;
    }

    [Serializable]
    public class LabelFrame
    {
        public string timeOffset;
        public float confidence;
    }

    [Serializable]
    public class ExplicitContentFrame
    {
        public string timeOffset;
        public Enumerators.Likelihood pornographyLikelihood;
    }

    [Serializable]
    public class SpeechRecognitionAlternative
    {
        public string transcript;
        public float confidence;
        public WordInfo[] words;
    }

    [Serializable]
    public class TextSegment
    {
        public VideoSegment segment;
        public float confidence;
        public TextFrame[] frames;
    }

    [Serializable]
    public class ObjectTrackingFrame
    {
        public Entity entity;
        public float confidence;
        public ObjectTrackingFrame[] frames;
        public VideoSegment segment;
        public long trackId;
    }

    [Serializable]
    public class WordInfo
    {
        public string startTime;
        public string endTime;
        public string word;
        public float confidence;
        public int speakerTag;
    }

    [Serializable]
    public class TextFrame
    {
        public NormalizedBoundingPoly rotatedBoundingBox;
        public string timeOffset;
    }

    [Serializable]
    public class NormalizedBoundingPoly
    {
        public NormalizedVertex[] vertices;
    }

    [Serializable]
    public class NormalizedVertex
    {
        public float x;
        public float y;
    }
    #endregion
}