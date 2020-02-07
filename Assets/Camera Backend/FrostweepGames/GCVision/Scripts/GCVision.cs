using System;
using System.Collections.Generic;
using UnityEngine;

namespace FrostweepGames.Plugins.GoogleCloud.Vision
{
    public class GCVision : MonoBehaviour
    {
        public event Action<VisionResponse, long> AnnotateSuccessEvent;
        public event Action<string, long> AnnotateFailedEvent;


        private static GCVision _Instance;
        public static GCVision Instance
        {
            get
            {
                if (_Instance == null)
                    _Instance = new GameObject("[Singleton]GCVision").AddComponent<GCVision>();

                return _Instance;
            }
        }


        private ServiceLocator _serviceLocator;

        private IVisionManager _visionManager;

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

            _visionManager = _serviceLocator.Get<IVisionManager>();

            _visionManager.AnnotateSuccessEvent += AnnotateSuccessEventHandler;
            _visionManager.AnnotateFailedEvent += AnnotateFailedEventHandler;
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
                _visionManager.AnnotateSuccessEvent -= AnnotateSuccessEventHandler;
                _visionManager.AnnotateFailedEvent -= AnnotateFailedEventHandler;

                _Instance = null;
                _serviceLocator.Dispose();
            }
        }

        public void GenerateAnnotateRequests()
        {

        }


        public void Annotate(List<AnnotateRequest> requests)
        {
            _visionManager.Annotate(requests);
        }
      

        private void AnnotateSuccessEventHandler(VisionResponse arg1, long arg2)
        {
            if (AnnotateSuccessEvent != null)
                AnnotateSuccessEvent(arg1, arg2);
        }

        private void AnnotateFailedEventHandler(string arg1, long arg2)
        {
            if (AnnotateFailedEvent != null)
                AnnotateFailedEvent(arg1, arg2);
        }
    }
}