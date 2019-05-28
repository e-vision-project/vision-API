using System;
using UnityEngine;
using UnityEngine.UI;

namespace FrostweepGames.Plugins.GoogleCloud.Translation.Examples
{
    public class GC_Translation_Example : MonoBehaviour
    {
        private GCTranslation _gcTranslation;

        private Button _translateButton,
                       _detectLanguageButton,
                       _getLanguagesButton;

        private Image _networkState;

        private InputField _textInputField,
                           _textNetworkResultInputField;

        private Dropdown _sourceLanguage,
                         _targetLanguage,
                         _modelType,
                         _formatType;

        private void Start()
        {
            _gcTranslation = GCTranslation.Instance;
            _gcTranslation.TranslateSuccessEvent += TranslateSuccessEventHandler;
            _gcTranslation.DetectLanguageSuccessEvent += DetectLanguageSuccessEventHandler;
            _gcTranslation.GetLanguagesSuccessEvent += GetLanguagesSuccessEventHandler;

            _gcTranslation.TranslateFailedEvent += TranslateFailedEventHandler;
            _gcTranslation.DetectLanguageFailedEvent += DetectLanguageFailedEventHandler;
            _gcTranslation.GetLanguagesFailedEvent += GetLanguagesFailedEventHandler;

            _gcTranslation.ContentOutOfLengthEvent += ContentOutOfLengthEventHandler;


            _translateButton = transform.Find("Canvas/Button_Translate").GetComponent<Button>();
            _detectLanguageButton = transform.Find("Canvas/Button_Detect").GetComponent<Button>();
            _getLanguagesButton = transform.Find("Canvas/Button_GetLanguages").GetComponent<Button>();

            _networkState = transform.Find("Canvas/Image_NetworkState").GetComponent<Image>();

            _textNetworkResultInputField = transform.Find("Canvas/InputField_Output").GetComponent<InputField>();
            _textInputField = transform.Find("Canvas/InputField_Input").GetComponent<InputField>();

            _sourceLanguage = transform.Find("Canvas/Dropdown_Source").GetComponent<Dropdown>();
            _targetLanguage = transform.Find("Canvas/Dropdown_Target").GetComponent<Dropdown>();
            _modelType = transform.Find("Canvas/Dropdown_Model").GetComponent<Dropdown>();
            _formatType = transform.Find("Canvas/Dropdown_Format").GetComponent<Dropdown>();

            _networkState.color = Color.green;

            _sourceLanguage.ClearOptions();
            _targetLanguage.ClearOptions();


            int length = Enum.GetNames(typeof(Enumerators.TextLanguage)).Length;
            for (int i = 0; i < length; i++)
            {
                _sourceLanguage.options.Add(new Dropdown.OptionData(((Enumerators.TextLanguage)i).ToString()));
                _targetLanguage.options.Add(new Dropdown.OptionData(((Enumerators.TextLanguage)i).ToString()));
            }

            _modelType.ClearOptions();
            _formatType.ClearOptions();

            length = Enum.GetNames(typeof(Enumerators.TextFormatType)).Length;

            for (int i = 0; i < length; i++)
                _formatType.options.Add(new Dropdown.OptionData(((Enumerators.TextFormatType)i).ToString()));

            length = Enum.GetNames(typeof(Enumerators.ModelType)).Length;

            for (int i = 0; i < length; i++)
                _modelType.options.Add(new Dropdown.OptionData(((Enumerators.ModelType)i).ToString()));

            _translateButton.onClick.AddListener(TranslateButtonOnClickHandler);
            _detectLanguageButton.onClick.AddListener(DetectLanguageButtonOnclickHandler);
            _getLanguagesButton.onClick.AddListener(GetLanguagesButtonOnclickHandler);

            _sourceLanguage.value = 1;
            _targetLanguage.value = 2;
            _modelType.value = 1;
            _formatType.value = 1;
        }

        private void OnDestroy()
        {
            _gcTranslation.TranslateSuccessEvent -= TranslateSuccessEventHandler;
            _gcTranslation.DetectLanguageSuccessEvent -= DetectLanguageSuccessEventHandler;
            _gcTranslation.GetLanguagesSuccessEvent -= GetLanguagesSuccessEventHandler;

            _gcTranslation.TranslateFailedEvent -= TranslateFailedEventHandler;
            _gcTranslation.DetectLanguageFailedEvent -= DetectLanguageFailedEventHandler;
            _gcTranslation.GetLanguagesFailedEvent -= GetLanguagesFailedEventHandler;

            _gcTranslation.ContentOutOfLengthEvent -= ContentOutOfLengthEventHandler;

            _translateButton.onClick.RemoveAllListeners();
            _detectLanguageButton.onClick.RemoveAllListeners();
            _getLanguagesButton.onClick.RemoveAllListeners();
        }


        private void TranslateButtonOnClickHandler()
        {
            if (string.IsNullOrEmpty(_textInputField.text))
                return;

            ResetState(false);

            _gcTranslation.Translate(new TranslationRequest()
            {
                q = _textInputField.text,
                source = ((Enumerators.TextLanguage)_sourceLanguage.value).ToString(),
                target = ((Enumerators.TextLanguage)_targetLanguage.value).ToString(),
                format = ((Enumerators.TextFormatType)_formatType.value).ToString(),
                model =  ((Enumerators.ModelType)_modelType.value).ToString()
            });
        }

        private void DetectLanguageButtonOnclickHandler()
        {
            if (string.IsNullOrEmpty(_textInputField.text))
                return;

            ResetState(false);

            _gcTranslation.DetectLanguage(new DetectLanguageRequest()
            {
                q = _textInputField.text
            });
        }

        private void GetLanguagesButtonOnclickHandler()
        {
            if (string.IsNullOrEmpty(_textInputField.text))
                return;

            ResetState(false);

            _gcTranslation.GetLanguages(new LanguagesRequest()
            {
                target = _textInputField.text,
                model = ((Enumerators.ModelType)_modelType.value).ToString()
            });
        }


        private void ResetState(bool status)
        {
            _networkState.color = status ? Color.green : Color.yellow;
            _translateButton.interactable = status;
            _detectLanguageButton.interactable = status;
            _getLanguagesButton.interactable = status;

            _textNetworkResultInputField.text = string.Empty;
        }

        // handlers
        private void TranslateSuccessEventHandler(TranslationResponse value)
        {
            ResetState(true);

            foreach (var translation in value.data.translations)
                _textNetworkResultInputField.text += translation.translatedText + "\n---------\n";
        }

        private void DetectLanguageSuccessEventHandler(DetectLanguageResponse value)
        {
            ResetState(true);

            foreach (var detection in value.data.detections)
            {
                foreach (var item in detection)
                    _textNetworkResultInputField.text += "language: " + item.language + "\n" +
                                                         "isReliable: " + item.isReliable + "\n" +
                                                         "confidence: " + item.confidence + "\n---------\n";
            }
        }

        private void GetLanguagesSuccessEventHandler(LanguagesResponse value)
        {
            ResetState(true);

            foreach(var language in value.data.languages)
            {
                _textNetworkResultInputField.text += "name: " + language.name + "\n" +
                                                     "language: " + language.language + "\n---------\n";
            }
        }

        private void TranslateFailedEventHandler(string value)
        {
            ResetState(true);

            _textNetworkResultInputField.text = value;
        }

        private void DetectLanguageFailedEventHandler(string value)
        {
            ResetState(true);

            _textNetworkResultInputField.text = value;
        }

        private void GetLanguagesFailedEventHandler(string value)
        {
            ResetState(true);

            _textNetworkResultInputField.text = value;
        }

        private void ContentOutOfLengthEventHandler()
        {
            ResetState(true);

            _textNetworkResultInputField.text = "Content Out Of Length";
        }
    }
}