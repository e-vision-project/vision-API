using UnityEngine;
using System;
using System.Collections.Generic;

namespace FrostweepGames.Plugins.GoogleCloud.Vision
{
    public class FileManager : IService, IDisposable, IFileManager
    {
        public event Action<SystemItem> OnSystemItemSelectedEvent;


        private FileBrowserPopup _fileBrowserPopup;

        public void Dispose()
        {
        }

        public void Init()
        {
            _fileBrowserPopup = new FileBrowserPopup();
        }

        public void Update()
        {
        }

        public void DrawFileBrowser()
        {
            _fileBrowserPopup.DrawPopup(Application.dataPath);
        }

        public void HideFileBrowser()
        {
            _fileBrowserPopup.HidePopup();
        }


        public void SelectFile(SystemItem systemItem)
        {
            if (OnSystemItemSelectedEvent != null)
                OnSystemItemSelectedEvent(systemItem);
        }
       

    }

    public class SystemItem
    {
        public string path,
                      name;

        public Enumerators.SystemItemType type;

        public SystemItem()
        {
            type = Enumerators.SystemItemType.UNKNOWN;
        }
    }
}