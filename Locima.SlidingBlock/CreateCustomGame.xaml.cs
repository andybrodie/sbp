using System;
using System.Collections.Generic;
using System.Windows.Input;
using Locima.SlidingBlock.Common;
using Locima.SlidingBlock.IO;
using Locima.SlidingBlock.Model;
using Locima.SlidingBlock.Persistence;
using Locima.SlidingBlock.ViewModel;
using Microsoft.Phone.Controls;

namespace Locima.SlidingBlock
{
    public partial class CreateCustomGame : PhoneApplicationPage
    {
        private readonly ICommand _selectImageCommand;
        private readonly DelegateCommand _startGameCommand;
        public List<int> TileSizes = new List<int>(new[] {3, 4, 5, 6, 7, 8, 9, 10});
        private SaveGame _puzzle;
        private const string SaveGameQueryParameterName = "SaveGame";

        public CreateCustomGame()
        {
            DataContext = this;
            InitializeComponent();
            _selectImageCommand = new DelegateCommand(SelectImageAction);
            _startGameCommand = new DelegateCommand(StartGameAction);
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            _puzzle = SaveGameStorageManager.Instance.GetContinuableGame(PlayerStorageManager.Instance.CurrentPlayer.Id);
            _puzzle.EnsureLevels(1);
            LevelState level = _puzzle.Levels[0];
            TilesAcross = Math.Max(level.TilesAcross,3);
            TilesHigh = Math.Max(level.TilesHigh,3);
            PreviewImage.Source = level.Image;
            _puzzle.LocalPlayer = new PlayerLink { PlayerDetailsId = PlayerStorageManager.Instance.CurrentPlayer.Id, Position = new Position {X=0, Y=0} };
        }

        public int TilesAcross { get; set; }
        public int TilesHigh { get; set; }

        public DelegateCommand StartGameCommand
        {
            get { return _startGameCommand; }
        }

        public ICommand SelectImageCommand
        {
            get { return _selectImageCommand; }
        }

        private void StartGameAction(object obj)
        {
            throw new NotImplementedException();
        }

        private void SelectImageAction(object obj)
        {
            NavigationService.Navigate(ImageChooser.CreateNavigationUri());
        }

        public static Uri CreateNavigationUri(string saveGameId)
        {
            const string pagePath = "/CreateCustomGame.xaml";
            return string.IsNullOrEmpty(saveGameId)
                       ? new Uri(pagePath, UriKind.Relative)
                       : new Uri(
                             string.Format("{0}?{1}={2}", pagePath, SaveGameQueryParameterName, saveGameId),
                             UriKind.Relative);
        }
    }
}