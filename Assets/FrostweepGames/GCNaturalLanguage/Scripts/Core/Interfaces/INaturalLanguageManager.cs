using System;

namespace FrostweepGames.Plugins.GoogleCloud.NaturalLanguage
{
    public interface INaturalLanguageManager
    {
        event Action<AnalyzeEntitiesResponse> AnalyzeEntitiesSuccessEvent;
        event Action<AnalyzeEntitySentimentResponse> AnalyzeEntitySentimentSuccessEvent;
        event Action<AnalyzeSentimentResponse> AnalyzeSentimentSuccessEvent;
        event Action<AnalyzeSyntaxResponse> AnalyzeSyntaxSuccessEvent;
        event Action<AnnotateTextResponse> AnnotateTextSuccessEvent;
        event Action<ClassifyTextResponse> ClassifyTextSuccessEvent;

        event Action<string> AnalyzeEntitiesFailedEvent;
        event Action<string> AnalyzeEntitySentimentFailedEvent;
        event Action<string> AnalyzeSentimentFailedEvent;
        event Action<string> AnalyzeSyntaxFailedEvent;
        event Action<string> AnnotateTextFailedEvent;
        event Action<string> ClassifyTextFailedEvent;

        string PrepareLanguage(Enumerators.Language lang);

        void Annotate(AnalyzeEntitiesRequest entitiesRequest);
        void Annotate(AnalyzeEntitySentimentRequest entitySentiment);
        void Annotate(AnalyzeSentimentRequest sentimentRequest);
        void Annotate(AnalyzeSyntaxRequest syntaxRequest);
        void Annotate(AnnotateTextRequest textRequest);
        void Annotate(ClassifyTextRequest classifyTextRequest);
    }
}