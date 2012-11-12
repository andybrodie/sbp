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
    public class AddEditGameDefinitionViewModel : ViewModelBase
    {
        private string _author;
        private GameDefinition _gameDefinition;
        private ObservableCollection<LevelDefinitionViewModel> _levelList;
        private string _title;

        public AddEditGameDefinitionViewModel()
        {
            ConfirmSaveAndContinue = new DelegateCommand(ConfirmSaveAndContinueAction);
            ConfirmCancel = new DelegateCommand(ConfirmCancelAction);
            LevelList = new ObservableCollection<LevelDefinitionViewModel>();
        }

        protected ICommand ConfirmSaveAndContinue { get; private set; }

        protected ICommand ConfirmCancel { get; private set; }

        public string GameDefinitionId { get; set; }

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
            RefreshGameDefinition();
            GameDefinitionStorageManager.Instance.Save(_gameDefinition);
        }

        private void RefreshGameDefinition()
        {
            if (_gameDefinition == null)
            {
                _gameDefinition = new GameDefinition();
            }
            _gameDefinition.Title = Title;
            _gameDefinition.Author = Author;
        }


        public void AddEditLevel(bool createNew, int levelIndex)
        {
            if (_gameDefinition != null)
            {
                SendViewMessage(
                    new NavigationMessageArgs(LevelEditor.CreateNavigationUri(_gameDefinition.Id, levelIndex,
                                                                                         createNew)));
            }
            else
            {
                if (_gameDefinition == null)
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
                new NavigationMessageArgs(LevelEditor.CreateNavigationUri(_gameDefinition.Id, levelIndex,
                                                                                     false)));
        }


        public void Initialise()
        {
            if (!string.IsNullOrEmpty(GameDefinitionId))
            {
                _gameDefinition = GameDefinitionStorageManager.Instance.Load(GameDefinitionId);
                Title = _gameDefinition.Title;
                Author = _gameDefinition.Author;
                LevelList.Clear();
                foreach (LevelDefinition level in _gameDefinition.Levels)
                {
                    LevelList.Add(new LevelDefinitionViewModel(level));
                }
            }
        }
    }
}