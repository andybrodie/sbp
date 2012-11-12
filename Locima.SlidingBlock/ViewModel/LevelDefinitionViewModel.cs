using System;
using System.Windows.Media.Imaging;
using Locima.SlidingBlock.GameTemplates;
using Locima.SlidingBlock.IO;
using Locima.SlidingBlock.Scrambles;

namespace Locima.SlidingBlock.ViewModel
{
    public class LevelDefinitionViewModel : ViewModelBase
    {
        private LevelDefinition _level;

        public LevelDefinitionViewModel(LevelDefinition level)
        {
            _level = level;
            CreateThumbnail();
        }

        private void CreateThumbnail()
        {
            Thumbnail = _level.CreateThumbnail(64, 64);
        }

        public LevelDefinition Level
        {
            get { return _level; }
        }

        public WriteableBitmap Thumbnail { get; private set; }

        public string ImageTitle
        {
            get { return _level.ImageTitle; }
            set
            {
                _level.ImageTitle = value;
                OnNotifyPropertyChanged("ImageTitle");
            }
        }

        public string ImageText
        {
            get { return _level.ImageText; }
            set
            {
                _level.ImageText = value;
                OnNotifyPropertyChanged("ImageText");
            }
        }

        public Scrambler.ScrambleType ScrambleType
        {
            get { return _level.ScrambleType; }
            set
            {
                _level.ScrambleType = value;
                OnNotifyPropertyChanged("ScrambleType");
            }
        }

        public string LicenseTitle
        {
            get { return _level.License.Title; }
            set
            {
                _level.License.Title = value;
                OnNotifyPropertyChanged("LicenseTitle");
            }
        }

        public Uri LicenceLink
        {
            get { return _level.License.Link; }
            set
            {
                _level.License.Link = value;
                OnNotifyPropertyChanged("LicenseLink");
            }
        }
    }
}