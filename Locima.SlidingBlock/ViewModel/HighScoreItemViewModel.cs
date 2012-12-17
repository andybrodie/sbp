using System;
using System.Windows;
using System.Windows.Media;
using Locima.SlidingBlock.Common;
using Locima.SlidingBlock.Persistence;

namespace Locima.SlidingBlock.ViewModel
{
    /// <summary>
    /// The view model for a single entry within the high score table using a model of <see cref="HighScore"/>
    /// </summary>
    /// <remarks>
    /// This is used by <see cref="HighScoresViewModel"/></remarks>
    public class HighScoreItemViewModel : ViewModelBase
    {
        private readonly HighScore _score;
        private DateTime _dateEntered;
        private string _difficulty;
        private bool _isHighlighted;
        private string _playerId;
        private string _playerName;
        private string _templateName;
        private TimeSpan _time;
        private int _totalMoves;


        /// <summary>
        /// Initialise this view model to present data for <paramref name="score"/>
        /// </summary>
        /// <param name="score">The model</param>
        public HighScoreItemViewModel(HighScore score)
        {
            _score = score;
            PlayerName = score.Name;
            Time = score.TotalTime;
            TotalMoves = score.TotalMoves;
            DateEntered = score.When;
            TemplateName = score.TemplateName;
            Difficulty = score.Difficulty;
        }


        /// <summary>
        /// The difficulty of the game
        /// </summary>
        protected string Difficulty
        {
            get { return _difficulty; }
            set
            {
                _difficulty = value;
                OnNotifyPropertyChanged("Difficulty");
            }
        }


        /// <summary>
        /// The name of the game template that was used for this game
        /// </summary>
        /// <remarks>We use the name instead of the ID in case the template is deleted</remarks>
        protected string TemplateName
        {
            get { return _templateName; }
            set
            {
                _templateName = value;
                OnNotifyPropertyChanged("TemplateName");
            }
        }

        /// <summary>
        /// The amount of time it took the player to finish the game
        /// </summary>
        public TimeSpan Time
        {
            get { return _time; }
            set
            {
                _time = value;
                _score.TotalTime = _time;
                OnNotifyPropertyChanged("Time");
            }
        }

        /// <summary>
        /// The ID of the player that was responsible for this high score entry
        /// </summary>
        /// <remarks>
        /// This could be used in future to ensure that if the player changes their name, then the name in the high score table can also be automatically updated.
        /// </remarks>
        public string PlayerId
        {
            get { return _playerId; }
            set
            {
                _playerId = value;
                OnNotifyPropertyChanged("PlayerId");
            }
        }


        /// <summary>
        /// The number of moves (tile slides) it took to finish the game
        /// </summary>
        public int TotalMoves
        {
            get { return _totalMoves; }
            set
            {
                _totalMoves = value;
                OnNotifyPropertyChanged("TotalMoves");
            }
        }


        /// <summary>
        /// The name of the playewr who was responsible for the high score
        /// </summary>
        public string PlayerName
        {
            get { return _playerName; }
            set
            {
                _playerName = value;
                OnNotifyPropertyChanged("PlayerName");
            }
        }


        /// <summary>
        /// If true, then the high score entry will be highlighted on the page
        /// </summary>
        public bool IsHighlighted
        {
            get { return _isHighlighted; }
            set
            {
                _isHighlighted = value;
                OnNotifyPropertyChanged("DateEntered");
            }
        }


        /// <summary>
        /// When the high score was registered on the table
        /// </summary>
        public DateTime DateEntered
        {
            get { return _dateEntered; }
            set
            {
                _dateEntered = value;
                OnNotifyPropertyChanged("DateEntered");
            }
        }


        /// <summary>
        /// The brush to use to colour the high score entry.
        /// </summary>
        /// <remarks>Selection is made based on <see cref="IsHighlighted"/>.  If <c>true</c> then <c>PhoneAccentBrush</c> is used, otherwise <c>PhoneForegroundBrush</c></remarks>
        public Brush EntryStyle
        {
            get
            {
                return (Brush) (IsHighlighted
                                    ? Application.Current.Resources["PhoneAccentBrush"]
                                    : Application.Current.Resources["PhoneForegroundBrush"]);
            }
        }


        /// <summary>
        /// Formats the total moves and time taken as a localised string
        /// </summary>
        public string GameStats
        {
            get { return LocalizationHelper.GetString("HighScoreStats", TotalMoves, Time.ToString("c")); }
        }


        /// <summary>
        /// A description of the game that way played (template name and number of tiles)
        /// </summary>
        public string GameDescription
        {
            get { return LocalizationHelper.GetString("GameDescriptionHighScoreItem", Difficulty, TemplateName); }
        }
    }
}