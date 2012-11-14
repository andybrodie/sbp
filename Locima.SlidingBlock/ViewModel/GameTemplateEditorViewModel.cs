using System.Collections.ObjectModel;
using System.Windows.Input;
using Locima.SlidingBlock.GameTemplates;
using Locima.SlidingBlock.IO;
using Locima.SlidingBlock.Messaging;

namespace Locima.SlidingBlock.ViewModel
{
    /// <summary>
    /// 
    /// </summary>
    public class GameTemplateEditorViewModel : ViewModelBase
    {
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


        public void AddEditLevel(bool createNew, int levelIndex)
        {
            if (_gameTemplate != null)
            {
                SendViewMessage(
                    new NavigationMessageArgs(LevelEditor.CreateNavigationUri(_gameTemplate.Id, levelIndex, createNew)));
            }
            else
            {
                if (_gameTemplate == null)
                {
                    SendViewMessage(new ConfirmationMessageArgs
                                        {
                                            Title = "XXX Title",
                                            Message = "XXX You must save the game to continue",
                                            OnOkCommand = ConfirmSaveAndContinue,
                                            OnCancelCommand = ConfirmCancel
                                        });
                }
            }
        }


        public void EditLevel(int levelIndex)
        {
            SendViewMessage(
                new NavigationMessageArgs(LevelEditor.CreateNavigationUri(_gameTemplate.Id, levelIndex,
                                                                                     false)));
        }


        public void Initialise()
        {
            if (!string.IsNullOrEmpty(GameTemplateId))
            {
                _gameTemplate = GameTemplateStorageManager.Instance.Load(GameTemplateId);
                Title = _gameTemplate.Title;
                Author = _gameTemplate.Author;
                LevelList.Clear();
                foreach (LevelDefinition level in _gameTemplate.Levels)
                {
                    LevelList.Add(new LevelDefinitionViewModel(level));
                }
            }
        }
    }
}