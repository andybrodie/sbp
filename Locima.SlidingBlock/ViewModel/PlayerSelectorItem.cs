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
    public class PlayerSelectorItem : ViewModelBase
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private readonly PlayerDetails _playerDetails;
        private readonly PlayerSelectorViewModel _playerSelectorViewModel;

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

        public ICommand EditPlayerCommand { get; private set; }
        public ICommand DeletePlayerCommand { get; private set; }
        public ICommand ForceDeletePlayerCommand { get; private set; }
        public ICommand SelectPlayerCommand { get; private set; }

        public string Name
        {
            get { return _playerDetails.Name; }
        }


        public Brush PlayerBrush
        {
            get { return new SolidColorBrush(_playerDetails.PreferredColor); }
        }

        private void SelectPlayerAction(object obj)
        {
            Logger.Info("User has selected player {0}", Name);
            PlayerStorageManager.Instance.CurrentPlayer = _playerDetails;
            SendViewMessage(NavigationMessageArgs.Back);
        }


        private void EditPlayerAction(object obj)
        {
            Logger.Info("Editing player : {0}", Name);
            SendViewMessage(new NavigationMessageArgs
                {Uri = AddPlayer.CreateNavigationUri(_playerDetails.Id)});
        }


        private void DeletePlayerAction(object obj)
        {
            {
                IEnumerable<SaveGame> saveGamesForPlayer = SaveGameStorageManager.Instance.LoadGames();
                if (saveGamesForPlayer.Any())
                {
                    // If there are any save games associated with the player then notify the view that we need confirmation and on OK, call ForceDeletePlayerCommand
                    SendViewMessage(new ConfirmationMessageArgs
                        {
                            Title = LocalizationHelper.GetString("DeleteConfirmationTitle"),
                            Message = LocalizationHelper.GetString("DeleteConfirmationMessage", Name, saveGamesForPlayer.Count()),
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
        }


        private void ForceDeletePlayerAction(object obj)
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