using UnityEngine;
using System;
using Newtonsoft.Json;

namespace FrostweepGames.Plugins.GoogleCloud.VideoIntelligence
{
    public class VideoIntelligenceManager : IService, IDisposable, IVideoIntelligenceManager
    {
        public event Action<AnnotateResponse, long> AnnotateSuccessEvent;
        public event Action<string, long> AnnotateFailedEvent;

        public event Action<Operation, long> GetSuccessEvent;
        public event Action<string, long> GetFailedEvent;

        public event Action<ListOperationResponse, long> ListSuccessEvent;
        public event Action<string, long> ListFailedEvent;

        public event Action<string> CancelSuccessEvent;
        public event Action<string, long> CancelFailedEvent;

        public event Action<string> DeleteSuccessEvent;
        public event Action<string, long> DeleteFailedEvent;

        private Networking _networking;
        private GCVideoIntelligence _gcVideoIntelligence;

        public void Init()
        {
            _gcVideoIntelligence = GCVideoIntelligence.Instance;

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

        public void Annotate(AnnotateVideoRequest request)
        {
            if (request == null)
                throw new NotImplementedException("request is null");

            string uri = Constants.POST_VIDEO_ANNOTATE_REQUEST_URL + Constants.API_KEY_PARAM;

            uri += _gcVideoIntelligence.isUseAPIKeyFromPrefab ? _gcVideoIntelligence.apiKey : Constants.GC_API_KEY;

            _networking.SendRequest(uri, JsonConvert.SerializeObject(request), NetworkEnumerators.RequestType.POST, new object[] { Enumerators.ApiType.ANNOTATE });
        }

        public void Get(string name)
        {
            if (string.IsNullOrEmpty(name))
                throw new NotImplementedException("name is null or empty");

            string uri = Constants.GET_OPERATIONS_GET_REQUEST_URL.Replace("{name}", name) + Constants.API_KEY_PARAM;

            uri += _gcVideoIntelligence.isUseAPIKeyFromPrefab ? _gcVideoIntelligence.apiKey : Constants.GC_API_KEY;

            _networking.SendRequest(uri, string.Empty, NetworkEnumerators.RequestType.GET, new object[] { Enumerators.ApiType.GET });
        }

        public void List(string name, string filter, double pageSize, string pageToken)
        {
            if (string.IsNullOrEmpty(name))
                throw new NotImplementedException("name is null or empty");

            string uri = Constants.GET_OPERATIONS_LIST_REQUEST_URL + Constants.API_KEY_PARAM;

            uri += _gcVideoIntelligence.isUseAPIKeyFromPrefab ? _gcVideoIntelligence.apiKey : Constants.GC_API_KEY;

            uri += "&name=" + name + "&filter=" + filter + "&pageSize=" + pageSize + "&pageToken=" + pageToken;

            _networking.SendRequest(uri, string.Empty, NetworkEnumerators.RequestType.GET, new object[] { Enumerators.ApiType.LIST });
        }

        public void Cancel(string name)
        {
            if (string.IsNullOrEmpty(name))
                throw new NotImplementedException("name is null or empty");

            string uri = Constants.POST_OPERATIONS_CANCEL_REQUEST_URL.Replace("{name}", name) + Constants.API_KEY_PARAM;

            uri += _gcVideoIntelligence.isUseAPIKeyFromPrefab ? _gcVideoIntelligence.apiKey : Constants.GC_API_KEY;

            _networking.SendRequest(uri, string.Empty, NetworkEnumerators.RequestType.POST, new object[] { Enumerators.ApiType.CANCEL });
        }

        public void Delete(string name)
        {
            if (string.IsNullOrEmpty(name))
                throw new NotImplementedException("name is null or empty");

            string uri = Constants.DELETE_OPERATIONS_DELETE_REQUEST_URL + name + Constants.API_KEY_PARAM;

            uri += _gcVideoIntelligence.isUseAPIKeyFromPrefab ? _gcVideoIntelligence.apiKey : Constants.GC_API_KEY;

            _networking.SendRequest(uri, string.Empty, NetworkEnumerators.RequestType.DELETE, new object[] { Enumerators.ApiType.DELETE });
        }


        private void NetworkResponseEventHandler(NetworkResponse response)
        {
            if (response.parameters == null)
            {
                return;
            }

            if (!string.IsNullOrEmpty(response.error) || response.response.Contains("error"))
            {
                string error = response.error + "\n" + response.response;

                if (_gcVideoIntelligence.isFullDebugLogIfError)
                    Debug.Log(error);

                switch ((Enumerators.ApiType)response.parameters[0])
                {
                    case Enumerators.ApiType.ANNOTATE:
                        if (AnnotateFailedEvent != null)
                            AnnotateFailedEvent(error, response.netPacketIndex);
                        break;
                    case Enumerators.ApiType.GET:
                        if (GetFailedEvent != null)
                            GetFailedEvent(error, response.netPacketIndex);
                        break;
                    case Enumerators.ApiType.LIST:
                        if (ListFailedEvent != null)
                            ListFailedEvent(error, response.netPacketIndex);
                        break;
                    case Enumerators.ApiType.CANCEL:
                        if (CancelFailedEvent != null)
                            CancelFailedEvent(error, response.netPacketIndex);
                        break;
                    case Enumerators.ApiType.DELETE:
                        if (DeleteFailedEvent != null)
                            DeleteFailedEvent(error, response.netPacketIndex);
                        break;
                }
            }
            else
            {
                response.response = response.response.Replace("@type", "type");

                switch ((Enumerators.ApiType)response.parameters[0])
                {
                    case Enumerators.ApiType.ANNOTATE:
                        if (AnnotateSuccessEvent != null)
                            AnnotateSuccessEvent(JsonConvert.DeserializeObject<AnnotateResponse>(response.response), response.netPacketIndex);
                        break;
                    case Enumerators.ApiType.GET:
                        if (GetSuccessEvent != null)
                            GetSuccessEvent(JsonConvert.DeserializeObject<Operation>(response.response), response.netPacketIndex);
                        break;
                    case Enumerators.ApiType.LIST:
                        if (ListSuccessEvent != null)
                            ListSuccessEvent(JsonConvert.DeserializeObject<ListOperationResponse>(response.response), response.netPacketIndex);
                        break;
                    case Enumerators.ApiType.CANCEL:
                        if (CancelSuccessEvent != null)
                            CancelSuccessEvent(response.response);
                        break;
                    case Enumerators.ApiType.DELETE:
                        if (DeleteSuccessEvent != null)
                            DeleteSuccessEvent(response.response);
                        break;
                }
            }
        }
    }
}