using UnityEngine;
using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace FrostweepGames.Plugins.GoogleCloud.Vision
{
    public class VisionManager : IService, IDisposable, IVisionManager
    {
        public event Action<VisionResponse, long> AnnotateSuccessEvent;
        public event Action<string, long> AnnotateFailedEvent;

        private Networking _networking;
		private GCVision _gcSpeechRecognition;

        public void Init()
        {
			_gcSpeechRecognition = GCVision.Instance;
			
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


        public void Annotate(List<AnnotateRequest> requestsList)
        {
            if (requestsList == null)
                throw new NotImplementedException("List<AnnotateRequest> isn't seted!");


            var requests = Generate(requestsList);

            string postData = string.Empty;
            string uri = string.Empty;


            if (!_gcSpeechRecognition.isUseAPIKeyFromPrefab)
                uri = Constants.IMAGES_ANNOTATE_REQUEST_URL + Constants.API_KEY_PARAM + Constants.GC_API_KEY;
            else
                uri = Constants.IMAGES_ANNOTATE_REQUEST_URL + Constants.API_KEY_PARAM + _gcSpeechRecognition.apiKey;

            postData = JsonUtility.ToJson(GenerateSyncRequest(requests));

            _networking.SendRequest(uri, postData, NetworkEnumerators.RequestType.POST);
        }

        private void NetworkResponseEventHandler(NetworkResponse response)
        {
            if(!string.IsNullOrEmpty(response.error))
            {
                if(AnnotateFailedEvent != null)
                    AnnotateFailedEvent(response.error + "; " +response.response, response.netPacketIndex);
            }
            else
            {
                if (response.response.Contains("error"))
                {
                    if (GCVision.Instance.isFullDebugLogIfError)
                        Debug.Log(response.error + "\n" + response.response);

                    if (AnnotateFailedEvent != null)
                        AnnotateFailedEvent(response.response, response.netPacketIndex);
                }
                else if (response.response.Contains("responses"))
                {
                    if (AnnotateSuccessEvent != null)
                        AnnotateSuccessEvent(JsonConvert.DeserializeObject<VisionResponse>(response.response), response.netPacketIndex);
                }
                else if (response.response.Contains("{}"))
                {
                    if (AnnotateFailedEvent != null)
                        AnnotateFailedEvent("Nothing Not Detected!", response.netPacketIndex);
                }
                else
                {
                    if (AnnotateFailedEvent != null)
                        AnnotateFailedEvent("Process Parse Response failed with error: " + response.response, response.netPacketIndex);
                }          
            }
        }

        private VisionRequest GenerateSyncRequest(AnnotateImageRequest[] requests)
        {
            VisionRequest request = new VisionRequest();

            request.requests = requests;

            return request;
        }

        private AnnotateImageRequest[] Generate(List<AnnotateRequest> requestsList)
        {
            AnnotateImageRequest[] requests = new AnnotateImageRequest[requestsList.Count];

            for (int i = 0; i < requestsList.Count; i++)
            {
                requests[i] = new AnnotateImageRequest();
                requests[i].features = requestsList[i].features.ToArray();
                requests[i].image = requestsList[i].image;
                requests[i].imageContext = requestsList[i].context;
            }

            return requests;
        }
    }
}