namespace FrostweepGames.Plugins.GoogleCloud.Vision
{
    public class Constants
    {
        internal const string IMAGES_ANNOTATE_REQUEST_URL = "https://vision.googleapis.com/v1/images:annotate";

        internal const string FILES_ASYNC_BATCH_ANNOTATE_REQUEST_URL = "https://vision.googleapis.com/v1/files:asyncBatchAnnotate";

        internal const string LOCATIONS_OPERATIONS_GET_REQUEST_URL = "https://vision.googleapis.com/v1/{name=locations/*/operations/*}";

        internal const string OPERATIONS_GET_REQUEST_URL = "https://vision.googleapis.com/v1/{name=operations/*}";
        internal const string OPERATIONS_CANCEL_REQUEST_URL = "https://vision.googleapis.com/v1/{name=operations/**}:cancel";
        internal const string OPERATIONS_DELETE_REQUEST_URL = "https://vision.googleapis.com/v1/{name=operations/**}";
        internal const string OPERATIONS_LIST_REQUEST_URL = "https://vision.googleapis.com/v1/{name}";


        internal const string API_KEY_PARAM = "?key=";


        internal const string GC_API_KEY = ""; // Google Cloud API Key. Only for test! Use your own API Key in Live!
    }
}