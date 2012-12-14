using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using Locima.SlidingBlock.Common;
using Locima.SlidingBlock.GameTemplates;
using Locima.SlidingBlock.IO;
using Locima.SlidingBlock.Messaging;
using NLog;

namespace Locima.SlidingBlock.ViewModel
{
    /// <summary>
    /// The MVVM view model for the game editor
    /// </summary>
    public class GameEditorViewModel : ViewModelBase
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Backing field for <see cref="Author"/>
        /// </summary>
        private string _author;


        /// <summary>
        /// The game template currently being edited (or created if new), it won't be saved until <see cref="SaveTemplateAndResyncViewModel"/> is called
        /// </summary>
        private GameTemplate _gameTemplate;

        /// <summary>
        /// Backing field for <see cref="LevelList"/>
        /// </summary>
        private ObservableCollection<LevelDefinitionViewModel> _levelList;

        /// <summary>
        /// Backing field for <see cref="Title"/>
        /// </summary>
        private string _title;

        /// <summary>
        /// Wires up <see cref="ICommand"/> instances within this view model and initialises <see cref="LevelList"/>
        /// </summary>
        public GameEditorViewModel()
        {
            ConfirmCancelCommand = new DelegateCommand(ConfirmCancelAction);
            CancelDiscardChangesCommand =
                new DelegateCommand(o => Logger.Debug("User didn't want to discard changes after all"));
            DiscardChangesCommand = new DelegateCommand(DiscardChangesAction);
            LevelList = new ObservableCollection<LevelDefinitionViewModel>();
        }

        /// <summary>
        /// When executed this will ask the user if they're sure they wish to discard their changes, and if confirmed will delete and nchanges made 
        /// by the user and navigate back to the <see cref="GameTemplateSelector"/> page.
        /// </summary>
        public ICommand ConfirmCancelCommand { get; private set; }

        /// <summary>
        /// The ID of the template to edit
        /// </summary>
        public string GameTemplateId { get; set; }

        /// <summary>
        /// The title of the game as seen by the view
        /// </summary>
        public string Title
        {
            get { return _title; }
            set
            {
                _title = value;
                OnNotifyPropertyChanged("Title");
            }
        }


        /// <summary>
        /// The author of the game as seen by the view
        /// </summary>
        public string Author
        {
            get { return _author; }
            set
            {
                _author = value;
                OnNotifyPropertyChanged("Author");
            }
        }


        /// <summary>
        /// The list of levels as seen by the view
        /// </summary>
        public ObservableCollection<LevelDefinitionViewModel> LevelList
        {
            get { return _levelList; }
            private set
            {
                _levelList = value;
                OnNotifyPropertyChanged("LevelList");
            }
        }


        /// <summary>
        /// Invoked if the user elects to cancel request to discard their changes as a result of <see cref="ConfirmCancelCommand"/>
        /// </summary>
        protected ICommand CancelDiscardChangesCommand { get; set; }

        /// <summary>
        /// Invoke to discard any changes the user has made to the game template
        /// </summary>
        protected ICommand DiscardChangesCommand { get; set; }

        /// <summary>
        /// Action for <see cref="DiscardChangesCommand"/> to discard any changes made to the current game template by discard the shadow and navigating back to the game template selector
        /// </summary>
        /// <param name="obj"></param>
        private void DiscardChangesAction(object obj)
        {
            // The user has opted to discard all their changes to a template, so delete any shadow and go back
            if (_gameTemplate.IsShadow)
            {
                Logger.Info("Discarding shadow {0} and navigating back", _gameTemplate);
                GameTemplateStorageManager.Instance.Delete(_gameTemplate.Id);
            }
            SendViewMessage(NavigationMessageArgs.Back);
        }


        private void ConfirmCancelAction(object obj)
        {
            SendViewMessage(new ConfirmationMessageArgs
                                {
                                    Title = LocalizationHelper.GetString("ConfirmDiscardGameChangesTitle"),
                                    Message = LocalizationHelper.GetString("ConfirmDiscardGameChangesText"),
                                    OnCancelCommand = CancelDiscardChangesCommand,
                                    OnOkCommand = DiscardChangesCommand
                                });
        }


        /// <summary>
        /// Creates a new game template if necessary
        /// </summary>
        /// <remarks>
        /// We may be invoked with no game template to edit, in which a new game template should be created
        /// This is the point at which we decide whether we're going to make a shadow of an existing game, or just create an edit a new game.
        /// </remarks>
        public void Initialise()
        {
            if (!string.IsNullOrEmpty(GameTemplateId))
            {
                _gameTemplate = GameTemplateStorageManager.Instance.Load(GameTemplateId);
                // We might be coming back to a shadow if a previous editing session was terminated due to an external event
                if (_gameTemplate.IsShadow)
                {
                    Logger.Debug("Continuing previous editing session on {0}", _gameTemplate);
                }
                else
                {
                    _gameTemplate.MakeShadow();
                    Logger.Debug("Editing existing game template {0}, so creating shadow {1}", _gameTemplate.ShadowOf,
                                 _gameTemplate.Id);
                }
            }
            else
            {
                Logger.Info("Creating new game template, no shadow required");
                _gameTemplate = new GameTemplate
                                    {
                                        Title = LocalizationHelper.GetString("DefaultGameTemplateTitle"),
                                        Author = PlayerStorageManager.Instance.CurrentPlayer.Name,
                                        Levels = new List<LevelDefinition>()
                                    };
            }
            PopulateViewModelFromModel();
        }


        /// <summary>
        /// Called when the save button is pressed, this will overwrite the original game template with our modified version
        /// </summary>
        public void SaveFinalTemplateChanges()
        {
            _gameTemplate.Title = Title;
            _gameTemplate.Author = Author;
            MakeAllImagesPermanent(_gameTemplate);
            if (_gameTemplate.IsShadow)
            {
                Logger.Info("Saving changes to shadow over original {0}", _gameTemplate);
                GameTemplateStorageManager.Instance.PromoteShadow(_gameTemplate);
            }
            else
            {
                Logger.Info("Saving non-shadow template {0}", _gameTemplate);
                GameTemplateStorageManager.Instance.Save(_gameTemplate);
            }
            SendViewMessage(NavigationMessageArgs.Back);
        }


        /// <summary>
        /// Searches <paramref name="gameTemplate"/> for the use of any temporary images and converts them to permanent ones
        /// </summary>
        /// <param name="gameTemplate">The game template to search for temporary images within</param>
        private void MakeAllImagesPermanent(GameTemplate gameTemplate)
        {
            foreach (LevelDefinition level in gameTemplate.Levels)
            {
                if (level.ImageId != null)    // Ignore any levels which don't have an image (usually because it's in the XapImageUri field)
                {
                    if (ImageStorageManager.Instance.IsTemporary(level.ImageId))
                    {
                        // Promotion will change the ID
                        level.ImageId = ImageStorageManager.Instance.Promote(level.ImageId);
                    }
                }
            }

        }


        /// <summary>
        /// Inserts a level at the index specified by <paramref name="insertPoint"/>
        /// </summary>
        /// <param name="insertPoint">The index of the new level</param>
        private void InsertLevelAt(int insertPoint)
        {
            Logger.Info("Creating new level at index {0}", insertPoint);
            LevelDefinition newLevel = new LevelDefinition
                                           {
                                               ImageTitle = LocalizationHelper.GetString("DefaultLevelTitle"),
                                               ImageText = LocalizationHelper.GetString("DefaultLevelText"),
                                               License = LicenseDefinition.CcBy30,
                                               XapImageUri = new Uri("GameTemplates/DefaultImage.jpg", UriKind.Relative)
                                           };
            _gameTemplate.Levels.Insert(insertPoint, newLevel);
            SaveTemplateAndResyncViewModel();
        }




        /// <summary>
        /// If <paramref name="createNew"/> is <c>true</c> then this adds a new level at the <paramref name="levelIndex"/> specified and navigates to <see cref="LevelEditor"/>
        /// If <paramref name="createNew"/> is <c>false</c> then this navigates to <see cref="LevelEditor"/> editing the existing level at <paramref name="levelIndex"/>
        /// </summary>
        /// <param name="createNew">If <c>true</c> a new level will be created, otherwise an existing level at <paramref name="levelIndex"/> will be edited</param>
        /// <param name="levelIndex">The index of the level to either create or edit (depending on <paramref name="createNew"/></param>
        public void AddEditLevel(bool createNew, int levelIndex)
        {
            if (createNew)
            {
                InsertLevelAt(levelIndex);
            }
            else
            {
                // Need to save the template if we're editing an existing game template causing a new shadow to be created but it hasn't been saved yet
                SaveTemplateAndResyncViewModel();
            }
            SendViewMessage(
                new NavigationMessageArgs(LevelEditor.CreateNavigationUri(_gameTemplate.Id, levelIndex, createNew)));
        }


        /// <summary>
        /// Adds a level before <paramref name="level"/>
        /// </summary>
        /// <param name="level">The level after which to insert a new level</param>
        /// <remarks><see cref="ICommand"/> action for <see cref="LevelDefinitionViewModel.AddLevelAfterCommand"/></remarks>
        public void AddLevelAfter(LevelDefinition level)
        {
            int insertPoint = _gameTemplate.Levels.IndexOf(level);
            InsertLevelAt(insertPoint + 1);
        }


        /// <summary>
        /// Adds a level after <paramref name="level"/>
        /// </summary>
        /// <param name="level">The level before which to insert a new level</param>
        /// <remarks><see cref="ICommand"/> action for <see cref="LevelDefinitionViewModel.AddLevelBeforeCommand"/></remarks>
        public void AddLevelBefore(LevelDefinition level)
        {
            int insertPoint = _gameTemplate.Levels.IndexOf(level);
            InsertLevelAt(insertPoint);
        }


        /// <summary>
        /// Moves the <paramref name="relativePosition"/> number of levels up (if negative) or down (if positive)
        /// </summary>
        /// <param name="level">The level to move</param>
        /// <param name="relativePosition">The new relative position (0 means stay still) of the level to its original location</param>
        /// <remarks><see cref="ICommand"/> action for <see cref="LevelDefinitionViewModel.MoveLeveCommand"/></remarks>
        public void MoveLevel(LevelDefinition level, int relativePosition)
        {
            int index = _gameTemplate.Levels.IndexOf(level);
            int newIndex = index + relativePosition;
            if (newIndex < 0 || newIndex > _gameTemplate.Levels.Count - 1)
            {
                Logger.Debug("Ignoring call to move level {0} by {1} because it would be out of range", index,
                             relativePosition);
            }
            else
            {
                Logger.Info("Swapping levels {0} and {1}", index, newIndex);
                LevelDefinition temp = _gameTemplate.Levels[index];
                _gameTemplate.Levels[index] = _gameTemplate.Levels[newIndex];
                _gameTemplate.Levels[newIndex] = temp;
                SaveTemplateAndResyncViewModel();
            }
        }


        /// <summary>
        /// Deletes the passed level from the <see cref="_gameTemplate"/>
        /// </summary>
        /// <remarks><see cref="ICommand"/> action for <see cref="LevelDefinitionViewModel.DeleteLevelCommand"/></remarks>
        public void DeleteLevel(LevelDefinition level)
        {
            Logger.Info("Deleting level {0}", this);
            // Don't forget Andy, you CAN'T delete the image in level.ImageId yet, because the user might decide not to save change.
            _gameTemplate.Levels.Remove(level);
            SaveTemplateAndResyncViewModel();
        }


        /// <summary>
        /// Refreshes <see cref="Title"/>, <see cref="Author"/> and the list of levels in the view model from <see cref="_gameTemplate"/>
        /// </summary>
        private void PopulateViewModelFromModel()
        {
            Title = _gameTemplate.Title;
            Author = _gameTemplate.Author;
            LevelList.Clear();
            foreach (LevelDefinition level in _gameTemplate.Levels)
            {
                LevelList.Add(new LevelDefinitionViewModel(this, level));
            }
        }


        /// <summary>
        /// Saves any changes made by the user and resyncs the view model fields that the view is using back to the model
        /// </summary>
        public void SaveTemplateAndResyncViewModel()
        {
            _gameTemplate.Title = Title;
            _gameTemplate.Author = Author;
            GameTemplateStorageManager.Instance.Save(_gameTemplate);
            PopulateViewModelFromModel();
        }

    }
}