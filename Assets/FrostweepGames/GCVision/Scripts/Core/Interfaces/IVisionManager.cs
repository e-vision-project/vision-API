using System;
using System.Collections.Generic;

namespace FrostweepGames.Plugins.GoogleCloud.Vision
{
    public interface IVisionManager
    {
        event Action<VisionResponse, long> AnnotateSuccessEvent;
        event Action<string, long> AnnotateFailedEvent;
        void Annotate(List<AnnotateRequest> requestsList);
    }
}