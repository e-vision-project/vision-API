using System;

namespace FrostweepGames.Plugins.GoogleCloud.Vision
{
    public interface IFileManager
    {
        event Action<SystemItem> OnSystemItemSelectedEvent;
        void DrawFileBrowser();
        void HideFileBrowser();

        void SelectFile(SystemItem systemItem);
    }
}