using System;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using Locima.SlidingBlock.Common;
using Locima.SlidingBlock.GameTemplates;
using Locima.SlidingBlock.IO;
using Locima.SlidingBlock.Messaging;
using NLog;

namespace Locima.SlidingBlock.ViewModel
{
    public class LevelEditorViewModel : ViewModelBase
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private bool _createNew;
        private LevelDefinition _currentLevel;
        private GameTemplate _gameTemplate;
        private string _imageText;
        private string _imageTitle;
        private string _licenseTitle;
        private Uri _licenseUri;
        private string _pageTitle;
        private WriteableBitmap _image;
        private string _newImageId;

        public LevelEditorViewModel()
        {
            SaveCommand = new DelegateCommand(SaveAction);
            CancelCommand = new DelegateCommand(CancelAction);
            SelectImageCommand = new DelegateCommand(SelectImageAction);
        }

        public bool CreateNew
        {
            protected get { return _createNew; }
            set
            {
                _createNew = value;
                PageTitle = LocalizationHelper.GetString(_createNew
                                                             ? "AddLevelDefinitionPageTitle"
                                                             : "EditLevelDefinitionPageTitle");
            }
        }


        public string PageTitle
        {
            get { return _pageTitle; }
            set
            {
                _pageTitle = value;
                OnNotifyPropertyChanged("PageTitle");
            }
        }

        public int LevelIndex { protected get; set; }

        public string GameTemplateId { protected get; set; }

        public ICommand SaveCommand { get; set; }

        public ICommand CancelCommand { get; set; }

        public string ImageTitle
        {
            get { return _imageTitle; }
            set
            {
                _imageTitle = value;
                OnNotifyPropertyChanged("ImageTitle");
            }
        }


        public string ImageText
        {
            get { return _imageText; }
            set
            {
                _imageText = value;
                OnNotifyPropertyChanged("ImageText");
            }
        }


        public string LicenseTitle
        {
            get { return _licenseTitle; }
            set
            {
                _licenseTitle = value;
                OnNotifyPropertyChanged("LicenseTitle");
            }
        }


        public Uri LicenseUri
        {
            get { return _licenseUri; }
            set
            {
                _licenseUri = value;
                OnNotifyPropertyChanged("LicenseUri");
            }
        }


        /// <summary>
        /// The command executed when the user selects an image (<see cref="SelectImageAction"/>)
        /// </summary>
        public ICommand SelectImageCommand { get; set; }

        /// <summary>
        /// The image shown on the level editor that is used for this level
        /// </summary>
        public WriteableBitmap Image
        {
            get { return _image; }
            private set
            {
                _image = value;
                OnNotifyPropertyChanged("Image");
            }
        }

        /// <summary>
        /// This is set when a new image has been selected for this level (inside <see cref="ImageChooser"/>) and we're returning to this page.
        /// </summary>
        /// <remarks>
        /// This causes the image on the screen to be updated</remarks>
        public string NewImageId
        {
            private get { return _newImageId; }
            set
            {
                _newImageId = value;
                UpdateImage();
            }
        }


        /// <summary>
        /// When an image has been selected 
        /// </summary>
        /// <param name="unused"></param>
        private void SelectImageAction(object unused)
        {
            SendViewMessage(new NavigationMessageArgs(ImageChooser.CreateNavigationUri(GameTemplateId, _gameTemplate.Levels.IndexOf(_currentLevel))));
        }
        

        private void SaveAction(object obj)
        {
            GameTemplateStorageManager.Instance.Save(_gameTemplate);
            SendViewMessage(NavigationMessageArgs.Back);
        }


        private void CancelAction(object obj)
        {
            SendViewMessage(NavigationMessageArgs.Back);
        }


        public void Initialise()
        {
            _gameTemplate = GameTemplateStorageManager.Instance.Load(GameTemplateId);
            if (CreateNew)
            {
                Logger.Info("Inserting new level at index {0} within {1}", LevelIndex, _gameTemplate.Levels.Count);
                _gameTemplate.Levels.Insert(LevelIndex, new LevelDefinition());
            }
            else
            {
                Logger.Info("Editing existing level {0} of {1}", LevelIndex, _gameTemplate.Levels.Count);
            }
            _currentLevel = _gameTemplate.Levels[LevelIndex];

            // At this point, if a NewImageId has been specified, replace the existing image in the level with the new image
            UpdateImage();

            Image = _currentLevel.GetImage();
            PopulateViewModelWithLevel();
        }


        private void UpdateImage()
        {
            if (!string.IsNullOrEmpty(NewImageId))
            {
                if (Image == null)
                {
                    Logger.Info("Setting image in level to image {0}", NewImageId);
                }
                else
                {
                    Logger.Info("Replacing existing image in level definition with new image {0}", NewImageId);
                }

                // TODO Yuk!  This is because the image is a temporary image, it needs promoting to a persistent image or it'll get deleted the next time we start the app
                WriteableBitmap newImage = ImageStorageManager.Instance.Load(NewImageId);
                string imageId = ImageStorageManager.Instance.Save(newImage);
                _currentLevel.ImageId = imageId;
                
                Image = newImage;
            }
        }


        private void PopulateViewModelWithLevel()
        {
            _currentLevel = _gameTemplate.Levels[LevelIndex];
            ImageTitle = _currentLevel.ImageTitle;
            ImageText = _currentLevel.ImageText;
            if (_currentLevel.License == null)
            {
                _currentLevel.License = new LicenseDefinition();
            }
            else
            {
                LicenseTitle = _currentLevel.License.Title;
                LicenseUri = _currentLevel.License.Link;
            }
        }
    }
}