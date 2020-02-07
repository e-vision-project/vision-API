using FrostweepGames.Plugins.GoogleCloud.Vision.Helpers;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace FrostweepGames.Plugins.GoogleCloud.Vision.Examples
{
    public class GC_Vision_Example : MonoBehaviour
    {
        private GCVision _gcVision;
        private IFileManager _fileManager;

        private UnityEngine.UI.Image _workingState;

        private Button _annotateButton,
                       _chooseImageButton;

        private Text _selectedImageText;

        private InputField _imageUrlInputField;

        private bool _isWaitingForResponse = false;

        private Transform _parentOfHeadersGroup,
                          _parentOfContentGroup;

        private List<AnnotateHeaderItem> _annotateHeaderItems;
        private List<AnnotateContentItem> _annotateContentItems;


        private VisionResponse _currentVisionResponse;

        private string _selectedImageData;

        private void Start()
        {
            _gcVision = GCVision.Instance;
            _gcVision.AnnotateSuccessEvent += AnnotateSuccessEventHandler;
            _gcVision.AnnotateFailedEvent += AnnotateFailedEventHandler;

            _fileManager = _gcVision.ServiceLocator.Get<IFileManager>();


            _workingState = transform.Find("Canvas/Image_RecordState").GetComponent<UnityEngine.UI.Image>();

            _imageUrlInputField = transform.Find("Canvas/InputField_Url").GetComponent<InputField>();

            _selectedImageText = transform.Find("Canvas/Text_ImageName").GetComponent<Text>();

            _annotateButton = transform.Find("Canvas/Button_Annotate").GetComponent<Button>();
            _chooseImageButton = transform.Find("Canvas/Button_ChooseImage").GetComponent<Button>();

            _parentOfHeadersGroup = transform.Find("Canvas/Panel_Content/Group_Header");
            _parentOfContentGroup = transform.Find("Canvas/Panel_Content/Group_Content/Group");

            _annotateButton.onClick.AddListener(AnnotateButtonOnClickHandler);
            _chooseImageButton.onClick.AddListener(ChooseImageButtonOnClickHandler);

            _selectedImageData = string.Empty;
            _selectedImageText.text = "No Image selected.";
            _workingState.color = UnityEngine.Color.green;
        }

        private void OnDestroy()
        {
            _gcVision.AnnotateSuccessEvent -= AnnotateSuccessEventHandler;
            _gcVision.AnnotateFailedEvent -= AnnotateFailedEventHandler;
        }

       
        private void AnnotateFailedEventHandler(string obj, long requestIndex)
        {
            _isWaitingForResponse = false;

            Debug.Log("Annotate Image failed with error: " + obj);

            _workingState.color = UnityEngine.Color.green;
        }

        private void AnnotateSuccessEventHandler(VisionResponse obj, long requestIndex)
        {
            _isWaitingForResponse = false;

            _workingState.color = UnityEngine.Color.green;

            _currentVisionResponse = obj;

            FillHeaders();
        }

        private void AnnotateButtonOnClickHandler()
        {
            if (_isWaitingForResponse)
                return;

            if (string.IsNullOrEmpty(_selectedImageData) && string.IsNullOrEmpty(_imageUrlInputField.text))
                return;

            _currentVisionResponse = null;

            _isWaitingForResponse = true;


            //include all features
            var features = new List<Feature>();
            for (int i = 0; i < Enum.GetNames(typeof(Enumerators.FeatureType)).Length; i++)
                features.Add(new Feature() { maxResults = 10, type = ((Enumerators.FeatureType)i).ToString() });


            var img = new Image();
            if (string.IsNullOrEmpty(_selectedImageData))
            {
                img.source = new ImageSource()
                {
                    imageUri = _imageUrlInputField.text,
                    gcsImageUri = string.Empty,
                };

                img.content = string.Empty;
            }
            else
            {
                img.source = new ImageSource()
                {
                    imageUri = string.Empty,
                    gcsImageUri = string.Empty,
                };

                img.content = _selectedImageData;
            }


            _gcVision.Annotate(new List<AnnotateRequest>()
            {
                new AnnotateRequest()
                {
                    image = img,
                    context = new ImageContext()
                    {
                        cropHintsParams = new CropHintsParams()
                        {
                            aspectRatios = new double[] { 1, 2 }
                        },
                        languageHints = new string[]
                        {
                            "english"
                        },
                        latLongRect = new LatLongRect()
                        {
                            maxLatLng = new LatLng()
                            {
                                latitude = 0,
                                longitude = 0
                            },
                            minLatLng = new LatLng()
                            {
                                latitude = 0,
                                longitude = 0
                            }
                        }
                    },
                    features = features
                }
            });

            _workingState.color = UnityEngine.Color.yellow;

            _selectedImageData = string.Empty;
            _selectedImageText.text = "No Image selected.";
            _imageUrlInputField.text = string.Empty;
        }

        private void ChooseImageButtonOnClickHandler()
        {
            _fileManager.OnSystemItemSelectedEvent += OnSystemItemSelectedEventHandler;
            _fileManager.DrawFileBrowser();
        }

        private void OnSystemItemSelectedEventHandler(SystemItem item)
        {
            _fileManager.OnSystemItemSelectedEvent -= OnSystemItemSelectedEventHandler;

            if (InternalTools.IsValidImageFile(item.path))
            {
                _selectedImageText.text = item.name;
               _selectedImageData = ImageConvert.Convert(ImageConvert.GetTextureFromPath(item.path));
            }
            else
            {
                _selectedImageText.text = "File isn't valid.";
            }
        }

        private void FillHeaders()
        {
            ResetHeaders();

            _annotateHeaderItems = new List<AnnotateHeaderItem>();

            int length = Enum.GetNames(typeof(Enumerators.ResponseDataType)).Length;
            AnnotateHeaderItem newItem;

            for (int i = 0; i < length; i++)
            {
                newItem = new AnnotateHeaderItem(_parentOfHeadersGroup, (Enumerators.ResponseDataType)i);
                newItem.SelectAnnotateHeaderItemEvent += SelectAnnotateHeaderItemEventHandler;
                _annotateHeaderItems.Add(newItem);
            }

            SelectAnnotateHeaderItemEventHandler(_annotateHeaderItems[0]);
        }

        private void SelectAnnotateHeaderItemEventHandler(AnnotateHeaderItem headerItem)
        {
            foreach (var item in _annotateHeaderItems)
            {
                if (item != headerItem)
                    item.Deselect();
            }
            headerItem.Select();

            FillContentDataByFeatureType(headerItem.selfType);
        }

        private void ResetHeaders()
        {
            if(_annotateHeaderItems != null)
            {
                foreach (var item in _annotateHeaderItems)
                    item.Dispose();
                _annotateHeaderItems.Clear();
                _annotateHeaderItems = null;
            }
        }

        private void FillContentDataByFeatureType(Enumerators.ResponseDataType type)
        {
            if (_currentVisionResponse == null)
                return;

            ResetContentData();

            _annotateContentItems = new List<AnnotateContentItem>();
            AnnotateContentItem newItem;

            foreach (var response in _currentVisionResponse.responses)
            {
                switch(type)
                {
                    case Enumerators.ResponseDataType.LABELS:
                        {
                            GenerateObjectsFromArray(response.labelAnnotations, Enumerators.DataType.LABELS);
                        }
                        break;
                    case Enumerators.ResponseDataType.LOGOS:
                        {
                            GenerateObjectsFromArray(response.logoAnnotations, Enumerators.DataType.LOGOS);
                        }
                        break;
                    case Enumerators.ResponseDataType.TEXT:
                        {
                            GenerateObjectsFromArray(response.textAnnotations, Enumerators.DataType.TEXT);
                        }
                        break;
                    case Enumerators.ResponseDataType.PROPERTIES:
                        {
                            if (response.imagePropertiesAnnotation == null)
                                break;

                            GenerateObjectsFromArray(response.imagePropertiesAnnotation.dominantColors.colors, Enumerators.DataType.DOMINANT_COLORS);
                        }
                        break;
                    case Enumerators.ResponseDataType.WEB:
                        {
                            if (response.webDetection == null)
                                break;

                            GenerateObjectsFromArray(response.webDetection.fullMatchingImages, Enumerators.DataType.FULLY_MATCHED_IMAGES);
                            GenerateObjectsFromArray(response.webDetection.pagesWithMatchingImages, Enumerators.DataType.PAGES_WITH_MATHCHED_IMAGES);
                            GenerateObjectsFromArray(response.webDetection.partialMatchingImages, Enumerators.DataType.PARTIALLY_MATCHED_IMAGES);
                            GenerateObjectsFromArray(response.webDetection.visuallySimilarImages, Enumerators.DataType.VISUAL_SIMILAR_IMAGES);
                            GenerateObjectsFromArray(response.webDetection.webEntities, Enumerators.DataType.WEB_ENTITIES);
                        }
                        break;
                    case Enumerators.ResponseDataType.SAFE_SEARCH:
                        {
                            if (response.safeSearchAnnotation == null)
                                break;

                            newItem = new AnnotateContentItem(_parentOfContentGroup, response.safeSearchAnnotation, Enumerators.DataType.SAFE_SEARCHS);
                            _annotateContentItems.Add(newItem);
                        }
                        break;
                    case Enumerators.ResponseDataType.DOCUMENT:
                        {
                            if (response.fullTextAnnotation == null)
                                break;

                           GenerateObjectsFromArray(response.fullTextAnnotation.pages, Enumerators.DataType.PAGES);
                        }
                        break;
                    default: break;
                }
            }
        }

        private void GenerateObjectsFromArray(object[] array, Enumerators.DataType type)
        {
            if (array == null)
                return;

            AnnotateContentItem newItem;

            foreach (var item in array)
            {
                if (item == null)
                    continue;

                newItem = new AnnotateContentItem(_parentOfContentGroup, item, type);
                _annotateContentItems.Add(newItem);

                if (type == Enumerators.DataType.PAGES)
                {
                    foreach (var block in (item as Page).blocks)
                    {
                        newItem = new AnnotateContentItem(_parentOfContentGroup, block, Enumerators.DataType.BLOCKS);
                        _annotateContentItems.Add(newItem);

                        foreach (var paragraph in block.paragraphs)
                        {
                            newItem = new AnnotateContentItem(_parentOfContentGroup, paragraph, Enumerators.DataType.PARAGRAPH);
                            _annotateContentItems.Add(newItem);
                        }
                    }
                }
            }
        }

        private void ResetContentData()
        {
            if (_annotateContentItems != null)
            {
                foreach (var item in _annotateContentItems)
                    item.Dispose();
                _annotateContentItems.Clear();
                _annotateContentItems = null;
            }
        }
    }
    public class AnnotateHeaderItem
    {
        public event Action<AnnotateHeaderItem> SelectAnnotateHeaderItemEvent;

        private GameObject _selfObject;

        private UnityEngine.UI.Image _selectImage;
        private Button _selectButton;
        private Text _nameText;

        public bool isSelected;

        public Enumerators.ResponseDataType selfType;

        public AnnotateHeaderItem(Transform parent, Enumerators.ResponseDataType type)
        {
            selfType = type;

            _selfObject = MonoBehaviour.Instantiate(Resources.Load<GameObject>("UI/Elements/Item_Type"));
            _selfObject.transform.SetParent(parent, false);

            _selectButton = _selfObject.GetComponent<Button>();
            _selectImage = _selfObject.GetComponent<UnityEngine.UI.Image>();
            _nameText = _selfObject.transform.Find("Text_Name").GetComponent<Text>();

            _selectButton.onClick.AddListener(SelectButtonOnClickHandler);

            _nameText.text = selfType.ToString().Replace("_", " ");
        }


        public void Select()
        {
            isSelected = true;
            _selectImage.color = UnityEngine.Color.grey;
        }

        public void Deselect()
        {
            isSelected = false;
            _selectImage.color = UnityEngine.Color.white;
        }

        public void Dispose()
        {
            MonoBehaviour.Destroy(_selfObject);
        }

        private void SelectButtonOnClickHandler()
        {
            if (isSelected)
                return;

            if (SelectAnnotateHeaderItemEvent != null)
                SelectAnnotateHeaderItemEvent(this);
        }
    }

    public class AnnotateContentItem
    {
        private GameObject _selfObject;

        private Text _nameText;
        private UnityEngine.UI.Image _selfImage;
        private Button _selectButton;

        private Enumerators.DataType _selfDataType;

        public AnnotateContentItem(Transform parent, object value, Enumerators.DataType dataType)
        {
            _selfDataType = dataType;

            _selfObject = MonoBehaviour.Instantiate(Resources.Load<GameObject>("UI/Elements/Item_Content"));
            _selfObject.transform.SetParent(parent, false);

            _nameText = _selfObject.GetComponent<Text>();
            _selfImage = _selfObject.transform.Find("Image_Content").GetComponent<UnityEngine.UI.Image>();
            _selectButton = _selfObject.GetComponent<Button>();

            _selfImage.enabled = false;

            FillDataByType(value);
        }


        public void Dispose()
        {
            MonoBehaviour.Destroy(_selfObject);
        }


        private void FillDataByType(object value)
        {
            switch(_selfDataType)
            {
                case Enumerators.DataType.LABELS:
                    {
                        var param = value as EntityAnnotation;
                        _nameText.text = CutString(param.description) + " - " + Math.Round(param.score * 100) + "%"; 
                        _selectButton.onClick.AddListener(() => { Application.OpenURL("https://www.google.com/search?q=" + param.description); });
                    }
                    break;
                case Enumerators.DataType.LOGOS:
                    {
                        var param = value as EntityAnnotation;
                        _nameText.text = CutString(param.description) + " - " + Math.Round(param.score * 100) + "%";
                        _selectButton.onClick.AddListener(() => { Application.OpenURL("https://www.google.com/search?q=" + param.description + "+logos&tbm=isch"); });
                    }
                    break;
                case Enumerators.DataType.WEB_ENTITIES:
                    {
                        var param = value as WebEntity;
                        _nameText.text = CutString(param.description) + " - " + param.score;
                        _selectButton.onClick.AddListener(() => { Application.OpenURL("https://www.google.com/search?q=" + param.description + "+logos&tbm=isch"); });
                    }
                    break;
                case Enumerators.DataType.DOMINANT_COLORS:
                    {
                        _selfImage.enabled = true;

                        var param = value as ColorInfo;
                        _selfImage.color = VisionResponseConverter.ConvertToUnityColor(param.color);
                        _nameText.text = string.Format("RGB({0},{1},{2})", Mathf.RoundToInt(_selfImage.color.r * 255),
                                                                           Mathf.RoundToInt(_selfImage.color.g * 255),
                                                                           Mathf.RoundToInt(_selfImage.color.b * 255));
                    }
                    break;
                case Enumerators.DataType.TEXT:
                    {
                        var param = value as EntityAnnotation;
                        _nameText.text = param.description;
                    }
                    break;
                case Enumerators.DataType.PAGES_WITH_MATHCHED_IMAGES:
                    {
                        var param = value as WebPage;
                        _nameText.text = CutString(param.url);
                        _selectButton.onClick.AddListener(() => { Application.OpenURL(param.url); });
                    }
                    break;
                case Enumerators.DataType.FULLY_MATCHED_IMAGES:
                case Enumerators.DataType.PARTIALLY_MATCHED_IMAGES:
                case Enumerators.DataType.VISUAL_SIMILAR_IMAGES:
                    {
                        var param = value as WebImage;
                        _nameText.text = CutString(param.url);
                        _selectButton.onClick.AddListener(() => { Application.OpenURL(param.url); });
                    }
                    break;
                case Enumerators.DataType.SAFE_SEARCHS:
                    {
                        var param = value as SafeSearchAnnotation;
                        _nameText.text = "Adult - " + param.adult.ToString() + "\n" +
                                         "Mediacal - " + param.medical.ToString() + "\n" + 
                                         "Spoof - " + param.spoof.ToString() + "\n" + 
                                         "Violence - " + param.violence.ToString();
                    }
                    break;
                case Enumerators.DataType.PAGES:
                    {
                        _nameText.text = "Page - ";
                    }
                    break;
                case Enumerators.DataType.BLOCKS:
                    {
                        var param = value as Block;
                        _nameText.text = "Block - " + param.blockType;
                    }
                    break;
                case Enumerators.DataType.PARAGRAPH:
                    {
                        var param = value as Paragraph;
                        string words = string.Empty;
                        foreach (var word in param.words)
                        {
                            foreach(var symbol in word.symbols)
                            {
                                words += symbol.text;
                            }

                            words += ",";
                        }

                        _nameText.text = "Paragraph - " + words;
                    }
                    break;
                default: break;
            }
        }

        private string CutString(string val)
        {
            if (string.IsNullOrEmpty(val))
                return string.Empty;

            if (val.Length <= 32)
                return val;

            return val.Substring(0, 29) + "...";
        }
    }
}