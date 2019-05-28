using System.Collections.Generic;

namespace FrostweepGames.Plugins.GoogleCloud.NaturalLanguage
{
    #region requests

    public class AnalyzeEntitiesRequest
    {
        public Document document;
        public Enumerators.EncodingType encodingType;
    }

    public class AnalyzeEntitySentimentRequest
    {
        public Document document;
        public Enumerators.EncodingType encodingType;
    }

    public class AnalyzeSentimentRequest
    {
        public Document document;
        public Enumerators.EncodingType encodingType;
    }

    public class AnalyzeSyntaxRequest
    {
        public Document document;
        public Enumerators.EncodingType encodingType;
    }

    public class AnnotateTextRequest
    {
        public Document document;
        public Features features;
        public Enumerators.EncodingType encodingType;
    }

    public class ClassifyTextRequest
    {
        public Document document;
    }

    #endregion

    #region models

    public class Document
    {
        public Enumerators.DocumentType type;

        public string language;
    }

    public class GCSDocument : Document
    {
        public string gcsContentUri;
    }

    public class LocalDocument : Document
    {
        public string content;
    }


    public class Entity
    {
        public string name;
        public Enumerators.EntityType type;
        public double salience;
        public EntityMention[] mentions;
        public Sentiment sentiment;
        public Dictionary<string, string> metadata;

    }

    public class EntityMention
    {
        public TextSpan text;
        public Enumerators.EntityMentionType type;
        public Sentiment sentiment;
    }

    public class Sentiment
    {
        public double magnitude,
                      score;
    }

    public class TextSpan
    {
        public string content;
        public double beginOffset;
    }

    public class Sentence
    {
        public TextSpan text;
        public Sentiment sentiment;
    }

    public class Token
    {
        public TextSpan text;
        public PartOfSpeech partOfSpeech;
        public DependencyEdge dependencyEdge;
        public string lemma;
    }

    public class PartOfSpeech
    {
        public Enumerators.Tag tag;
        public Enumerators.Aspect aspect;
        public Enumerators.Case @case;
        public Enumerators.Form form;
        public Enumerators.Gender gender;
        public Enumerators.Mood mood;
        public Enumerators.Number number;
        public Enumerators.Person person;
        public Enumerators.Proper proper;
        public Enumerators.Reciprocity reciprocity;
        public Enumerators.Tense tense;
        public Enumerators.Voice voice;
    }

    public class DependencyEdge
    {
        public double headTokenIndex;
        public Enumerators.Label label;
    }

    public class Features
    {
        public bool extractSyntax, 
                    extractEntities, 
                    extractDocumentSentiment,
                    extractEntitySentiment;
    }

    public class ClassificationCategory
    {
        public string name;
        public double confidence;
    }

    #endregion

    #region responses

    public class AnalyzeEntitiesResponse
    {
        public Entity[] entities;
        public string language;
    }

    public class AnalyzeEntitySentimentResponse
    {
        public Entity[] entities;
        public string language;
    }

    public class AnalyzeSentimentResponse
    {
        public Sentiment documentSentiment;
        public string language;
        public Sentence[] sentences;
    }

    public class AnalyzeSyntaxResponse
    {
        public Token[] tokens;
        public Sentence[] sentences;
        public string language;
    }

    public class AnnotateTextResponse
    {
        public Token[] tokens;
        public Sentence[] sentences;
        public Entity[] entities;
        public Sentiment documentSentiment;
        public string language;
        public ClassificationCategory[] categories;
    }

    public class ClassifyTextResponse
    {
        public ClassificationCategory[] categories;

    }
    #endregion
}