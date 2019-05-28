using System;
using UnityEngine;

namespace FrostweepGames.Plugins.GoogleCloud.NaturalLanguage
{
    public class GCNaturalLanguage : MonoBehaviour
    {
        public event Action<AnalyzeEntitiesResponse> AnalyzeEntitiesSuccessEvent;
        public event Action<AnalyzeEntitySentimentResponse> AnalyzeEntitySentimentSuccessEvent;
        public event Action<AnalyzeSentimentResponse> AnalyzeSentimentSuccessEvent;
        public event Action<AnalyzeSyntaxResponse> AnalyzeSyntaxSuccessEvent;
        public event Action<AnnotateTextResponse> AnnotateTextSuccessEvent;
        public event Action<ClassifyTextResponse> ClassifyTextSuccessEvent;

        public event Action<string> AnalyzeEntitiesFailedEvent;
        public event Action<string> AnalyzeEntitySentimentFailedEvent;
        public event Action<string> AnalyzeSentimentFailedEvent;
        public event Action<string> AnalyzeSyntaxFailedEvent;
        public event Action<string> AnnotateTextFailedEvent;
        public event Action<string> ClassifyTextFailedEvent;


        private static GCNaturalLanguage _Instance;
        public static GCNaturalLanguage Instance
        {
            get
            {
                if (_Instance == null)
                {
                    var obj = Resources.Load<GameObject>("Prefabs/GCNaturalLanguage");

                    if (obj != null)
                    {
                        obj.name = "[Singleton]GCNaturalLanguage";
                        _Instance = obj.GetComponent<GCNaturalLanguage>();
                    }
                    else
                        _Instance = new GameObject("[Singleton]GCNaturalLanguage").AddComponent<GCNaturalLanguage>();
                }

                return _Instance;
            }
        }


        private ServiceLocator _serviceLocator;

        private INaturalLanguageManager _naturalLanguagenManager;

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

            _naturalLanguagenManager = _serviceLocator.Get<INaturalLanguageManager>();

            _naturalLanguagenManager.AnalyzeEntitiesSuccessEvent += AnalyzeEntitiesSuccessEventHandler;
            _naturalLanguagenManager.AnalyzeEntitySentimentSuccessEvent += AnalyzeEntitySentimentSuccessEventHandler; 
            _naturalLanguagenManager.AnalyzeSentimentSuccessEvent += AnalyzeSentimentSuccessEventHandler;
            _naturalLanguagenManager.AnalyzeSyntaxSuccessEvent += AnalyzeSyntaxSuccessEventHandler;
            _naturalLanguagenManager.AnnotateTextSuccessEvent += AnnotateTextSuccessEventHandler;
            _naturalLanguagenManager.ClassifyTextSuccessEvent += ClassifyTextSuccessEventHandler;

            _naturalLanguagenManager.AnalyzeEntitiesFailedEvent += AnalyzeEntitiesFailedEventHandler;
            _naturalLanguagenManager.AnalyzeEntitySentimentFailedEvent += AnalyzeEntitySentimentFailedEventHandler;
            _naturalLanguagenManager.AnalyzeSentimentFailedEvent += AnalyzeSentimentFailedEventHandler;
            _naturalLanguagenManager.AnalyzeSyntaxFailedEvent += AnalyzeSyntaxFailedEventHandler;
            _naturalLanguagenManager.AnnotateTextFailedEvent += AnnotateTextFailedEventHandler;
            _naturalLanguagenManager.ClassifyTextFailedEvent += ClassifyTextFailedEventHandler;
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
                _naturalLanguagenManager.AnalyzeEntitiesSuccessEvent -= AnalyzeEntitiesSuccessEventHandler;
                _naturalLanguagenManager.AnalyzeEntitySentimentSuccessEvent -= AnalyzeEntitySentimentSuccessEventHandler;
                _naturalLanguagenManager.AnalyzeSentimentSuccessEvent -= AnalyzeSentimentSuccessEventHandler;
                _naturalLanguagenManager.AnalyzeSyntaxSuccessEvent -= AnalyzeSyntaxSuccessEventHandler;
                _naturalLanguagenManager.AnnotateTextSuccessEvent -= AnnotateTextSuccessEventHandler;
                _naturalLanguagenManager.ClassifyTextSuccessEvent -= ClassifyTextSuccessEventHandler;

                _naturalLanguagenManager.AnalyzeEntitiesFailedEvent -= AnalyzeEntitiesFailedEventHandler;
                _naturalLanguagenManager.AnalyzeEntitySentimentFailedEvent -= AnalyzeEntitySentimentFailedEventHandler;
                _naturalLanguagenManager.AnalyzeSentimentFailedEvent -= AnalyzeSentimentFailedEventHandler;
                _naturalLanguagenManager.AnalyzeSyntaxFailedEvent -= AnalyzeSyntaxFailedEventHandler;
                _naturalLanguagenManager.AnnotateTextFailedEvent -= AnnotateTextFailedEventHandler;
                _naturalLanguagenManager.ClassifyTextFailedEvent -= ClassifyTextFailedEventHandler;

                _Instance = null;
                _serviceLocator.Dispose();
            }
        }

        public string PrepareLanguage(Enumerators.Language lang)
        {
            return _naturalLanguagenManager.PrepareLanguage(lang);
        }

        public void Annotate(AnalyzeEntitiesRequest entitiesRequest)
        {
            _naturalLanguagenManager.Annotate(entitiesRequest);
        }
        public void Annotate(AnalyzeEntitySentimentRequest entitySentiment)
        {
            _naturalLanguagenManager.Annotate(entitySentiment);
        }
        public void Annotate(AnalyzeSentimentRequest sentimentRequest)
        {
            _naturalLanguagenManager.Annotate(sentimentRequest);
        }
        public void Annotate(AnalyzeSyntaxRequest syntaxRequest)
        {
            _naturalLanguagenManager.Annotate(syntaxRequest);
        }
        public void Annotate(AnnotateTextRequest textRequest)
        {
            _naturalLanguagenManager.Annotate(textRequest);
        }
        public void Annotate(ClassifyTextRequest classifyTextRequest)
        {
            _naturalLanguagenManager.Annotate(classifyTextRequest);
        }


        private void AnnotateTextFailedEventHandler(string obj)
        {
            if (AnnotateTextFailedEvent != null)
                AnnotateTextFailedEvent(obj);
        }

        private void AnalyzeSyntaxFailedEventHandler(string obj)
        {
            if (AnalyzeSyntaxFailedEvent != null)
                AnalyzeSyntaxFailedEvent(obj);
        }

        private void AnalyzeSentimentFailedEventHandler(string obj)
        {
            if (AnalyzeSentimentFailedEvent != null)
                AnalyzeSentimentFailedEvent(obj);
        }

        private void AnalyzeEntitySentimentFailedEventHandler(string obj)
        {
            if (AnalyzeEntitySentimentFailedEvent != null)
                AnalyzeEntitySentimentFailedEvent(obj);
        }

        private void AnalyzeEntitiesFailedEventHandler(string obj)
        {
            if (AnalyzeEntitiesFailedEvent != null)
                AnalyzeEntitiesFailedEvent(obj);
        }

        private void ClassifyTextFailedEventHandler(string obj)
        {
            if (ClassifyTextFailedEvent != null)
                ClassifyTextFailedEvent(obj);
        }

        private void AnnotateTextSuccessEventHandler(AnnotateTextResponse obj)
        {
            if (AnnotateTextSuccessEvent != null)
                AnnotateTextSuccessEvent(obj);
        }

        private void AnalyzeSyntaxSuccessEventHandler(AnalyzeSyntaxResponse obj)
        {
            if (AnalyzeSyntaxSuccessEvent != null)
                AnalyzeSyntaxSuccessEvent(obj);
        }

        private void AnalyzeSentimentSuccessEventHandler(AnalyzeSentimentResponse obj)
        {
            if (AnalyzeSentimentSuccessEvent != null)
                AnalyzeSentimentSuccessEvent(obj);
        }

        private void AnalyzeEntitySentimentSuccessEventHandler(AnalyzeEntitySentimentResponse obj)
        {
            if (AnalyzeEntitySentimentSuccessEvent != null)
                AnalyzeEntitySentimentSuccessEvent(obj);
        }

        private void AnalyzeEntitiesSuccessEventHandler(AnalyzeEntitiesResponse obj)
        {
            if (AnalyzeEntitiesSuccessEvent != null)
                AnalyzeEntitiesSuccessEvent(obj);
        }

        private void ClassifyTextSuccessEventHandler(ClassifyTextResponse obj)
        {
            if (ClassifyTextSuccessEvent != null)
                ClassifyTextSuccessEvent(obj);
        }
    }
}