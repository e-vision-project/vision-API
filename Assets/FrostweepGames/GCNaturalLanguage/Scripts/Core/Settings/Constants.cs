namespace FrostweepGames.Plugins.GoogleCloud.NaturalLanguage
{
    public class Constants
    {
        public const string ANALYZE_ENTITIES_REQUEST_URL = "https://language.googleapis.com/v1/documents:analyzeEntities";
        public const string ANALYZE_ENTITY_SENTIMENT_REQUEST_URL = "https://language.googleapis.com/v1/documents:analyzeEntitySentiment";
        public const string ANALYZE_SENTIMENT_REQUEST_URL = "https://language.googleapis.com/v1/documents:analyzeSentiment";
        public const string ANALYZE_SYNTAX_REQUEST_URL = "https://language.googleapis.com/v1/documents:analyzeSyntax";
        public const string ANNOTATE_TEXT_REQUEST_URL = "https://language.googleapis.com/v1/documents:annotateText";
        public const string CLASSIFY_TEXT_REQUEST_URL = "https://language.googleapis.com/v1/documents:classifyText";

        public const string API_KEY_PARAM = "?key=";


        public const string GC_API_KEY = ""; // Google Cloud API Key
    }
}