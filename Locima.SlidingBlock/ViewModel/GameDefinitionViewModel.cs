using Locima.SlidingBlock.GameTemplates;

namespace Locima.SlidingBlock.ViewModel
{
    /// <summary>
    /// MVVM view model for a single item in the list of custom games (<see cref="GameDefinitionSelectorViewModel"/>)
    /// </summary>
    public class GameDefinitionViewModel : ViewModelBase
    {
        private string _title;

        /// <summary>
        /// Initialise this view model and all its fields using the <paramref name="game"/> passed
        /// </summary>
        /// <param name="parent">The view model that owns this specific custom game</param>
        /// <param name="game">The game definition that this item represents</param>
        public GameDefinitionViewModel(GameDefinitionSelectorViewModel parent, GameDefinition game)
        {
            ShareMessageHandlers(parent);
            Title = game.Title;
            Author = game.Author;
            LevelCount = game.Levels.Count;
            Id = game.Id;
        }

        public int LevelCount { get; private set; }

        public string Author { get; private set; }

        public string Title
        {
            get { return _title; }
            private set { _title = value; 
            OnNotifyPropertyChanged("Title");}
        }

        public string Id { get; private set; }
    }
}