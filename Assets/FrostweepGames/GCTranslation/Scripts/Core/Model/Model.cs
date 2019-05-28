using System.Collections.Generic;

namespace FrostweepGames.Plugins.GoogleCloud.Translation
{
    #region requests

    public class TranslationRequest
    {
        public string q;
        public string source;
        public string target;
        public string format;
        public string model;
    }

    public class DetectLanguageRequest
    {
        public string q;
    }

    public class LanguagesRequest
    {
        public string target;
        public string model;
    }

    #endregion

    #region models

    public class TranslateTextResponseList
    {
        public TranslateTextResponseTranslation[] translations;
    }

    public class TranslateTextResponseTranslation
    {
        public string detectedSourceLanguage;
        public string translatedText;
    }


    public class DetectLanguageResponseList
    {
        public List<List<DetectListValuie>> detections;
    }

    public class DetectListValuie
    {
        public string language;
        public bool isReliable;
        public float confidence;
    }


    public class GetSupportedLanguagesResponseList
    {
        public GetSupportedLanguagesResponseLanguage[] languages;
    }

    public class GetSupportedLanguagesResponseLanguage
    {
        public string language;
        public string name;
    }

    #endregion

    #region responses

    public class TranslationResponse
    {
        public TranslateTextResponseList data;
    }

    public class DetectLanguageResponse
    {
        public DetectLanguageResponseList data;
    }

    public class LanguagesResponse
    {
        public GetSupportedLanguagesResponseList data;
    }
    #endregion
}