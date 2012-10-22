using System;
using System.Windows;
using System.Windows.Media;
using Locima.SlidingBlock.Common;
using Locima.SlidingBlock.Persistence;

namespace Locima.SlidingBlock.ViewModel
{
    public class HighScoreItemViewModel : ViewModelBase
    {
        private DateTime _dateEntered;
        private bool _isHighlighted;
        private string _playerId;
        private string _playerName;
        private Highscore _score;
        private TimeSpan _time;
        private int _totalMoves;


        public HighScoreItemViewModel(Highscore score)
        {
            _score = score;
            PlayerName = score.Name;
            Time = score.TotalTime;
            DateEntered = score.When;
        }

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

        public string PlayerId
        {
            get { return _playerId; }
            set
            {
                _playerId = value;
                OnNotifyPropertyChanged("PlayerId");
            }
        }

        public int TotalMoves
        {
            get { return _totalMoves; }
            set { _totalMoves = value; 
            OnNotifyPropertyChanged("TotalMoves");}
        }

        public string PlayerName
        {
            get { return _playerName; }
            set
            {
                _playerName = value;
                OnNotifyPropertyChanged("PlayerName");
            }
        }


        public bool IsHighlighted
        {
            get { return _isHighlighted; }
            set
            {
                _isHighlighted = value;
                OnNotifyPropertyChanged("DateEntered");
            }
        }


        public DateTime DateEntered
        {
            get { return _dateEntered; }
            set
            {
                _dateEntered = value;
                OnNotifyPropertyChanged("DateEntered");
            }
        }


        public Brush EntryStyle
        {
            get
            {
                if (!IsDesignTime)
                {
                    return (Brush) (IsHighlighted
                                        ? Application.Current.Resources["PhoneAccentBrush"]
                                        : Application.Current.Resources["PhoneForegroundBrush"]);
                } else
                {
                    return IsHighlighted
                        ? new SolidColorBrush(Color.FromArgb(255,255,255,0))
                        : new SolidColorBrush(Color.FromArgb(255,255,255,255));
                }
            }
        }


        public string GameStats
        {
            get {
                return !IsDesignTime
                           ? LocalizationHelper.GetString("HighscoreStats", TotalMoves, Time.ToString("c"))
                           : TotalMoves + " moves in " + Time.ToString("c");
            }
        }
    }
}