using System;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using Locima.SlidingBlock.Common;
using Locima.SlidingBlock.IO;
using Locima.SlidingBlock.Messaging;
using Locima.SlidingBlock.Persistence;
using NLog;

namespace Locima.SlidingBlock.ViewModel
{

    /// <summary>
    /// The MVVM view model object for a single <see cref="SaveGame"/> shown inside the <see cref="SaveGameSelectorViewModel"/>
    /// </summary>
    public class SaveGameMenuItem : ViewModelBase
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        ///   This lock object prevents parallel execution of deletion and launch of a game (the UI may send the event for delete then the user may click before the UI has reflected
        ///   the deletion)
        /// </summary>
        private readonly object _deleteLaunchLock = new object();

        /// <summary>
        /// The <see cref="SaveGame"/> that this item represents
        /// </summary>
        private SaveGame _game;

        /// <summary>
        /// The <see cref="GamePage"/> that will be navigated to if this item is selected
        /// </summary>
        /// <see cref="GamePage.CreateNavigationUri"/>
        /// <see cref="LaunchGameAction"/>
        private Uri _launchGameUri;

        /// <summary>
        /// The parent MVVM view model
        /// </summary>
        private SaveGameSelectorViewModel _saveGameSelectorViewModel;

        /// <summary>
        ///   Used to flag a game that has been deleted in <see cref="DeleteGameAction" /> to ensure that any calls to <see
        ///    cref="LaunchGameAction" /> are ignored if this is set.
        /// </summary>
        /// <remarks>
        ///   Used in conjunction with <see cref="_deleteLaunchLock" />
        /// </remarks>
        private bool _isDeleted;

        /// <summary>
        /// Configures this item with the <paramref name="game"/> provided
        /// </summary>
        /// <param name="saveGameSelectorViewModel">The parent MVVM view model</param>
        /// <param name="game">The game represented by this item</param>
        public void Initialise(SaveGameSelectorViewModel saveGameSelectorViewModel, SaveGame game)
        {
            _saveGameSelectorViewModel = saveGameSelectorViewModel;
            _game = game;
            SaveGameTitle = LocalizationHelper.GetString("SaveGameLevelTitle", game.CurrentLevelIndex + 1);
            SaveGameLastUpdate = game.LastUpdate.DateTime;
            Thumbnail = game.CurrentLevel.Thumbnail;
            _launchGameUri = GamePage.CreateNavigationUri(game.Id, 0);

            DeleteGameCommand = new DelegateCommand(DeleteGameAction);
            LaunchGameCommand = new DelegateCommand(LaunchGameAction);
            ShareMessageHandlers(saveGameSelectorViewModel);
        }

        /// <summary>
        /// A main title for the game, set in <see cref="Initialise"/>.
        /// </summary>
        public string SaveGameTitle { get; private set; }

        /// <summary>
        /// A description of the game, set in <see cref="Initialise"/>.
        /// </summary>
        public DateTime SaveGameLastUpdate { get; private set; }

        /// <summary>
        /// A thumbnail to show representing the current state of the game, set in <see cref="Initialise"/>.
        /// </summary>
        public WriteableBitmap Thumbnail { get; private set; }

        /// <summary>
        /// A command the View can execute to delete this game
        /// </summary>
        public ICommand DeleteGameCommand { get; private set; }

        /// <summary>
        /// A command the View can execute to launch this game
        /// </summary>
        public ICommand LaunchGameCommand { get; private set; }

        /// <summary>
        /// If <see cref="LaunchGameCommand"/> is executed, this uses <see cref="ViewModelBase.SendViewMessage"/> navigate to the <see cref="GamePage"/> page to start this game.
        /// </summary>
        /// <param name="obj"></param>
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
        

        /// <summary>
        /// If <see cref="DelegateCommand"/> is executed, this method deletes this item and removes this from the <see cref="_saveGameSelectorViewModel"/> parent view model collection.
        /// </summary>
        /// <param name="obj"></param>
        private void DeleteGameAction(object obj)
        {
            lock (_deleteLaunchLock)
            {
                SaveGameStorageManager.Instance.DeleteGame(_game);
                _saveGameSelectorViewModel.SavedGames.Remove(this);
                _isDeleted = true;
            }
        }

        /// <summary>
        /// Overrides the default implementation to add the <see cref="SaveGameTitle"/> for ease of debugging
        /// </summary>
        public override string ToString()
        {
            return string.Format("{0}({1})", base.ToString(), SaveGameTitle);
        }
    }
}