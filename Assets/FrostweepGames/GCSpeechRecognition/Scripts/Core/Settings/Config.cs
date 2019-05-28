namespace FrostweepGames.Plugins.GoogleCloud.SpeechRecognition
{
    [System.Serializable]
    public class Config 
    {
        public string name;


        public int sampleRate;
        public bool isEnabledProfanityFilter;
        public bool isEnabledWordTimeOffsets;      
        public int maxAlternatives;
        public Enumerators.AudioEncoding audioEncoding;
        public Enumerators.LanguageCode defaultLanguage;

        public Enumerators.GoogleServiceRequestType recognitionType;

        public double voiceDetectionThreshold;

        public bool useVolumeMultiplier;
        public float audioVolumeMultiplier;

        public bool enabledAndroidCertificateCheck;
        public string keySignature;
        public string packageName = "com.companyname.appname";
    }
}