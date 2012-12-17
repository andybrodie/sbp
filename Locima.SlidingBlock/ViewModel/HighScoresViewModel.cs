using System;
using System.Collections.ObjectModel;
using System.Windows.Input;
using System.Windows.Navigation;
using Locima.SlidingBlock.IO;
using Locima.SlidingBlock.Messaging;
using Locima.SlidingBlock.Persistence;

namespace Locima.SlidingBlock.ViewModel
{

    /// <summary>
    /// View model for the <see cref="HighScores"/> page using <see cref="HighScoreTable"/> as the model.  This class also uses <see cref="HighScoreItemViewModel"/> for individual
    /// entries within the high score table
    /// </summary>
    public class HighScoresViewModel : ViewModelBase
    {

        /// <summary>
        /// The name of the ListBox control that contains the high score entries.
        /// </summary>
        /// <remarks>
        /// This is used to allow the <see cref="ScrollToViewMessage"/> event handler to identify the ListBox to scroll to</remarks>
        public const string HighScoreTableViewObjectName = "HighScoreTableControl";

        /// <summary>
        /// Backing field for <see cref="ButtonLabel"/>
        /// </summary>
        private string _buttonLabel;

        /// <summary>
        /// Backing field for <see cref="Model"/>
        /// </summary>
        private HighScoreTable _model;


        /// <summary>
        /// Initialises <see cref="HighScores"/> with an empty collection and <see cref="ClickCommand"/>
        /// </summary>
        public HighScoresViewModel()
        {
            HighScores = new ObservableCollection<HighScoreItemViewModel>();
            ClickCommand = new DelegateCommand(HandleClick);
        }

        /// <summary>
        /// The list of all the high scores
        /// </summary>
        /// <remarks>
        /// This is the list of items to display in the high score table
        /// </remarks>
        public ObservableCollection<HighScoreItemViewModel> HighScores { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        public ICommand ClickCommand { get; set; }

        /// <summary>
        /// The label for the button that allows the user to navigate to the "next" page after the high score screen.  This changes depending on how the high score table page was arrived at
        /// </summary>
        public string ButtonLabel
        {
            get { return _buttonLabel; }
            set
            {
                _buttonLabel = value;
                OnNotifyPropertyChanged("ButtonLabel");
            }
        }

        /// <summary>
        /// The Uri to navigate to when the user clicks the button at the bottom of the high score page.  If null then we will navigate back.
        /// </summary>
        public Uri ButtonUri { get; set; }

        /// <summary>
        /// Requests the view sends us to the next page.  What the next page is depends on the value of <see cref="ButtonUri"/>
        /// </summary>
        /// <param name="obj"></param>
        private void HandleClick(object obj)
        {
            SendViewMessage(null == ButtonUri
                                ? new NavigationMessageArgs {NavigationMode = NavigationMode.Back}
                                : new NavigationMessageArgs(ButtonUri));
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


        /// <summary>
        /// Wipe the <see cref="HighScores"/> view model collection and reload it from the <see cref="Model"/> and sort by high score
        /// </summary>
        private void ResyncViewModelFromModel()
        {
            HighScores.Clear();
            HighScoreItemViewModel mostRecent = null;
            foreach (HighScore score in _model.Scores)
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