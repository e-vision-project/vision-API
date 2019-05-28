﻿using System;

namespace FrostweepGames.Plugins.GoogleCloud.SpeechRecognition
{
    [Serializable]
    public class RecognitionAudio
    {
        public string content = string.Empty;
     //   public string uri = string.Empty;
    }

    [Serializable]
    public class RecognitionConfig
    {
        public string encoding; //[Required] Encoding of audio data sent in all RecognitionAudio messages. 
        public int sampleRateHertz; //[Required] Sample rate in Hertz of the audio data sent in all RecognitionAudio messages. 
        public string languageCode; //[Optional] The language of the supplied audio as a BCP-47 language tag.
        public int maxAlternatives; //[Optional] Maximum number of recognition hypotheses to be returned. valid 0-30
        public bool profanityFilter; //[Optional] If set to true, the server will attempt to filter out profanities, replacing all but the initial character in each filtered word with asterisks, e.g. "f***". If set to false or omitted, profanities won't be filtered out. 
        public SpeechContext[] speechContexts = new SpeechContext[0];//[Optional] A means to provide context to assist the speech recognition. 
        public bool enableWordTimeOffsets;
    }

    [Serializable]
    public class SpeechContext
    {
        public string[] phrases = new string[0];
    }

    [Serializable]
    public class RecognitionResponse
    {
        public SpeechRecognitionResult[] results = new SpeechRecognitionResult[0];
    }

    [Serializable]
    public class LongRunningRecognizeResponse
    {
        public string @type;
        public SpeechRecognitionResult[] results;
        public string totalBilledTime;
}

    [Serializable]
    public class OperationResponse
    {
        public string name;
        public Metadata metadata;
        public bool done;
        public object error;
        public LongRunningRecognizeResponse response;
    }

    [Serializable]
    public class Metadata
    {
        public string @type;
        public int progressPercent;
        public DateTime startTime;
        public DateTime lastUpdateTime;
    }

    [Serializable]
    public class OperationLongRecognizeResponse
    {
        public string name;
    }

    [Serializable]
    public class RecognitionRequest
    {
        public RecognitionConfig config = new RecognitionConfig();
        public RecognitionAudio audio = new RecognitionAudio();
    }

    [Serializable]
    public class LongRunningRecognizeMetadata
    {
        public int progressPercent;
        public string startTime;  // Timestamp format
        public string lastUpdateTime;  // Timestamp format
    }


    [Serializable]
    public class SpeechRecognitionAlternative
    {
        public string transcript;
        public double confidence;
        public WordInfo[] words;

    }

    [Serializable]
    public class SpeechRecognitionResult
    {
        public SpeechRecognitionAlternative[] alternatives = new SpeechRecognitionAlternative[0];
    }

    [Serializable]
    public class WordInfo
    {
        public string startTime;
        public string endTime;
        public string word;
    }
}