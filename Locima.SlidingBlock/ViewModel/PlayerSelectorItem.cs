using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using System.Windows.Media;
using Locima.SlidingBlock.Common;
using Locima.SlidingBlock.IO;
using Locima.SlidingBlock.Messaging;
using Locima.SlidingBlock.Persistence;
using NLog;

namespace Locima.SlidingBlock.ViewModel
{
    /// <summary>
    /// The view model object for a player within a list.  <see cref="PlayerSelectorViewModel"/> is the parent view model and <see cref="PlayerDetails"/> provides the model
    /// </summary>
    public class PlayerSelectorItem : ViewModelBase
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private readonly PlayerDetails _playerDetails;
        private readonly PlayerSelectorViewModel _playerSelectorViewModel;

        /// <summary>
        /// Initialise this object with its parent view model and its model object
        /// </summary>
        /// <remarks>
        /// This class makes use of <see cref="IViewModelBase.ShareMessageHandlers"/> to </remarks>
        /// <param name="playerSelectorViewModel"></param>
        /// <param name="player"></param>
        public PlayerSelectorItem(PlayerSelectorViewModel playerSelectorViewModel, PlayerDetails player)
        {
            EditPlayerCommand = new DelegateCommand(EditPlayerAction);
            DeletePlayerCommand = new DelegateCommand(DeletePlayerAction);
            ForceDeletePlayerCommand = new DelegateCommand(ForceDeletePlayerAction);
            SelectPlayerCommand = new DelegateCommand(SelectPlayerAction);

            _playerDetails = player;
            _playerSelectorViewModel = playerSelectorViewModel;
            ShareMessageHandlers(playerSelectorViewModel);
        }

        /// <summary>
        /// Invoked to edit this player (navigates to the <see cref="PlayerEditor"/> page)
        /// </summary>
        /// <see cref="EditPlayerAction"/>
        public ICommand EditPlayerCommand { get; private set; }

        /// <summary>
        /// Invoked to request that this player is deleted.  If the player has any <see cref="SaveGame"/>s then we'll put up an "Are you sure?" message
        /// before going on to <see cref="ForceDeletePlayerCommand"/>
        /// </summary>
        /// <see cref="DeletePlayerAction"/>
        public ICommand DeletePlayerCommand { get; private set; }

        /// <summary>
        /// This proceeds with the deletion of a player, along with all their associated <see cref="SaveGame"/> instances.
        /// </summary>
        /// <remarks>
        /// I don't think this is named very well.  <see cref="DeletePlayerCommand"/> is for the initial user request to delete, <see cref="ForceDeletePlayerCommand"/> is for proceeding with the deletion.</remarks>
        /// <see cref="ForceDeletePlayerAction"/>
        public ICommand ForceDeletePlayerCommand { get; private set; }

        /// <summary>
        /// Invoked when the user selects this player to use
        /// </summary>
        /// <see cref="SelectPlayerAction"/>
        public ICommand SelectPlayerCommand { get; private set; }


        /// <summary>
        /// The name of this player
        /// </summary>
        public string Name
        {
            get { return _playerDetails.Name; }
        }


        /// <summary>
        /// Each player may have a preferred color, so paint their name in their color
        /// </summary>
        /// <remarks>
        /// TODO Work out what to do when the preferred color is the same as the background!
        /// </remarks>
        public Brush PlayerBrush
        {
            get { return new SolidColorBrush(_playerDetails.PreferredColor); }
        }

        /// <summary>
        /// Sets the current player in <see cref="IPlayerStorageManager.CurrentPlayer"/> and navigates back
        /// </summary>
        /// <see cref="SelectPlayerCommand"/>
        private void SelectPlayerAction(object unused)
        {
            Logger.Info("User has selected player {0}", Name);
            PlayerStorageManager.Instance.CurrentPlayer = _playerDetails;
            SendViewMessage(NavigationMessageArgs.Back);
        }


        /// <summary>
        /// Navigates on to the edit player screen for this player (<see cref="PlayerEditor"/>)
        /// </summary>
        /// <see cref="EditPlayerCommand"/>
        private void EditPlayerAction(object unused)
        {
            Logger.Info("Editing player : {0}", Name);
            SendViewMessage(new NavigationMessageArgs
                                {Uri = PlayerEditor.CreateNavigationUri(_playerDetails.Id)});
        }


        /// <summary>
        /// If the player has any save games, then pop up a delete confirmation message (using <see cref="IViewModelBase.SendViewMessage"/> with a <see cref="ConfirmationMessageArgs"/>)
        /// passing <see cref="ForceDeletePlayerCommand"/> as the action if the user confirms.
        /// </summary>
        /// <param name="obj">Passed to <see cref="ForceDeletePlayerAction"/> only</param>
        /// <see cref="DeletePlayerCommand"/>
        private void DeletePlayerAction(object obj)
        {
            IEnumerable<SaveGame> saveGamesForPlayer = SaveGameStorageManager.Instance.LoadGames();
            int saveGameCount = saveGamesForPlayer.Count();
            if (saveGameCount > 0)
            {
                // If there are any save games associated with the player then notify the view that we need confirmation and on OK, call ForceDeletePlayerCommand
                SendViewMessage(new ConfirmationMessageArgs
                                    {
                                        Title = LocalizationHelper.GetString("DeleteConfirmationTitle"),
                                        Message =
                                            LocalizationHelper.GetString("DeleteConfirmationMessage", Name,
                                                                         saveGameCount),
                                        OnOkCommand = ForceDeletePlayerCommand
                                    });
            }
            else
            {
                // No save games, so just delete the player already
                ForceDeletePlayerAction(obj);
                _playerSelectorViewModel.PlayerList.Remove(this);
            }
        }


        /// <summary>
        /// Deletes a player and all their save games then removes this object from the parent view model (<see cref="PlayerSelectorViewModel"/>) so the UI updates
        /// </summary>
        private void ForceDeletePlayerAction(object unused)
        {
            Logger.Info("Deleting player {0}({1}) and all their save games", _playerDetails.Name, _playerDetails.Id);

            // Delete all the save games associated with this player
            SaveGameStorageManager.Instance.DeleteGames(_playerDetails.Id);

            // Delete the player
            PlayerStorageManager.Instance.DeletePlayer(_playerDetails.Id);

            // Remove them from the View
            _playerSelectorViewModel.PlayerList.Remove(this);
        }
    }
}