using UnityEngine;
using System;

namespace FrostweepGames.Plugins.GoogleCloud.Translation
{
    public class GCTranslation : MonoBehaviour
    {
        public event Action<TranslationResponse> TranslateSuccessEvent;
        public event Action<DetectLanguageResponse> DetectLanguageSuccessEvent;
        public event Action<LanguagesResponse> GetLanguagesSuccessEvent;

        public event Action<string> TranslateFailedEvent;
        public event Action<string> DetectLanguageFailedEvent;
        public event Action<string> GetLanguagesFailedEvent;

        public event Action ContentOutOfLengthEvent;

        private static GCTranslation _Instance;
        public static GCTranslation Instance
        {
            get
            {
                if (_Instance == null)
                {
                    var obj = Resources.Load<GameObject>("Prefabs/GCTranslation");

                    if (obj != null)
                    {
                        obj.name = "[Singleton]GCTranslation";
                        _Instance = obj.GetComponent<GCTranslation>();
                    }
                    else
                        _Instance = new GameObject("[Singleton]GCTranslation").AddComponent<GCTranslation>();
                }

                return _Instance;
            }
        }


        private ServiceLocator _serviceLocator;

        private ITranslationManager _translationManager;

        public ServiceLocator ServiceLocator { get { return _serviceLocator; } }

        [Header("Prefab Object Settings")]
        public bool isDontDestroyOnLoad = false;
        public bool isFullDebugLogIfError = false;
        public bool isUseAPIKeyFromPrefab = false;

        [Header("Prefab Fields")]
        public string apiKey = string.Empty;

        private void Awake()
        {
            if (_Instance != null)
            {
                Destroy(gameObject);
                return;
            }

            if (isDontDestroyOnLoad)
                DontDestroyOnLoad(gameObject);

            _Instance = this;

            _serviceLocator = new ServiceLocator();
            _serviceLocator.InitServices();

            _translationManager = _serviceLocator.Get<ITranslationManager>();

            _translationManager.TranslateSuccessEvent += TranslateSuccessEventHandler;
            _translationManager.DetectLanguageSuccessEvent += DetectLanguageSuccessEventHandler;
            _translationManager.GetLanguagesSuccessEvent += GetLanguagesSuccessEventHandler;

            _translationManager.TranslateFailedEvent += TranslateFailedEventHandler;
            _translationManager.DetectLanguageFailedEvent += DetectLanguageFailedEventHandler;
            _translationManager.GetLanguagesFailedEvent += GetLanguagesFailedEventHandler;

            _translationManager.ContentOutOfLengthEvent += ContentOutOfLengthEventHandler;
        }

        private void Update()
        {
            if (_Instance == this)
            {
                _serviceLocator.Update();
            }
        }

        private void OnDestroy()
        {
            if (_Instance == this)
            {
                _translationManager.TranslateSuccessEvent -= TranslateSuccessEventHandler;
                _translationManager.DetectLanguageSuccessEvent -= DetectLanguageSuccessEventHandler;
                _translationManager.GetLanguagesSuccessEvent -= GetLanguagesSuccessEventHandler;

                _translationManager.TranslateFailedEvent -= TranslateFailedEventHandler;
                _translationManager.DetectLanguageFailedEvent -= DetectLanguageFailedEventHandler;
                _translationManager.GetLanguagesFailedEvent -= GetLanguagesFailedEventHandler;

                _translationManager.ContentOutOfLengthEvent -= ContentOutOfLengthEventHandler;

                _Instance = null;
                _serviceLocator.Dispose();
            }
        }

        public void Translate(TranslationRequest translationRequest)
        {
            _translationManager.Translate(translationRequest);
        }

        public void DetectLanguage(DetectLanguageRequest detectLanguageRequest)
        {
            _translationManager.DetectLanguage(detectLanguageRequest);
        }

        public void GetLanguages(LanguagesRequest languagesRequest)
        {
            _translationManager.GetLanguages(languagesRequest);
        }

        private void TranslateSuccessEventHandler(TranslationResponse value)
        {
            if (TranslateSuccessEvent != null)
                TranslateSuccessEvent(value);
        }

        private void DetectLanguageSuccessEventHandler(DetectLanguageResponse value)
        {
            if (DetectLanguageSuccessEvent != null)
                DetectLanguageSuccessEvent(value);
        }

        private void GetLanguagesSuccessEventHandler(LanguagesResponse value)
        {
            if (GetLanguagesSuccessEvent != null)
                GetLanguagesSuccessEvent(value);
        }

        private void TranslateFailedEventHandler(string value)
        {
            if (TranslateFailedEvent != null)
                TranslateFailedEvent(value);
        }

        private void DetectLanguageFailedEventHandler(string value)
        {
            if (DetectLanguageFailedEvent != null)
                DetectLanguageFailedEvent(value);
        }

        private void GetLanguagesFailedEventHandler(string value)
        {
            if (GetLanguagesFailedEvent != null)
                GetLanguagesFailedEvent(value);
        }

        private void ContentOutOfLengthEventHandler()
        {
            if (ContentOutOfLengthEvent != null)
                ContentOutOfLengthEvent();
        }
    }
}