using System;
using System.ComponentModel;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using Locima.SlidingBlock.Common;
using Locima.SlidingBlock.GameTemplates;
using Locima.SlidingBlock.IO;
using Locima.SlidingBlock.Messaging;
using NLog;

namespace Locima.SlidingBlock.ViewModel
{
    /// <summary>
    /// The MVVM View Model for the <see cref="LevelEditor"/>, the model being an instance of <see cref="LevelDefinition"/> within a <see cref="GameTemplate"/>
    /// </summary>
    public class LevelEditorViewModel : ViewModelBase
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Backing field for <see cref="CreateNew"/>
        /// </summary>
        private bool _createNew;

        /// <summary>
        /// The MVVM model for this view model
        /// </summary>
        private LevelDefinition _currentLevel;

        /// <summary>
        /// The game template that this level belongs to, ultimately we save and load this to edit the level
        /// </summary>
        private GameTemplate _gameTemplate;

        /// <summary>
        /// Backing field for <see cref="Image"/>
        /// </summary>
        private WriteableBitmap _image;

        /// <summary>
        /// Backing field for <see cref="ImageText"/>
        /// </summary>
        private string _imageText;

        /// <summary>
        /// Backing field for <see cref="ImageTitle"/>
        /// </summary>
        private string _imageTitle;

        private bool _isEditable;

        /// <summary>
        /// Backing field for <see cref="LicenseTitle"/>
        /// </summary>
        private string _licenseTitle;

        /// <summary>
        /// Backing field for <see cref="LicenseUri"/>
        /// </summary>
        private Uri _licenseUri;

        /// <summary>
        /// Backing field for <see cref="NewImageId"/>
        /// </summary>
        private string _newImageId;

        /// <summary>
        /// Backing field for <see cref="PageTitle"/>
        /// </summary>
        private string _pageTitle;

        /// <summary>
        /// Initialise command instances
        /// </summary>
        public LevelEditorViewModel()
        {
            SaveCommand = new DelegateCommand(SaveAction, IsEditableCanExecuteHandler);
            CancelCommand = new DelegateCommand(CancelAction, IsEditableCanExecuteHandler);
            SelectImageCommand = new DelegateCommand(SelectImageAction, IsEditableCanExecuteHandler);
        }

        /// <summary>
        /// Sets <see cref="PageTitle"/> according to whether we're editing an existing level or creating a new level here
        /// </summary>
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


        /// <summary>
        /// The page title, which changes depending on the value of <see cref="CreateNew"/>
        /// </summary>
        public string PageTitle
        {
            get { return _pageTitle; }
            set
            {
                _pageTitle = value;
                OnNotifyPropertyChanged("PageTitle");
            }
        }

        /// <summary>
        /// The index of the level we're editing within the <see cref="_gameTemplate"/>
        /// </summary>
        public int LevelIndex { protected get; set; }

        /// <summary>
        /// The ID of the game template being edited
        /// </summary>
        public string GameTemplateId { protected get; set; }


        /// <summary>
        /// Command to save changes to the level
        /// </summary>
        public ICommand SaveCommand { get; set; }

        /// <summary>
        /// Command to cancel changes to the level and go back to the <see cref="GameEditor"/>
        /// </summary>
        public ICommand CancelCommand { get; set; }

        /// <summary>
        /// The title for the level/image, maps to <see cref="LevelDefinition.ImageTitle"/>
        /// </summary>
        public string ImageTitle
        {
            get { return _imageTitle; }
            set
            {
                _imageTitle = value;
                OnNotifyPropertyChanged("ImageTitle");
            }
        }


        /// <summary>
        /// The text description of the level/image, maps to <see cref="LevelDefinition.ImageText"/>
        /// </summary>
        public string ImageText
        {
            get { return _imageText; }
            set
            {
                _imageText = value;
                OnNotifyPropertyChanged("ImageText");
            }
        }


        /// <summary>
        /// The title of the license applying to the image in <see cref="Image"/>, maps to <see cref="LicenseDefinition.Title"/>
        /// </summary>
        public string LicenseTitle
        {
            get { return _licenseTitle; }
            set
            {
                _licenseTitle = value;
                OnNotifyPropertyChanged("LicenseTitle");
            }
        }


        /// <summary>
        /// The Url of the license applying to the image in <see cref="Image"/>, maps to <see cref="LicenseDefinition.Link"/>
        /// </summary>
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
        /// If true then the fields of this level may be edited.  This is used by <see cref="ICommand.CanExecute"/> implementations and as a binding
        /// target for <see cref="Control.IsEnabled"/>
        /// </summary>
        public bool IsEditable
        {
            get { return _isEditable; }
            private set
            {
                _isEditable = value;
                OnNotifyPropertyChanged("IsEditable");
            }
        }

        /// <summary>
        /// Used to control various command's executability
        /// </summary>
        /// <param name="o">unused</param>
        /// <returns>The value of <see cref="IsEditable"/></returns>
        private bool IsEditableCanExecuteHandler(object o)
        {
            return IsEditable;
        }


        private void SaveLevel()
        {
            PopuldateModelFromViewModel();
            GameTemplateStorageManager.Instance.Save(_gameTemplate);
        }

        /// <summary>
        /// When an image has been selected 
        /// </summary>
        /// <param name="unused"></param>
        private void SelectImageAction(object unused)
        {
            SaveLevel();
            SendViewMessage(new NavigationMessageArgs(ImageChooser.CreateNavigationUri(GameTemplateId, LevelIndex)));
        }


        private void SaveAction(object obj)
        {
            SaveLevel();
            NavigateBackToGameEditor();
        }


        private void CancelAction(object obj)
        {
            NavigateBackToGameEditor();
        }


        /// <summary>
        /// Navigates to the previous page, which will always be the game editor, taking the creation of shadows in to account
        /// </summary>
        private void NavigateBackToGameEditor()
        {
            Logger.Debug(
                "Creating the illusion of navigating \"back\" to GameEditor by removing backstack entry to it and creating new Uri that refers to the shadow");
            SendViewMessage(new NavigationMessageArgs
                {
                    Uri = GameEditor.CreateNavigationUri(GameTemplateId, 2)
                });
        }


        /// <summary>
        /// Initialise <see cref="_currentLevel"/>, set up the image and populate the view model from <see cref="_currentLevel"/>
        /// </summary>
        public void Initialise()
        {
            PropertyChanged += OnPropertyChanged;
            _gameTemplate = GameTemplateStorageManager.Instance.Load(GameTemplateId);
            Logger.Info("{0} level {0} of {1}", CreateNew ? "Creating" : "Editing", LevelIndex,
                        _gameTemplate.Levels.Count);
            _currentLevel = _gameTemplate.Levels[LevelIndex];

            // At this point, if a NewImageId has been specified, replace the existing image in the level with the new image
            UpdateImage();
            IsEditable = !_gameTemplate.IsReadOnly;
            Image = _currentLevel.GetImage();
            PopulateViewModelWithLevel();
        }

        /// <summary>
        /// This view model cares about property changes to support disabling application bar buttons when <see cref="IsEditable"/> is updated
        /// </summary>
        /// <param name="sender">Ignored</param>
        /// <param name="propertyChangedEventArgs">Need <see cref="PropertyChangedEventArgs.PropertyName"/> to only react to changes to <see cref="IsEditable"/></param>
        private void OnPropertyChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs)
        {
            if ("IsEditable" == propertyChangedEventArgs.PropertyName)
            {
                Logger.Debug("Firing CanExecuteChanged events as IsEditable has been changed");
                ((DelegateCommand) CancelCommand).RaiseCanExecuteChanged();
                ((DelegateCommand) SaveCommand).RaiseCanExecuteChanged();
                ((DelegateCommand) SelectImageCommand).RaiseCanExecuteChanged();
            }
        }


        /// <summary>
        /// Updates the image based on a change to <see cref="NewImageId"/>. 
        /// </summary>
        /// <remarks>
        /// This is used when navigating from the <see cref="ImageChooser"/> to replace the current image with the one specified by the ID set in <see cref="NewImageId"/></remarks>
        private void UpdateImage()
        {
            if (!string.IsNullOrEmpty(NewImageId))
            {
                Logger.Info(
                    Image == null
                        ? "Setting image in level to image {0}"
                        : "Replacing existing image in level definition with new image {0}", NewImageId);

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


        private void PopuldateModelFromViewModel()
        {
            _currentLevel.ImageTitle = ImageTitle;
            _currentLevel.ImageText = ImageText;
            _currentLevel.License = new LicenseDefinition {Title = LicenseTitle, Link = LicenseUri};
        }
    }
}