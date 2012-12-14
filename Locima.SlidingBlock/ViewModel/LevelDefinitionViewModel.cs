using System;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using Locima.SlidingBlock.GameTemplates;

namespace Locima.SlidingBlock.ViewModel
{
    /// <summary>
    /// The MVVM child view model of <see cref="GameEditorViewModel"/> for the view of a single view in the list of all levels within
    /// a <see cref="GameTemplate"/>, used in <see cref="GameEditor"/> and model <see cref="LevelDefinition"/>
    /// </summary>
    public class LevelDefinitionViewModel : ViewModelBase
    {

        /// <summary>
        /// Backing field for <see cref="Level"/>
        /// </summary>
        private readonly LevelDefinition _level;

        /// <summary>
        /// The parent view model that this level is being shown in
        /// </summary>
        private readonly GameEditorViewModel _parent;

        /// <summary>
        /// Backing field for <see cref="Thumbnail"/>
        /// </summary>
        private WriteableBitmap _thumbnail;

        /// <summary>
        /// Initialise commands and internal variables
        /// </summary>
        /// <param name="gameEditorViewModel">The parent view model of this one</param>
        /// <param name="level">The model for this view model object</param>
        public LevelDefinitionViewModel(GameEditorViewModel gameEditorViewModel, LevelDefinition level)
        {
            _parent = gameEditorViewModel;
            _level = level;
            AddLevelBeforeCommand = new DelegateCommand(parameter => _parent.AddLevelBefore(_level));
            AddLevelAfterCommand = new DelegateCommand(parameter => _parent.AddLevelAfter(_level));
            MoveLeveCommand = new DelegateCommand(parameter => _parent.MoveLevel(_level, Int32.Parse((string) parameter)));
            DeleteLevelCommand = new DelegateCommand(parameter => _parent.DeleteLevel(_level));
            CreateThumbnail();
        }


        /// <summary>
        /// The model for thie MVVM view model
        /// </summary>
        public LevelDefinition Level
        {
            get { return _level; }
        }


        /// <summary>
        /// The thumbnail shown next to each level entry
        /// </summary>
        public WriteableBitmap Thumbnail
        {
            get { return _thumbnail; }
            private set
            {
                _thumbnail = value;
                OnNotifyPropertyChanged("Thumbnail");
            }
        }

        /// <summary>
        /// The title of the level, mapped from <see cref="LevelDefinition.ImageTitle"/>
        /// </summary>
        public string ImageTitle
        {
            get { return _level.ImageTitle; }
            set
            {
                _level.ImageTitle = value;
                OnNotifyPropertyChanged("ImageTitle");
            }
        }

        /// <summary>
        /// A text description of the level, mapped from <see cref="LevelDefinition.ImageText"/>
        /// </summary>
        public string ImageText
        {
            get { return _level.ImageText; }
            set
            {
                _level.ImageText = value;
                OnNotifyPropertyChanged("ImageText");
            }
        }


        /// <summary>
        /// Invoked from the level context menu to add a new level before this one
        /// </summary>
        /// <remarks>The action is provided by <see cref="GameEditorViewModel.AddLevelBefore"/></remarks>
        public ICommand AddLevelBeforeCommand { get; private set; }

        /// <summary>
        /// Invoked from the level context menu to add a new level after this one
        /// </summary>
        /// <remarks>The action is provided by <see cref="GameEditorViewModel.AddLevelAfter"/></remarks>
        public ICommand AddLevelAfterCommand { get; private set; }

        /// <summary>
        /// Invoked from the context menu to delete this level
        /// </summary>
        public ICommand DeleteLevelCommand { get; private set; }

        /// <summary>
        /// Invoked from the context menu to move this level up or down
        /// </summary>
        /// <remarks>The action is provided by <see cref="GameEditorViewModel.MoveLevel"/></remarks>
        public ICommand MoveLeveCommand { get; private set; }

        /// <summary>
        /// Creates a 64 x 64 pixel thumbnail to display next to the level details for this level
        /// </summary>
        private void CreateThumbnail()
        {
            Thumbnail = _level.CreateThumbnail(64, 64);
        }
    }
}