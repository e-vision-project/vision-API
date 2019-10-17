using System;

namespace FrostweepGames.Plugins.GoogleCloud.VideoIntelligence
{
    public interface IVideoIntelligenceManager
    {
        event Action<AnnotateResponse, long> AnnotateSuccessEvent;
        event Action<string, long> AnnotateFailedEvent;

        event Action<Operation, long> GetSuccessEvent;
        event Action<string, long> GetFailedEvent;

        event Action<ListOperationResponse, long> ListSuccessEvent;
        event Action<string, long> ListFailedEvent;

        event Action<string> CancelSuccessEvent;
        event Action<string, long> CancelFailedEvent;

        event Action<string> DeleteSuccessEvent;
        event Action<string, long> DeleteFailedEvent;

        void Annotate(AnnotateVideoRequest request);
        void Get(string name);
        void List(string name, string filter, double pageSize, string pageToken);
        void Cancel(string name);
        void Delete(string name);
    }
}