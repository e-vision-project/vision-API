using UnityEngine;
using System;
using UnityEngine.UI;

namespace FrostweepGames.Plugins.GoogleCloud.NaturalLanguage
{
    public class GC_NaturalLanguage_TutorialExample : MonoBehaviour
    {
        private GCNaturalLanguage _gcNaturalLanguage;

        public Button doItButton;
        public Text textInfo;
        public InputField inputTextField;

        private void Start()
        {
            _gcNaturalLanguage = GCNaturalLanguage.Instance;

            _gcNaturalLanguage.AnnotateTextSuccessEvent += _gcNaturalLanguage_AnnotateTextSuccessEvent;
            _gcNaturalLanguage.AnalyzeEntitySentimentSuccessEvent += _gcNaturalLanguage_AnalyzeEntitySentimentSuccessEvent;
            _gcNaturalLanguage.AnalyzeSentimentSuccessEvent += _gcNaturalLanguage_AnalyzeSentimentSuccessEvent;
            _gcNaturalLanguage.AnalyzeSyntaxSuccessEvent += _gcNaturalLanguage_AnalyzeSyntaxSuccessEvent;
            _gcNaturalLanguage.AnalyzeEntitiesSuccessEvent += _gcNaturalLanguage_AnalyzeEntitiesSuccessEvent;
            _gcNaturalLanguage.ClassifyTextSuccessEvent += _gcNaturalLanguage_ClassifyTextSuccessEvent;


            _gcNaturalLanguage.AnnotateTextFailedEvent += _gcNaturalLanguage_AnnotateTextFailedEvent;
            _gcNaturalLanguage.AnalyzeEntitySentimentFailedEvent += _gcNaturalLanguage_AnalyzeEntitySentimentFailedEvent;
            _gcNaturalLanguage.AnalyzeSentimentFailedEvent += _gcNaturalLanguage_AnalyzeSentimentFailedEvent;
            _gcNaturalLanguage.AnalyzeSyntaxFailedEvent += _gcNaturalLanguage_AnalyzeSyntaxFailedEvent;
            _gcNaturalLanguage.AnalyzeEntitiesFailedEvent += _gcNaturalLanguage_AnalyzeEntitiesFailedEvent;
            _gcNaturalLanguage.ClassifyTextFailedEvent += _gcNaturalLanguage_ClassifyTextFailedEvent;

            doItButton.onClick.AddListener(DoItButtonOnClickHandler);
        }


        private void DoItButtonOnClickHandler()
        {
            string content = inputTextField.text;

            if (string.IsNullOrEmpty(content))
                return;

            textInfo.text = string.Empty;

    

            AnnotateText(content, Enumerators.Language.en);
            AnalyzeEntities(content, Enumerators.Language.en);
            AnalyzeEntitySentiment(content, Enumerators.Language.en);
            AnalyzeSentiment(content, Enumerators.Language.en);
            AnalyzeSyntax(content, Enumerators.Language.en);
            ClassifyText(content, Enumerators.Language.en);
        }


        #region failed handlers
        private void _gcNaturalLanguage_ClassifyTextFailedEvent(string obj)
        {
            Debug.Log(obj);
        }

        private void _gcNaturalLanguage_AnalyzeEntitiesFailedEvent(string obj)
        {
            Debug.Log(obj);
        }

        private void _gcNaturalLanguage_AnalyzeSyntaxFailedEvent(string obj)
        {
            Debug.Log(obj);
        }

        private void _gcNaturalLanguage_AnalyzeSentimentFailedEvent(string obj)
        {
            Debug.Log(obj);
        }

        private void _gcNaturalLanguage_AnalyzeEntitySentimentFailedEvent(string obj)
        {
            Debug.Log(obj);
        }

        private void _gcNaturalLanguage_AnnotateTextFailedEvent(string obj)
        {
            Debug.Log(obj);
        }

        #endregion failed handlers

        #region sucess handlers

        private void _gcNaturalLanguage_AnnotateTextSuccessEvent(AnnotateTextResponse obj)
        {
            string result = string.Empty;

            foreach (var sentence in obj.sentences)
                result += sentence.text.content + Environment.NewLine;

            textInfo.text += "sentences :" + result + "\n";
        }

        private void _gcNaturalLanguage_ClassifyTextSuccessEvent(ClassifyTextResponse obj)
        {
            string result = string.Empty;

            foreach (var item in obj.categories)
                result += item.name + " | " + item.confidence + Environment.NewLine;

            textInfo.text += "sentences :" + result + "\n";
        }

        private void _gcNaturalLanguage_AnalyzeEntitiesSuccessEvent(AnalyzeEntitiesResponse obj)
        {
            string result = string.Empty;

            foreach (var item in obj.entities)
                result += item.name + " | " + item.type + Environment.NewLine;

            textInfo.text += "entities :" + result + "\n";
        }

        private void _gcNaturalLanguage_AnalyzeSyntaxSuccessEvent(AnalyzeSyntaxResponse obj)
        {
            string result = string.Empty;

            foreach (var item in obj.sentences)
                result += item.text.content + Environment.NewLine;

            textInfo.text += "sentences :" + result + "\n";
        }

        private void _gcNaturalLanguage_AnalyzeSentimentSuccessEvent(AnalyzeSentimentResponse obj)
        {
            string result = string.Empty;

            foreach (var item in obj.sentences)
                result += item.text.content + " | " + item.sentiment.magnitude + Environment.NewLine;

            textInfo.text += "sentences :" + result + "\n";
        }

        private void _gcNaturalLanguage_AnalyzeEntitySentimentSuccessEvent(AnalyzeEntitySentimentResponse obj)
        {
            string result = string.Empty;

            foreach (var item in obj.entities)
                result += item.name + " | " + item.type + Environment.NewLine;

            textInfo.text += "entities :" + result + "\n";
        }

        #endregion sucess handlers


        public void AnnotateText(string text, Enumerators.Language lang)
        {
            _gcNaturalLanguage.Annotate(new AnnotateTextRequest()
            {
                encodingType = Enumerators.EncodingType.UTF8,
                features = new Features
                {
                    extractDocumentSentiment = true,
                    extractEntities = true,
                    extractEntitySentiment = true,
                    extractSyntax = true
                },
                document = new LocalDocument()
                {
                    content = text,
                    language = _gcNaturalLanguage.PrepareLanguage(lang),
                    type = Enumerators.DocumentType.PLAIN_TEXT
                }
            });
        }
        public void AnalyzeEntities(string text, Enumerators.Language lang)
        {
            _gcNaturalLanguage.Annotate(new AnalyzeEntitiesRequest()
            {
                encodingType = Enumerators.EncodingType.UTF8,
                document = new LocalDocument()
                {
                    content = text,
                    language = _gcNaturalLanguage.PrepareLanguage(lang),
                    type = Enumerators.DocumentType.PLAIN_TEXT
                }
            });
        }
        public void AnalyzeEntitySentiment(string text, Enumerators.Language lang)
        {
            _gcNaturalLanguage.Annotate(new AnalyzeEntitySentimentRequest()
            {
                encodingType = Enumerators.EncodingType.UTF8,
                document = new LocalDocument()
                {
                    content = text,
                    language = _gcNaturalLanguage.PrepareLanguage(lang),
                    type = Enumerators.DocumentType.PLAIN_TEXT
                }
            });
        }
        public void AnalyzeSentiment(string text, Enumerators.Language lang)
        {
            _gcNaturalLanguage.Annotate(new AnalyzeSentimentRequest()
            {
                encodingType = Enumerators.EncodingType.UTF8,
                document = new LocalDocument()
                {
                    content = text,
                    language = _gcNaturalLanguage.PrepareLanguage(lang),
                    type = Enumerators.DocumentType.PLAIN_TEXT
                }
            });
        }
        public void AnalyzeSyntax(string text, Enumerators.Language lang)
        {
            _gcNaturalLanguage.Annotate(new AnalyzeSyntaxRequest()
            {
                encodingType = Enumerators.EncodingType.UTF8,
                document = new LocalDocument()
                {
                    content = text,
                    language = _gcNaturalLanguage.PrepareLanguage(lang),
                    type = Enumerators.DocumentType.PLAIN_TEXT
                }
            });
        }
        public void ClassifyText(string text, Enumerators.Language lang)
        {
            _gcNaturalLanguage.Annotate(new ClassifyTextRequest()
            {
                document = new LocalDocument()
                {
                    content = text,
                    language = _gcNaturalLanguage.PrepareLanguage(lang),
                    type = Enumerators.DocumentType.PLAIN_TEXT
                },
            });
        }
    }

}