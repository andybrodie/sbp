using System;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using Locima.SlidingBlock.IO;
using Locima.SlidingBlock.Messaging;
using Locima.SlidingBlock.Persistence;
using NLog;

namespace Locima.SlidingBlock.ViewModel
{
    public class SaveGameMenuItem : ViewModelBase
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        ///   This lock object prevents parallel execution of deletion and launch of a game (the UI may send the event for delete then the user may click before the UI has reflected
        ///   the deletion)
        /// </summary>
        private readonly object _deleteLaunchLock = new object();

        private SaveGame _game;
        private Uri _launchGameUri;
        private SaveGameSelectorViewModel _saveGameSelectorViewModel;

        /// <summary>
        ///   Used to flag a game that has been deleted in <see cref="DeleteGameAction" /> to ensure that any calls to <see
        ///    cref="LaunchGameAction" /> are ignored if this is set.
        /// </summary>
        /// <remarks>
        ///   Used in conjunction with <see cref="_deleteLaunchLock" />
        /// </remarks>
        private bool _isDeleted;

        public void Initialise(SaveGameSelectorViewModel saveGameSelectorViewModel, SaveGame game)
        {
            _saveGameSelectorViewModel = saveGameSelectorViewModel;
            _game = game;
            SaveGameTitle = game.Name;
            SaveGameDescription = game.LastUpdate.ToString();
            Thumbnail = game.CurrentLevel.Thumbnail;
            _launchGameUri = GamePage.CreateNavigationUri(game.Id, false);

            DeleteGameCommand = new DelegateCommand(DeleteGameAction);
            LaunchGameCommand = new DelegateCommand(LaunchGameAction);
            ShareMessageHandlers(saveGameSelectorViewModel);
        }

        public string SaveGameTitle { get; private set; }

        public string SaveGameDescription { get; private set; }

        public WriteableBitmap Thumbnail { get; private set; }

        public ICommand DeleteGameCommand { get; private set; }

        public ICommand LaunchGameCommand { get; private set; }

        private void LaunchGameAction(object obj)
        {
            lock (_deleteLaunchLock)
            {
                if (!_isDeleted)
                {
                    SaveGameMenuItem item = (SaveGameMenuItem) obj;
                    Logger.Info("Launching game {0}", item._launchGameUri);
                    SendViewMessage(new NavigationMessageArgs(item._launchGameUri));
                }
                else
                {
                    Logger.Debug("Ignoring call to launch deleted game");
                }
            }
        }
        

        private void DeleteGameAction(object obj)
        {
            lock (_deleteLaunchLock)
            {
                SaveGameStorageManager.Instance.DeleteGame(_game);
                _saveGameSelectorViewModel.SavedGames.Remove(this);
                _isDeleted = true;
            }
        }

        public override string ToString()
        {
            return string.Format("{0}({1})", base.ToString(), SaveGameTitle);
        }
    }
}