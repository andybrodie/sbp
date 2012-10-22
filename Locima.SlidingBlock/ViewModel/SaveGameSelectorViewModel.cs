using System.Collections.ObjectModel;
using System.Collections.Specialized;
using Locima.SlidingBlock.IO;
using Locima.SlidingBlock.Persistence;
using NLog;

namespace Locima.SlidingBlock.ViewModel
{
    public class SaveGameSelectorViewModel : ViewModelBase
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public ObservableCollection<SaveGameMenuItem> SavedGames { get; set; }

        public void Initialise()
        {
            Refresh();
        }

        private void Refresh()
        {
            if (IsDesignTime) return;

            Logger.Info("Refreshing save game list for player {0}", PlayerStorageManager.Instance.CurrentPlayer);
            SavedGames = new ObservableCollection<SaveGameMenuItem>();
            SavedGames.CollectionChanged += SavedGamesCollectionChanged;
            foreach (SaveGame puzzle in SaveGameStorageManager.Instance.LoadGames(PlayerStorageManager.Instance.CurrentPlayer.Id))
            {
                SaveGameMenuItem sgmi = new SaveGameMenuItem();
                sgmi.Initialise(this, puzzle);
                SavedGames.Add(sgmi);
            }
        }


        public void SavedGamesCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            OnNotifyPropertyChanged("SavedGames");
        }
    }
}