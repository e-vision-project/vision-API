using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using UnityEngine.UI;

namespace FrostweepGames.Plugins.GoogleCloud.VideoIntelligence.Examples
{
    public class GC_VideoIntelligence_Example : MonoBehaviour
    {
        private GCVideoIntelligence _gcVideoIntelligence;

        public Button annotateButton;

        public Button getButton,
                      listButton,
                      cancelButton,
                      deleteButton,
                      selectButton;

        public InputField urlInputField;

        public InputField nameInputField;
        public InputField filterInputField;
        public InputField pageSizeInputField;
        public InputField pageTokenInputField;

        public Text logText;

        public Image statusImage;

        private CultureInfo _provider;

        private void Start()
        {
            _provider = (CultureInfo)CultureInfo.InvariantCulture.Clone();
            _provider.NumberFormat.NumberDecimalSeparator = ".";

            _gcVideoIntelligence = GCVideoIntelligence.Instance;

            _gcVideoIntelligence.AnnotateSuccessEvent += AnnotateSuccessEventHandler;
            _gcVideoIntelligence.AnnotateFailedEvent += AnnotateFailedEventHandler;


            _gcVideoIntelligence.GetSuccessEvent += GetSuccessEventHandler;
            _gcVideoIntelligence.GetFailedEvent += GetFailedEventHandler;

            _gcVideoIntelligence.ListSuccessEvent += ListSuccessEventHandler;
            _gcVideoIntelligence.ListFailedEvent += ListFailedEventHandler;

            _gcVideoIntelligence.CancelSuccessEvent += CancelSuccessEventHandler;
            _gcVideoIntelligence.CancelFailedEvent += CancelFailedEventHandler;

            _gcVideoIntelligence.DeleteSuccessEvent += DeleteSuccessEventHandler;
            _gcVideoIntelligence.DeleteFailedEvent += DeleteFailedEventHandler;

            annotateButton.onClick.AddListener(AnnotateButtonOnClickHandler);

            getButton.onClick.AddListener(GetButtonOnClickHandler);
            listButton.onClick.AddListener(ListButtonOnClickHandler);
            cancelButton.onClick.AddListener(CancelButtonOnClickHandler);
            deleteButton.onClick.AddListener(DeleteButtonOnClickHandler);
            selectButton.onClick.AddListener(SelectButtonOnClickHandler);

#if UNITY_EDITOR

            //selectButton.interactable = true;
#else
            selectButton.interactable = false;
#endif  

            urlInputField.text = Application.dataPath + "/FrostweepGames/GCVideoIntelligence/StreamingAssets/CloudVideoIntelligenceAPIDemo.mp4";
        }

        private void SelectButtonOnClickHandler()
        {
#if UNITY_EDITOR
            urlInputField.text = UnityEditor.EditorUtility.OpenFilePanel("Video File Browser", System.IO.Directory.GetLogicalDrives()[0], "mp4,flv,avi,mov,ogv,wmv,m4v,mpg");
#endif
        }

        private void AnnotateButtonOnClickHandler()
        {
            if(string.IsNullOrEmpty(urlInputField.text))
            {
                statusImage.color = Color.red;
                return;
            }

            statusImage.color = Color.yellow;

            AnnotateVideo(VideoConvert.Convert(urlInputField.text));
        }

        private void GetButtonOnClickHandler()
        {
            if (string.IsNullOrEmpty(nameInputField.text))
            {
                statusImage.color = Color.red;
                return;
            }

            statusImage.color = Color.yellow;

            _gcVideoIntelligence.Get(nameInputField.text);
        }

        private void ListButtonOnClickHandler()
        {
            if (string.IsNullOrEmpty(nameInputField.text))
            {
                statusImage.color = Color.red;
                return;
            }

            statusImage.color = Color.yellow;

            _gcVideoIntelligence.List(
               nameInputField.text,
               filterInputField.text,
               double.Parse(pageSizeInputField.text, _provider),
               pageTokenInputField.text);
        }

        private void CancelButtonOnClickHandler()
        {
            if (string.IsNullOrEmpty(nameInputField.text))
            {
                statusImage.color = Color.red;
                return;
            }

            statusImage.color = Color.yellow;

            _gcVideoIntelligence.Cancel(nameInputField.text);
        }

        private void DeleteButtonOnClickHandler()
        {
            if (string.IsNullOrEmpty(nameInputField.text))
            {
                statusImage.color = Color.red;
                return;
            }

            statusImage.color = Color.yellow;

            _gcVideoIntelligence.Delete(nameInputField.text);
        }

        public void AnnotateVideo(string content)
        {
            _gcVideoIntelligence.Annotate(new ContentAnnotateVideoRequest()
            {
                features = new List<Enumerators.Feature>()
                {
                    Enumerators.Feature.EXPLICIT_CONTENT_DETECTION
                },
                inputContent = content
            });
        }

        private void AnnotateSuccessEventHandler(AnnotateResponse response, long arg2)
        {
            statusImage.color = Color.green;

            Debug.Log("AnnotateSuccessEventHandler: " + response.name);

            nameInputField.text = response.name;

            Log("AnnotateSuccessEventHandler: " + response.name);
        }

        private void GetSuccessEventHandler(Operation response, long arg2)
        {
            statusImage.color = Color.green;

            if (response.response != null)
            {
                if (response.response.annotationResults != null)
                {
                    foreach (var result in response.response.annotationResults)
                    {
                        foreach (var frame in result.explicitAnnotation.frames)
                        {
                            Debug.Log(frame.pornographyLikelihood + " | " + frame.timeOffset);

                            Log(frame.pornographyLikelihood + " | " + frame.timeOffset);
                        }
                    }
                }
            }
            else if (response.metadata != null)
            {
                foreach (var progress in response.metadata.annotationProgress)
                {
                    Debug.Log("progress: " + progress.progressPercent + " ; start Time" + progress.startTime + " ; update Time" + progress.updateTime);
                    Log("progress: " + progress.progressPercent + " ; start Time" + progress.startTime + " ; update Time" + progress.updateTime);
                }
            }
        }

        private void ListSuccessEventHandler(ListOperationResponse response, long arg2)
        {
            statusImage.color = Color.green;

            Debug.Log("ListSuccessEventHandler: " + response.nextPageToken);

            pageTokenInputField.text = response.nextPageToken;

            if (response.operations != null)
            {
                foreach (var operation in response.operations)
                {
                    Debug.Log(Newtonsoft.Json.JsonConvert.SerializeObject(operation));
                }
            }

            Log("ListSuccessEventHandler: " + response.nextPageToken);
        }

        private void CancelSuccessEventHandler(string response)
        {
            statusImage.color = Color.green;

            Debug.Log("CancelSuccessEventHandler: " + response);
            Log("CancelSuccessEventHandler: " + response);
        }

        private void DeleteSuccessEventHandler(string response)
        {
            statusImage.color = Color.green;

            Debug.Log("DeleteSuccessEventHandler: " + response);
            Log("DeleteSuccessEventHandler: " + response);
        }

        private void GetFailedEventHandler(string arg1, long arg2)
        {
            Debug.Log("Error: " + arg1 + " - " + arg2);

            Log("Error: " + arg1 + " - " + arg2);
            statusImage.color = Color.red;
        }

        private void ListFailedEventHandler(string arg1, long arg2)
        {
            Debug.Log("Error: " + arg1 + " - " + arg2);

            Log("Error: " + arg1 + " - " + arg2);
            statusImage.color = Color.red;
        }

        private void CancelFailedEventHandler(string arg1, long arg2)
        {
            Debug.Log("Error: " + arg1 + " - " + arg2);

            Log("Error: " + arg1 + " - " + arg2);
            statusImage.color = Color.red;
        }

        private void DeleteFailedEventHandler(string arg1, long arg2)
        {
            Debug.Log("Error: " + arg1 + " - " + arg2);

            Log("Error: " + arg1 + " - " + arg2);
            statusImage.color = Color.red;
        }

        private void AnnotateFailedEventHandler(string arg1, long arg2)
        {
            Debug.Log("Error: " + arg1 + " - " + arg2);

            Log("Error: " + arg1 + " - " + arg2);
            statusImage.color = Color.red;
        }

        private void Log(string log)
        {
            string oldLog = logText.text;
            logText.text = log + "\n" + oldLog;
        }
    }
}