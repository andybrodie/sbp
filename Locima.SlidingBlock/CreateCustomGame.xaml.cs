using System;
using System.Collections.Generic;
using System.Windows.Input;
using System.Windows.Navigation;
using Locima.SlidingBlock.Common;
using Locima.SlidingBlock.IO;
using Locima.SlidingBlock.Model;
using Locima.SlidingBlock.Persistence;
using Locima.SlidingBlock.ViewModel;
using Microsoft.Phone.Controls;

namespace Locima.SlidingBlock
{

    /// <summary>
    /// Creates a new custom game, so users can make up their own levels
    /// </summary>
    /// <remarks>
    /// Requires lots of rework currently</remarks>
    public partial class CreateCustomGame : PhoneApplicationPage
    {

        /// <summary>
        /// The available tile sizes
        /// </summary>
        public List<int> TileSizes = new List<int>(new[] {3, 4, 5, 6, 7, 8, 9, 10});
        private SaveGame _puzzle;
        private const string SaveGameQueryParameterName = "SaveGame";

        /// <summary>
        /// Initialise commands
        /// </summary>
        public CreateCustomGame()
        {
            DataContext = this;
            InitializeComponent();
            SelectImageCommand = new DelegateCommand(SelectImageAction);
            StartGameCommand = new DelegateCommand(StartGameAction);
        }

        /// <summary>
        /// Creates a single level <see cref="SaveGame"/>, this needs to be fixed
        /// </summary>
        /// <param name="e"></param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
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

        /// <summary>
        /// Number of tiles across in the custom level
        /// </summary>
        public int TilesAcross { get; set; }

        /// <summary>
        /// Number of tiles high in the custom level
        /// </summary>
        public int TilesHigh { get; set; }


        /// <summary>
        /// Command to start the game
        /// </summary>
        public DelegateCommand StartGameCommand { get; private set; }

        /// <summary>
        /// Command to select an image for a level
        /// </summary>
        public ICommand SelectImageCommand { get; private set; }

        private void StartGameAction(object obj)
        {
            throw new NotImplementedException();
        }

        private void SelectImageAction(object obj)
        {
            NavigationService.Navigate(ImageChooser.CreateNavigationUri());
        }

        /// <summary>
        /// Creates a Uri to link to this page
        /// </summary>
        /// <param name="saveGameId">The ID of the <see cref="SaveGame"/> to create the custom game within</param>
        /// <returns>A Uri to use with <see cref="NavigationService.Navigate"/></returns>
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