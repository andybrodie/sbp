using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Input;
using Locima.SlidingBlock.Common;
using Locima.SlidingBlock.GameTemplates;
using Locima.SlidingBlock.IO;
using Locima.SlidingBlock.Messaging;
using NLog;

namespace Locima.SlidingBlock.ViewModel
{
    public class GameTemplateEditorViewModel : ViewModelBase
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private string _author;
        private GameTemplate _gameTemplate;
        private ObservableCollection<LevelDefinitionViewModel> _levelList;
        private string _title;

        public GameTemplateEditorViewModel()
        {
            ConfirmSaveAndContinue = new DelegateCommand(ConfirmSaveAndContinueAction);
            ConfirmCancel = new DelegateCommand(ConfirmCancelAction);
            LevelList = new ObservableCollection<LevelDefinitionViewModel>();
        }

        protected ICommand ConfirmSaveAndContinue { get; private set; }

        protected ICommand ConfirmCancel { get; private set; }

        public string GameTemplateId { get; set; }

        public string Title
        {
            get { return _title; }
            set
            {
                _title = value;
                OnNotifyPropertyChanged("Title");
            }
        }

        public string Author
        {
            get { return _author; }
            set
            {
                _author = value;
                OnNotifyPropertyChanged("Author");
            }
        }

        public ObservableCollection<LevelDefinitionViewModel> LevelList
        {
            get { return _levelList; }
            private set
            {
                _levelList = value;
                OnNotifyPropertyChanged("LevelList");
            }
        }

        private void ConfirmCancelAction(object obj)
        {
            // If the user hit cancel, then take no action as they're rejected the action offered
        }


        private void ConfirmSaveAndContinueAction(object obj)
        {
            RefreshGameTemplate();
            GameTemplateStorageManager.Instance.Save(_gameTemplate);
        }

        private void RefreshGameTemplate()
        {
            if (_gameTemplate == null)
            {
                _gameTemplate = new GameTemplate();
            }
            _gameTemplate.Title = Title;
            _gameTemplate.Author = Author;
        }


        private void SaveShadow()
        {
            Logger.Info("Saving shadow {0}", _gameTemplate);
            if (!_gameTemplate.IsShadow)
            {
                _gameTemplate.MakeShadow();
            }
            GameTemplateStorageManager.Instance.Save(_gameTemplate);
        }


        public void AddEditLevel(bool createNew, int levelIndex)
        {
            SaveShadow();
            SendViewMessage(new NavigationMessageArgs(LevelEditor.CreateNavigationUri(_gameTemplate.Id, levelIndex, createNew)));
        }



        public void Initialise()
        {
            if (!string.IsNullOrEmpty(GameTemplateId))
            {
                _gameTemplate = GameTemplateStorageManager.Instance.Load(GameTemplateId);
            }
            else
            {
                Logger.Info("Creating new game template");
                _gameTemplate = new GameTemplate
                                    {
                                        Title = "XXX Default Title",
                                        Author = PlayerStorageManager.Instance.CurrentPlayer.Name,
                                        Levels = new List<LevelDefinition>()
                                    };
            }
            Title = _gameTemplate.Title;
            Author = _gameTemplate.Author;
            RefreshLevelList();
        }


        public void SaveTemplate()
        {
            if (_gameTemplate.IsShadow)
            {
                Logger.Info("Saving changes to shadow over original");
                GameTemplateStorageManager.Instance.PromoteShadow(_gameTemplate);
            }
            else
            {
                Logger.Info("Ignoring call to Save because this game template isn't a shadow, so we've not changed anything: {0}", _gameTemplate);
            }
            SendViewMessage(NavigationMessageArgs.Back);
        }


        private void InsertLevelAt(int insertPoint)
        {
            Logger.Info("Creating new level at index {0}", insertPoint);
            LevelDefinition newLevel = new LevelDefinition
                                           {
                                               ImageTitle = LocalizationHelper.GetString("DefaultLevelTitle"),
                                               ImageText = LocalizationHelper.GetString("DefaultLevelText"),
                                               License = LicenseDefinition.CcBy30,
                                               XapImageUri =
                                                   new Uri("/GameTemplates/DefaultImage.jpg", UriKind.Relative)
                                           };
            _gameTemplate.Levels.Insert(insertPoint, newLevel);
            SaveShadow();
            RefreshLevelList();
        }

        private void RefreshLevelList()
        {
            LevelList.Clear();
            foreach (LevelDefinition level in _gameTemplate.Levels)
            {
                LevelList.Add(new LevelDefinitionViewModel(this, level));
            }
        }

        public void AddLevelAfter(LevelDefinition level)
        {
            int insertPoint = _gameTemplate.Levels.IndexOf(level);
            InsertLevelAt(insertPoint + 1);
        }


        public void AddLevelBefore(LevelDefinition level)
        {
            int insertPoint = _gameTemplate.Levels.IndexOf(level);
            InsertLevelAt(insertPoint);
        }

        public void MoveLevel(LevelDefinition level, bool moveUp)
        {
            int index = _gameTemplate.Levels.IndexOf(level);
            int offset = 0;
            if (moveUp)
            {
                if (index == 0)
                {
                    Logger.Debug("Ignoring call to move level {0} up because it's already at the top", index);
                }
                else
                {
                    offset = -1;
                }
            }
            else
            {
                if (index == _gameTemplate.Levels.Count - 1)
                {
                    Logger.Debug("Ignoring call to move level {0} down because it's at the bottom", index);
                }
                else
                {
                    offset = 1;
                }
            }
            if (offset != 0)
            {
                Logger.Info("Swapping levels {0} and {1}", index, index + offset);
                LevelDefinition temp = _gameTemplate.Levels[index];
                _gameTemplate.Levels[index] = _gameTemplate.Levels[index + offset];
                _gameTemplate.Levels[index + offset] = temp;
                SaveShadow();
                RefreshLevelList();
            }
        }
    }
}