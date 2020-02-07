using FrostweepGames.Plugins.GoogleCloud.Vision.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

namespace FrostweepGames.Plugins.GoogleCloud.Vision
{
    public class FileBrowserPopup
    {
        private IFileManager _fileManager;

        private GameObject _selfObject;

        private Transform _parentOfCDItems,
                          _parentOfContentItems;

        private Text _currentPathText;

        private Button _backButton,
                       _cancelButton,
                       _applyButton;

        private ScrollRect _contentScrollRect;

        private List<FileHierarchyItem> _fileItems;
        private List<FileHierarchyItem> _folderItems;
        private List<DriveHierarchyItem> _cdItems;

        private List<string> _historyOfOpenedPathes;

        private bool _isFileWasSelected;
        private SystemItem _selectedFileItem;



        public FileBrowserPopup()
        {
            _fileManager = GCVision.Instance.ServiceLocator.Get<IFileManager>();
        }

        public void DrawPopup(string path)
        {
            _selfObject = MonoBehaviour.Instantiate(Resources.Load<GameObject>("Prefabs/FileBrowser"));

            _parentOfContentItems = _selfObject.transform.Find("Canvas/FileBrowserPage/Group_Content/View/Group");
            _parentOfCDItems = _selfObject.transform.Find("Canvas/FileBrowserPage/Group_CDs");

            _currentPathText = _selfObject.transform.Find("Canvas/FileBrowserPage/Group_Header/Text_CurrentFolderPath").GetComponent<Text>();
            _backButton = _selfObject.transform.Find("Canvas/FileBrowserPage/Group_Header/Button_Back").GetComponent<Button>();
            _applyButton = _selfObject.transform.Find("Canvas/FileBrowserPage/Group_Bottom/Button_Apply").GetComponent<Button>();
            _cancelButton = _selfObject.transform.Find("Canvas/FileBrowserPage/Group_Bottom/Button_Cancel").GetComponent<Button>();

            _contentScrollRect = _selfObject.transform.Find("Canvas/FileBrowserPage/Group_Content").GetComponent<ScrollRect>();


            _backButton.onClick.AddListener(BackButtonOnClickHandler);
            _applyButton.onClick.AddListener(ApplyButtonOnClickHandler);
            _cancelButton.onClick.AddListener(CancelButtonOnClickHandler);

            _historyOfOpenedPathes = new List<string>();

            FillSystemItemsIn(path);
        }

        public void HidePopup()
        {
            Reset();

            _historyOfOpenedPathes.Clear();

            MonoBehaviour.Destroy(_selfObject);
        }


        private void FillSystemItemsIn(string path, bool ignoreSavePath = false)
        {
            Reset();


            _folderItems = new List<FileHierarchyItem>();
            _fileItems = new List<FileHierarchyItem>();
            _cdItems = new List<DriveHierarchyItem>();

            DirectoryInfo dir = new DirectoryInfo(path);


         
            FileHierarchyItem fileHierarchyItem;
            if (dir.Exists)
            {
                foreach (var item in dir.GetDirectories())
                {
                    fileHierarchyItem = new FileHierarchyItem(_parentOfContentItems, new SystemItem()
                    {
                        name = item.Name,
                        path = item.FullName,
                        type = Enumerators.SystemItemType.FOLDER
                    });

                    fileHierarchyItem.FileHierarchyItemSelectedEvent += FolderFileHierarchyItemSelectedEvent;

                    _folderItems.Add(fileHierarchyItem);
                }

                foreach (var item in dir.GetFiles())
                {
                    fileHierarchyItem = new FileHierarchyItem(_parentOfContentItems, new SystemItem()
                    {
                        name = item.Name,
                        path = item.FullName,
                        type = Enumerators.SystemItemType.FILE
                    });

                    fileHierarchyItem.FileHierarchyItemSelectedEvent += FileHierarchyItemSelectedEvent; ;

                    _fileItems.Add(fileHierarchyItem);
                }
            }

            DriveHierarchyItem driveHierarchyItem;
            foreach (var item in Directory.GetLogicalDrives())
            {
                driveHierarchyItem = new DriveHierarchyItem(_parentOfCDItems, new SystemItem()
                {
                    name = item,
                    path = item,
                    type = Enumerators.SystemItemType.CD
                });

                driveHierarchyItem.DriveHierarchyItemSelectedEvent += DriveHierarchyItemSelectedEvent; ;

                _cdItems.Add(driveHierarchyItem);
            }

            InternalTools.FixVerticalLayoutGroupFitting(_parentOfContentItems);
            InternalTools.FixVerticalLayoutGroupFitting(_parentOfCDItems);

            _contentScrollRect.verticalNormalizedPosition = 1f;

            _currentPathText.text = path;

            if (!ignoreSavePath)
                _historyOfOpenedPathes.Add(path);
        }

        private void DriveHierarchyItemSelectedEvent(DriveHierarchyItem drive)
        {
            _selectedFileItem = null;
            FillSystemItemsIn(drive.systemItem.path);
        }

        private void FileHierarchyItemSelectedEvent(FileHierarchyItem file)
        {
            _selectedFileItem = file.systemItem;
        }

        private void FolderFileHierarchyItemSelectedEvent(FileHierarchyItem folder)
        {
            _selectedFileItem = null;
            FillSystemItemsIn(folder.systemItem.path);
        }

        private void Reset()
        {
            if(_fileItems != null)
            {
                foreach (var item in _fileItems)
                    item.Dispose();
                _fileItems.Clear();
                _fileItems = null;
            }

            if (_folderItems != null)
            {
                foreach (var item in _folderItems)
                    item.Dispose();
                _folderItems.Clear();
                _folderItems = null;
            }

            if (_cdItems != null)
            {
                foreach (var item in _cdItems)
                    item.Dispose();
                _cdItems.Clear();
                _cdItems = null;
            } 
        }

        private void BackButtonOnClickHandler()
        {
            if (_historyOfOpenedPathes.Count > 1)
            {
                FillSystemItemsIn(_historyOfOpenedPathes[_historyOfOpenedPathes.Count - 2], true);
                _historyOfOpenedPathes.RemoveAt(_historyOfOpenedPathes.Count - 2);
            }
        }

        private void ApplyButtonOnClickHandler()
        {
            if (_selectedFileItem == null)
                return;

            _fileManager.SelectFile(_selectedFileItem);
            _fileManager.HideFileBrowser();
        }

        private void CancelButtonOnClickHandler()
        {
            _fileManager.HideFileBrowser();
        }
    }

    public class DriveHierarchyItem
    {
        public event Action<DriveHierarchyItem> DriveHierarchyItemSelectedEvent;

        private GameObject _selfObject;

        private Button _selectButton;
        private Text _contentText;

        public SystemItem systemItem;


        public DriveHierarchyItem(Transform parent, SystemItem item)
        {
            systemItem = item;

            _selfObject = MonoBehaviour.Instantiate(Resources.Load<GameObject>("Prefabs/Item_DriveObject"));
            _selfObject.transform.SetParent(parent, false);


            _selectButton = _selfObject.GetComponent<Button>();
            _contentText = _selfObject.transform.Find("Text_Name").GetComponent<Text>();

            _selectButton.onClick.AddListener(SelectButonOnClickHandler);

            _contentText.text = systemItem.name;
        }

        private void SelectButonOnClickHandler()
        {
            if (DriveHierarchyItemSelectedEvent != null)
                DriveHierarchyItemSelectedEvent(this);
        }

        public void Dispose()
        {
            MonoBehaviour.Destroy(_selfObject);
        }
    }

    public class FileHierarchyItem
    {
        public event Action<FileHierarchyItem> FileHierarchyItemSelectedEvent;

        private GameObject _selfObject;

        private Button _selectButton;
        private Text _contentText;
        private UnityEngine.UI.Image _iconImage;


        public SystemItem systemItem;


        public FileHierarchyItem(Transform parent, SystemItem item)
        {
            systemItem = item;

            _selfObject = MonoBehaviour.Instantiate(Resources.Load<GameObject>("Prefabs/Item_SystemObject"));
            _selfObject.transform.SetParent(parent, false);


            _selectButton = _selfObject.GetComponent<Button>();
            _contentText = _selfObject.transform.Find("Text_Name").GetComponent<Text>();
            _iconImage = _selfObject.transform.Find("Image_Icon").GetComponent<UnityEngine.UI.Image>();

            _selectButton.onClick.AddListener(SelectButonOnClickHandler);

            FillData();
        }

        private void FillData()
        {
            switch(systemItem.type)
            {
                case Enumerators.SystemItemType.FILE:
                    {
                        _iconImage.sprite = Resources.Load<Sprite>("Images/file");
                    }
                    break;
                case Enumerators.SystemItemType.FOLDER:
                    {
                        _iconImage.sprite = Resources.Load<Sprite>("Images/folder");
                    }
                    break;
                default: break;
            }

            _contentText.text = systemItem.name;
        }

        private void SelectButonOnClickHandler()
        {
            if (FileHierarchyItemSelectedEvent != null)
                FileHierarchyItemSelectedEvent(this);
        }

        public void Dispose()
        {
            MonoBehaviour.Destroy(_selfObject);
        }

    }

}