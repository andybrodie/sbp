using System.Collections.ObjectModel;
using System.Collections.Specialized;
using Locima.SlidingBlock.IO;
using Locima.SlidingBlock.Persistence;
using NLog;

namespace Locima.SlidingBlock.ViewModel
{

    /// <summary>
    /// The MVVM view model for the <see cref="SavedGameSelector"/>
    /// </summary>
    public class SaveGameSelectorViewModel : ViewModelBase
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// The list of save games to select from
        /// </summary>
        public ObservableCollection<SaveGameMenuItem> SavedGames { get; set; }

        /// <summary>
        /// Initialises <see cref="SavedGames"/>
        /// </summary>
        public SaveGameSelectorViewModel()
        {
            SavedGames = new ObservableCollection<SaveGameMenuItem>();
            SavedGames.CollectionChanged += SavedGamesCollectionChanged;
        }

        /// <summary>
        /// Loads the relevant save games using <see cref="Refresh"/>
        /// </summary>
        public void Initialise()
        {
            Refresh();
        }


        /// <summary>
        /// Loads all the <see cref="SaveGame"/>s for the <see cref="IPlayerStorageManager.CurrentPlayer"/>
        /// </summary>
        private void Refresh()
        {
            if (IsDesignTime) return;

            Logger.Info("Refreshing save game list for player {0}", PlayerStorageManager.Instance.CurrentPlayer);
            SavedGames.Clear();
            foreach (SaveGame puzzle in SaveGameStorageManager.Instance.LoadGames(PlayerStorageManager.Instance.CurrentPlayer.Id))
            {
                SaveGameMenuItem sgmi = new SaveGameMenuItem();
                sgmi.Initialise(this, puzzle);
                SavedGames.Add(sgmi);
            }
        }


        /// <summary>
        /// Pass up changes to the <see cref="SavedGames"/> up to the view.
        /// </summary>
        public void SavedGamesCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            OnNotifyPropertyChanged("SavedGames");
        }
    }
}