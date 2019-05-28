using System;

namespace FrostweepGames.Plugins.GoogleCloud.Translation
{
    public interface ITranslationManager
    {
        event Action<TranslationResponse> TranslateSuccessEvent;
        event Action<DetectLanguageResponse> DetectLanguageSuccessEvent;
        event Action<LanguagesResponse> GetLanguagesSuccessEvent;

        event Action<string> TranslateFailedEvent;
        event Action<string> DetectLanguageFailedEvent;
        event Action<string> GetLanguagesFailedEvent;

        event Action ContentOutOfLengthEvent;

        void Translate(TranslationRequest translationRequest);
        void DetectLanguage(DetectLanguageRequest detectLanguageRequest);
        void GetLanguages(LanguagesRequest languagesRequest);
    }
}