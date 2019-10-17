namespace FrostweepGames.Plugins.GoogleCloud.VideoIntelligence
{
    public class Constants
    {
        internal const string POST_VIDEO_ANNOTATE_REQUEST_URL = "https://videointelligence.googleapis.com/v1/videos:annotate";


        internal const string GET_OPERATIONS_GET_REQUEST_URL = "https://videointelligence.googleapis.com/v1/operations/{name}";
        internal const string POST_OPERATIONS_CANCEL_REQUEST_URL = "https://videointelligence.googleapis.com/v1/operations/{name}:cancel";
        internal const string DELETE_OPERATIONS_DELETE_REQUEST_URL = "https://videointelligence.googleapis.com/v1/operations/";
        internal const string GET_OPERATIONS_LIST_REQUEST_URL = "https://videointelligence.googleapis.com/v1/operations";


        internal const string API_KEY_PARAM = "?key=";


        internal const string GC_API_KEY = ""; // Google Cloud API Key. Only for test! Use your own API Key in Live!
    }
}