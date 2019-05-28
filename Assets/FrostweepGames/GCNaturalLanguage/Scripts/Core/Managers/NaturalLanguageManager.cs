using UnityEngine;
using System;
using Newtonsoft.Json;

namespace FrostweepGames.Plugins.GoogleCloud.NaturalLanguage
{
    public class NaturalLanguageManager : IService, IDisposable, INaturalLanguageManager
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

        private Networking _networking;
        private GCNaturalLanguage _gcNaturalLanguage;

        public void Init()
        {
            _gcNaturalLanguage = GCNaturalLanguage.Instance;

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

        public string PrepareLanguage(Enumerators.Language lang)
        {
            return lang.ToString().Replace("_", "-");
        }

        public void Annotate(AnalyzeEntitiesRequest entitiesRequest)
        {
            AnnotateLocal(JsonConvert.SerializeObject(entitiesRequest), Enumerators.GoogleCloudRequestType.ANALYZE_ENTITIES_REQUEST);
        }
        public void Annotate(AnalyzeEntitySentimentRequest entitySentiment)
        {
            AnnotateLocal(JsonConvert.SerializeObject(entitySentiment), Enumerators.GoogleCloudRequestType.ANALYZE_ENTITY_SENTIMENT_REQUEST);
        }
        public void Annotate(AnalyzeSentimentRequest sentimentRequest)
        {
            AnnotateLocal(JsonConvert.SerializeObject(sentimentRequest), Enumerators.GoogleCloudRequestType.ANALYZE_SENTIMENT_REQUEST);
        }
        public void Annotate(AnalyzeSyntaxRequest syntaxRequest)
        {
            AnnotateLocal(JsonConvert.SerializeObject(syntaxRequest), Enumerators.GoogleCloudRequestType.ANALYZE_SYNTAX_REQUEST);
        }
        public void Annotate(AnnotateTextRequest textRequest)
        {
            AnnotateLocal(JsonConvert.SerializeObject(textRequest), Enumerators.GoogleCloudRequestType.ANNOTATE_TEXT_REQUEST);
        }
        public void Annotate(ClassifyTextRequest classifyTextRequest)
        {
            AnnotateLocal(JsonConvert.SerializeObject(classifyTextRequest), Enumerators.GoogleCloudRequestType.CLASSIFY_TEXT_REQUEST);
        }
        private void AnnotateLocal(string postData, Enumerators.GoogleCloudRequestType cloudRequestType)
        {
            string uri = string.Empty;

            switch (cloudRequestType)
            {
                case Enumerators.GoogleCloudRequestType.ANALYZE_ENTITIES_REQUEST:
                    uri += Constants.ANALYZE_ENTITIES_REQUEST_URL;
                    break;
                case Enumerators.GoogleCloudRequestType.ANALYZE_ENTITY_SENTIMENT_REQUEST:
                    uri += Constants.ANALYZE_ENTITY_SENTIMENT_REQUEST_URL;
                    break;
                case Enumerators.GoogleCloudRequestType.ANALYZE_SENTIMENT_REQUEST:
                    uri += Constants.ANALYZE_SENTIMENT_REQUEST_URL;
                    break;
                case Enumerators.GoogleCloudRequestType.ANALYZE_SYNTAX_REQUEST:
                    uri += Constants.ANALYZE_SYNTAX_REQUEST_URL;
                    break;
                case Enumerators.GoogleCloudRequestType.ANNOTATE_TEXT_REQUEST:
                    uri += Constants.ANNOTATE_TEXT_REQUEST_URL;
                    break;
                case Enumerators.GoogleCloudRequestType.CLASSIFY_TEXT_REQUEST:
                    uri += Constants.CLASSIFY_TEXT_REQUEST_URL;
                    break;
                default: break;
            }

            if (!_gcNaturalLanguage.isUseAPIKeyFromPrefab)
                uri += Constants.API_KEY_PARAM + Constants.GC_API_KEY;
            else
                uri += Constants.API_KEY_PARAM + _gcNaturalLanguage.apiKey;

            _networking.SendRequest(uri, postData, NetworkEnumerators.RequestType.POST, new object[] { cloudRequestType } );
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
                    if (_gcNaturalLanguage.isFullDebugLogIfError)
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
                case Enumerators.GoogleCloudRequestType.ANALYZE_ENTITIES_REQUEST:
                    {
                        if (AnalyzeEntitiesFailedEvent != null)
                            AnalyzeEntitiesFailedEvent(error);
                    }
                    break;
                case Enumerators.GoogleCloudRequestType.ANALYZE_ENTITY_SENTIMENT_REQUEST:
                    {
                        if (AnalyzeEntitySentimentFailedEvent != null)
                            AnalyzeEntitySentimentFailedEvent(error);
                    }
                    break;
                case Enumerators.GoogleCloudRequestType.ANALYZE_SENTIMENT_REQUEST:
                    {
                        if (AnalyzeSentimentFailedEvent != null)
                            AnalyzeSentimentFailedEvent(error);
                    }
                    break;
                case Enumerators.GoogleCloudRequestType.ANALYZE_SYNTAX_REQUEST:
                    {
                        if (AnalyzeSyntaxFailedEvent != null)
                            AnalyzeSyntaxFailedEvent(error);
                    }
                    break;
                case Enumerators.GoogleCloudRequestType.ANNOTATE_TEXT_REQUEST:
                    {
                        if (AnnotateTextFailedEvent != null)
                            AnnotateTextFailedEvent(error);
                    }
                    break;
                case Enumerators.GoogleCloudRequestType.CLASSIFY_TEXT_REQUEST:
                    {
                        if (ClassifyTextFailedEvent != null)
                            ClassifyTextFailedEvent(error);
                    }
                    break;
                default: break;
            }
        }
        private void ThrowSuccessEvent(string data, Enumerators.GoogleCloudRequestType type)
        {
            switch (type)
            {
                case Enumerators.GoogleCloudRequestType.ANALYZE_ENTITIES_REQUEST:
                    {
                        if (AnalyzeEntitiesSuccessEvent != null)
                            AnalyzeEntitiesSuccessEvent(JsonConvert.DeserializeObject<AnalyzeEntitiesResponse>(data));
                    }
                    break;
                case Enumerators.GoogleCloudRequestType.ANALYZE_ENTITY_SENTIMENT_REQUEST:
                    {
                        if (AnalyzeEntitySentimentSuccessEvent != null)
                            AnalyzeEntitySentimentSuccessEvent(JsonConvert.DeserializeObject<AnalyzeEntitySentimentResponse>(data));
                    }
                    break;
                case Enumerators.GoogleCloudRequestType.ANALYZE_SENTIMENT_REQUEST:
                    {
                        if (AnalyzeSentimentSuccessEvent != null)
                            AnalyzeSentimentSuccessEvent(JsonConvert.DeserializeObject<AnalyzeSentimentResponse>(data));
                    }
                    break;
                case Enumerators.GoogleCloudRequestType.ANALYZE_SYNTAX_REQUEST:
                    {
                        if (AnalyzeSyntaxSuccessEvent != null)
                            AnalyzeSyntaxSuccessEvent(JsonConvert.DeserializeObject<AnalyzeSyntaxResponse>(data));
                    }
                    break;
                case Enumerators.GoogleCloudRequestType.ANNOTATE_TEXT_REQUEST:
                    {
                        if (AnnotateTextSuccessEvent != null)
                            AnnotateTextSuccessEvent(JsonConvert.DeserializeObject<AnnotateTextResponse>(data));
                    }
                    break;
                case Enumerators.GoogleCloudRequestType.CLASSIFY_TEXT_REQUEST:
                    {
                        if (ClassifyTextSuccessEvent != null)
                            ClassifyTextSuccessEvent(JsonConvert.DeserializeObject<ClassifyTextResponse>(data));
                    }
                    break;
                default: break;
            }
        }
    }
}