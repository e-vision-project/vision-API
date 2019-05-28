using UnityEngine;
using System;
using Newtonsoft.Json;

namespace FrostweepGames.Plugins.GoogleCloud.Translation
{
    public class TranslationManager : IService, IDisposable, ITranslationManager
    {
        public event Action<TranslationResponse> TranslateSuccessEvent;
        public event Action<DetectLanguageResponse> DetectLanguageSuccessEvent;
        public event Action<LanguagesResponse> GetLanguagesSuccessEvent;

        public event Action<string> TranslateFailedEvent;
        public event Action<string> DetectLanguageFailedEvent;
        public event Action<string> GetLanguagesFailedEvent;

        public event Action ContentOutOfLengthEvent;

        private Networking _networking;
        private GCTranslation _gcTranslation;

        public void Init()
        {
            _gcTranslation = GCTranslation.Instance;

            _networking = new Networking();
            _networking.NetworkResponseEvent += NetworkResponseEventHandler;
        }

        public void Update()
        {
            _networking.Update();
        }

        public void Dispose()
        {
            _networking.NetworkResponseEvent -= NetworkResponseEventHandler;
            _networking.Dispose();
        }

        public void Translate(TranslationRequest translationRequest)
        {
            if (!CheckOnContentLength(translationRequest.q))
                return;

            SendRequestInternal(JsonConvert.SerializeObject(translationRequest), Enumerators.GoogleCloudRequestType.TRANSLATE);
        }

        public void DetectLanguage(DetectLanguageRequest detectLanguageRequest)
        {
            if (!CheckOnContentLength(detectLanguageRequest.q))
                return;

            SendRequestInternal(JsonConvert.SerializeObject(detectLanguageRequest), Enumerators.GoogleCloudRequestType.DETECT_LANGUAGE);
        }

        public void GetLanguages(LanguagesRequest languagesRequest)
        {
            SendRequestInternal(JsonConvert.SerializeObject(languagesRequest), Enumerators.GoogleCloudRequestType.GET_LANGUAGES);
        }

        private bool CheckOnContentLength(string content)
        {
            if (content.Length > Constants.MAX_LENGTH_OF_CONTENT)
            {
#if UNITY_EDITOR
                Debug.Log("Error: Text To Translate is too biggest. should be less then " + Constants.MAX_LENGTH_OF_CONTENT + " characters");
#endif
                if (ContentOutOfLengthEvent != null)
                    ContentOutOfLengthEvent();
                return false;
            }

            return true;
        }

        private void SendRequestInternal(string postData, Enumerators.GoogleCloudRequestType cloudRequestType)
        {
            string uri = string.Empty;
            NetworkEnumerators.RequestType requestType = NetworkEnumerators.RequestType.POST;

            switch (cloudRequestType)
            {
                case Enumerators.GoogleCloudRequestType.TRANSLATE:
                    uri += Constants.POST_TRANSLATE_REQUEST_URL;
                    break;
                case Enumerators.GoogleCloudRequestType.DETECT_LANGUAGE:
                    uri += Constants.POST_DETECT_REQUEST_URL;
                    break;
                case Enumerators.GoogleCloudRequestType.GET_LANGUAGES:
                    uri += Constants.GET_LANGUAGES_REQUEST_URL;
                 //   requestType = NetworkEnumerators.RequestType.GET; // on offical Google page it looks like GET request but it works correctly only with POST request
                    break;
                default: break;
            }

            if (!_gcTranslation.isUseAPIKeyFromPrefab)
                uri += Constants.API_KEY_PARAM + Constants.GC_API_KEY;
            else
                uri += Constants.API_KEY_PARAM + _gcTranslation.apiKey;

            _networking.SendRequest(uri, postData, requestType, new object[] { cloudRequestType });
        }

        private void NetworkResponseEventHandler(NetworkResponse response)
        {
            Enumerators.GoogleCloudRequestType googleCloudRequestType = (Enumerators.GoogleCloudRequestType)response.parameters[0];

            if (!string.IsNullOrEmpty(response.error))
            {
                ThrowFailedEvent(response.error, googleCloudRequestType);
            }
            else
            {
                if (response.response.Contains("error"))
                {
                    if (_gcTranslation.isFullDebugLogIfError)
                        Debug.Log(response.error + "\n" + response.response);

                    ThrowFailedEvent(response.response, googleCloudRequestType);
                }
                else
                    ThrowSuccessEvent(response.response, googleCloudRequestType);
            }
        }

        private void ThrowFailedEvent(string error, Enumerators.GoogleCloudRequestType type)
        {
            switch (type)
            {
                case Enumerators.GoogleCloudRequestType.TRANSLATE:
                    {
                        if (TranslateFailedEvent != null)
                            TranslateFailedEvent(error);
                    }
                    break;
                case Enumerators.GoogleCloudRequestType.DETECT_LANGUAGE:
                    {
                        if (DetectLanguageFailedEvent != null)
                            DetectLanguageFailedEvent(error);
                    }
                    break;
                case Enumerators.GoogleCloudRequestType.GET_LANGUAGES:
                    {
                        if (GetLanguagesFailedEvent != null)
                            GetLanguagesFailedEvent(error);
                    }
                    break;
                default: break;
            }
        }

        private void ThrowSuccessEvent(string data, Enumerators.GoogleCloudRequestType type)
        {
            switch (type)
            {
                case Enumerators.GoogleCloudRequestType.TRANSLATE:
                    {
                        if (TranslateSuccessEvent != null)
                            TranslateSuccessEvent(JsonConvert.DeserializeObject<TranslationResponse>(data));
                    }
                    break;
                case Enumerators.GoogleCloudRequestType.DETECT_LANGUAGE:
                    {
                        if (DetectLanguageSuccessEvent != null)
                            DetectLanguageSuccessEvent(JsonConvert.DeserializeObject<DetectLanguageResponse>(data));
                    }
                    break;
                case Enumerators.GoogleCloudRequestType.GET_LANGUAGES:
                    {
                        if (GetLanguagesSuccessEvent != null)
                            GetLanguagesSuccessEvent(JsonConvert.DeserializeObject<LanguagesResponse>(data));
                    }
                    break;
                default: break;
            }
        }
    }
}