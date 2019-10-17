using System;
using UnityEngine;

namespace FrostweepGames.Plugins.GoogleCloud.VideoIntelligence
{
    public class GCVideoIntelligence : MonoBehaviour
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


        private static GCVideoIntelligence _Instance;
        public static GCVideoIntelligence Instance
        {
            get
            {
                if (_Instance == null)
                    _Instance = new GameObject("[Singleton]GCVideoIntelligence").AddComponent<GCVideoIntelligence>();

                return _Instance;
            }
        }


        private ServiceLocator _serviceLocator;

        private IVideoIntelligenceManager _videoIntelligenceManager;

        public ServiceLocator ServiceLocator { get { return _serviceLocator; } }

        [Header("Prefab Object Settings")]
        public bool isDontDestroyOnLoad = false;
        public bool isFullDebugLogIfError = false;
        public bool isUseAPIKeyFromPrefab = false;

        [Header("Prefab Fields")]
        public string apiKey;

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

            _videoIntelligenceManager = _serviceLocator.Get<IVideoIntelligenceManager>();

            _videoIntelligenceManager.AnnotateSuccessEvent += AnnotateSuccessEventHandler;
            _videoIntelligenceManager.AnnotateFailedEvent += AnnotateFailedEventHandler;

            _videoIntelligenceManager.GetSuccessEvent += GetSuccessEventHandler;
            _videoIntelligenceManager.GetFailedEvent += GetFailedEventHandler;

            _videoIntelligenceManager.ListSuccessEvent += ListSuccessEventHandler;
            _videoIntelligenceManager.ListFailedEvent += ListFailedEventHandler;

            _videoIntelligenceManager.CancelSuccessEvent += CancelSuccessEventHandler;
            _videoIntelligenceManager.CancelFailedEvent += CancelFailedEventHandler;

            _videoIntelligenceManager.DeleteSuccessEvent += DeleteSuccessEventHandler;
            _videoIntelligenceManager.DeleteFailedEvent += DeleteFailedEventHandler;
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
                _videoIntelligenceManager.AnnotateSuccessEvent -= AnnotateSuccessEventHandler;
                _videoIntelligenceManager.AnnotateFailedEvent -= AnnotateFailedEventHandler;

                _videoIntelligenceManager.GetSuccessEvent -= GetSuccessEventHandler;
                _videoIntelligenceManager.GetFailedEvent -= GetFailedEventHandler;

                _videoIntelligenceManager.ListSuccessEvent -= ListSuccessEventHandler;
                _videoIntelligenceManager.ListFailedEvent -= ListFailedEventHandler;

                _videoIntelligenceManager.CancelSuccessEvent -= CancelSuccessEventHandler;
                _videoIntelligenceManager.CancelFailedEvent -= CancelFailedEventHandler;

                _videoIntelligenceManager.DeleteSuccessEvent -= DeleteSuccessEventHandler;
                _videoIntelligenceManager.DeleteFailedEvent -= DeleteFailedEventHandler;

                _Instance = null;
                _serviceLocator.Dispose();
            }
        }

        public void Annotate(AnnotateVideoRequest request)
        {
            _videoIntelligenceManager.Annotate(request);
        }

        public void Get(string name)
        {
            _videoIntelligenceManager.Get(name);
        }

        public void List(string name, string filter, double pageSize, string pageToken)
        {
            _videoIntelligenceManager.List(name, filter, pageSize, pageToken);
        }

        public void Cancel(string name)
        {
            _videoIntelligenceManager.Cancel(name);
        }

        public void Delete(string name)
        {
            _videoIntelligenceManager.Delete(name);
        }

        private void AnnotateSuccessEventHandler(AnnotateResponse arg1, long arg2)
        {
            if (AnnotateSuccessEvent != null)
                AnnotateSuccessEvent(arg1, arg2);
        }

        private void GetSuccessEventHandler(Operation arg1, long arg2)
        {
            if (GetSuccessEvent != null)
                GetSuccessEvent(arg1, arg2);
        }

        private void ListSuccessEventHandler(ListOperationResponse arg1, long arg2)
        {
            if (ListSuccessEvent != null)
                ListSuccessEvent(arg1, arg2);
        }

        private void CancelSuccessEventHandler(string response)
        {
            if (CancelSuccessEvent != null)
                CancelSuccessEvent(response);
        }

        private void DeleteSuccessEventHandler(string response)
        {
            if (DeleteSuccessEvent != null)
                DeleteSuccessEvent(response);
        }

        private void AnnotateFailedEventHandler(string arg1, long arg2)
        {
            if (AnnotateFailedEvent != null)
                AnnotateFailedEvent(arg1, arg2);
        }

        private void GetFailedEventHandler(string arg1, long arg2)
        {
            if (GetFailedEvent != null)
                GetFailedEvent(arg1, arg2);
        }

        private void ListFailedEventHandler(string arg1, long arg2)
        {
            if (ListFailedEvent != null)
                ListFailedEvent(arg1, arg2);
        }

        private void CancelFailedEventHandler(string arg1, long arg2)
        {
            if (CancelFailedEvent != null)
                CancelFailedEvent(arg1, arg2);
        }

        private void DeleteFailedEventHandler(string arg1, long arg2)
        {
            if (DeleteFailedEvent != null)
                DeleteFailedEvent(arg1, arg2);
        }
    }
}