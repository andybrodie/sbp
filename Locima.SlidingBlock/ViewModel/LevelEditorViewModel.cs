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
        private GameDefinition _gameDefinition;
        private string _imageText;
        private string _imageTitle;
        private string _licenseTitle;
        private Uri _licenseUri;
        private string _pageTitle;
        private WriteableBitmap _image;

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

        public string GameDefinitionId { protected get; set; }

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


        public ICommand SelectImageCommand { get; set; }

        public WriteableBitmap Image { get { return _image; } private set { _image = value;
        OnNotifyPropertyChanged("Image");} }

        private void SelectImageAction(object obj)
        {
            SendViewMessage(new NavigationMessageArgs(ImageChooser.CreateNavigationUri()));
        }

        private void SaveAction(object obj)
        {
            throw new NotImplementedException();
        }

        private void CancelAction(object obj)
        {
            SendViewMessage(NavigationMessageArgs.Back);
        }


        public void Initialise()
        {
            _gameDefinition = GameDefinitionStorageManager.Instance.Load(GameDefinitionId);
            if (CreateNew)
            {
                Logger.Info("Inserting new level at index {0} within {1}", LevelIndex, _gameDefinition.Levels.Count);
                _gameDefinition.Levels.Insert(LevelIndex, new LevelDefinition());
                PageTitle = LocalizationHelper.GetString("CreateLevel");
            }
            else
            {
                Logger.Info("Editing existing level {0} of {1}", LevelIndex, _gameDefinition.Levels.Count);
                PageTitle = LocalizationHelper.GetString("CreateLevel");
            }
            _currentLevel = _gameDefinition.Levels[LevelIndex];
            Image = _currentLevel.GetImage();
            PopulateViewModelWithLevel();
        }


        private void PopulateViewModelWithLevel()
        {
            _currentLevel = _gameDefinition.Levels[LevelIndex];
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