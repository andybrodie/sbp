using System;
using System.Collections.ObjectModel;
using System.Windows.Input;
using System.Windows.Navigation;
using Locima.SlidingBlock.IO;
using Locima.SlidingBlock.Messaging;
using Locima.SlidingBlock.Persistence;

namespace Locima.SlidingBlock.ViewModel
{
    public class HighScoresViewModel : ViewModelBase
    {
        public const string HighScoreTableViewObjectName = "HighScoreTableControl";
        private string _buttonLabel;
        private HighScoreTable _model;

        public HighScoresViewModel()
        {
            HighScores = new ObservableCollection<HighScoreItemViewModel>();
            ClickCommand = new DelegateCommand(HandleClick);
        }

        public ObservableCollection<HighScoreItemViewModel> HighScores { get; private set; }

        public ICommand ClickCommand { get; set; }

        public string ButtonLabel
        {
            get { return _buttonLabel; }
            set
            {
                _buttonLabel = value;
                OnNotifyPropertyChanged("ButtonLabel");
            }
        }

        public Uri ButtonUri { get; set; }

        private void HandleClick(object obj)
        {
            if (null==ButtonUri)
            {
                SendViewMessage(new NavigationMessageArgs {NavigationMode = NavigationMode.Back});
            }
            else
            {
                SendViewMessage(new NavigationMessageArgs(ButtonUri));
            }
        }

        /// <summary>
        ///   Load the high score table from storage to <see cref="_model" /> and set up the <see cref="HighScores" /> object based on that model
        /// </summary>
        public void Initialise()
        {
            if (!IsDesignTime)
            {
                _model = HighScoresStorageManager.Instance.Load();
                ResyncViewModelFromModel();
            }
        }


        private void ResyncViewModelFromModel()
        {
            HighScores.Clear();
            HighScoreItemViewModel mostRecent = null;
            foreach (Highscore score in _model.Scores)
            {
                HighScoreItemViewModel scoreVm = new HighScoreItemViewModel(score);
                if (mostRecent == null)
                {
                    mostRecent = scoreVm;
                }
                else
                {
                    if (mostRecent.DateEntered.CompareTo(scoreVm.DateEntered) < 0) mostRecent = scoreVm;
                }
                HighScores.Add(scoreVm);
            }
            _model.Sort();
            if (mostRecent != null)
            {
                mostRecent.IsHighlighted = true;
                SendViewMessage(new ScrollToViewMessage(HighScoreTableViewObjectName, mostRecent));
            }
        }
    }
}