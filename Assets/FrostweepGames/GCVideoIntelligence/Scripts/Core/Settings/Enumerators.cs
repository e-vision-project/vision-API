namespace FrostweepGames.Plugins.GoogleCloud.VideoIntelligence
{
    public class Enumerators
    {
        public enum ApiType : int
        {
            UNDEFINED = 0,

            ANNOTATE,
            GET,
            LIST,
            DELETE,
            CANCEL
        }

        public enum Feature
        {
            FEATURE_UNSPECIFIED,
            LABEL_DETECTION,
            SHOT_CHANGE_DETECTION,
            EXPLICIT_CONTENT_DETECTION,
            SPEECH_TRANSCRIPTION,
            TEXT_DETECTION,
            OBJECT_TRACKING
        }

        public enum LabelDetectionMode
        {
            LABEL_DETECTION_MODE_UNSPECIFIED,
            SHOT_MODE,
            FRAME_MODE,
            SHOT_AND_FRAME_MODE
        }

        public enum Likelihood
        {
            LIKELIHOOD_UNSPECIFIED,
            VERY_UNLIKELY,
            UNLIKELY,
            POSSIBLE,
            LIKELY,
            VERY_LIKELY
        }
    }
}