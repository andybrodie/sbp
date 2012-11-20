using System;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using Locima.SlidingBlock.GameTemplates;
using Locima.SlidingBlock.Scrambles;
using NLog;

namespace Locima.SlidingBlock.ViewModel
{
    public class LevelDefinitionViewModel : ViewModelBase
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private LevelDefinition _level;
        private GameTemplateEditorViewModel _parent;

        public LevelDefinitionViewModel(GameTemplateEditorViewModel gameTemplateEditorViewModel, LevelDefinition level)
        {
            _parent = gameTemplateEditorViewModel;
            _level = level;
            AddLevelBeforeCommand = new DelegateCommand(parameter => _parent.AddLevelBefore(_level));
            AddLevelAfterCommand = new DelegateCommand(parameter => _parent.AddLevelAfter(_level));
            MoveLeveCommand = new DelegateCommand(parameter => _parent.MoveLevel(_level, "Up".Equals(parameter)));

            DeleteLevelCommand = new DelegateCommand(DeleteLevelAction);
            CreateThumbnail();
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

        public ICommand AddLevelBeforeCommand { get; private set; }

        public ICommand AddLevelAfterCommand { get; private set; }

        public ICommand DeleteLevelCommand { get; private set; }

        public ICommand MoveLeveCommand { get; private set; }
    
        private void DeleteLevelAction(object parameter)
        {
            Logger.Info("Deleting level {0}", this);
            // TODO The image is orphaned, need to fix that!
            _parent.LevelList.Remove(this);
        }


        private void CreateThumbnail()
        {
            Thumbnail = _level.CreateThumbnail(64, 64);
        }
    }
}