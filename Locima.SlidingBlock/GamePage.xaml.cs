using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using Locima.SlidingBlock.Common;
using Locima.SlidingBlock.Controls;
using Locima.SlidingBlock.IO;
using Locima.SlidingBlock.Persistence;
using Locima.SlidingBlock.ViewModel;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using NLog;
using Locima.SlidingBlock.Messaging;

namespace Locima.SlidingBlock
{
    /// <summary>
    /// The main game page that uses the <see cref="Puzzle"/> custom control.
    /// </summary>
    public partial class GamePage : PhoneApplicationPage
    {

        private const string SaveGameQueryParameterName = "SaveGame";
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Convenient getter for the view model that is defined in XAML
        /// </summary>
        private PuzzleViewModel ViewModel
        {
            get { return ((PuzzleViewModel) Resources["ViewModel"]); }
        }


        /// <summary>
        /// Default page constructor that invoked <see cref="InitializeComponent"/>
        /// </summary>
        public GamePage()
        {
            InitializeComponent();
        }


        /// <summary>
        /// <para>Performs the majority of the initialisation of the page</para>
        /// </summary>
        /// <remarks>
        /// <list type="number">
        /// <item><description>Builds the application bar</description></item>
        /// <item><description>The <see cref="ViewModel"/> is set up in the XAML, so we only need to call <see cref="PuzzleViewModel.Initialise()"/></description></item>
        /// <item><description>Loads the configured game using <see cref="LoadSaveGame"/></description></item>
        /// <item><description>If the user has managed to load a completed game (<see cref="SaveGame.IsCompletedGame"/> (this can happen if Back is used), then
        /// bounce the user immediately to the <see cref="GameEnd"/> page</description></item>
        /// <item><description>Finally, <see cref="Locima.SlidingBlock.Controls.Puzzle.Initialise"/> is called </description></item>
        /// </list>
        /// </remarks>
        /// <param name="e"></param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            ViewModel.RegisterMessageHandler<GameStateChangeMessageArgs>(HandleGameStateChanges);
            ViewModel.Initialise();
            this.RegisterDefaultMessageHandlers(ViewModel);
            SaveGame gameState = LoadSaveGame();
            if (gameState.IsCompletedGame)
            {
                Logger.Info("Completed game loaded, bouncing user directly to the GameEnd screen");
                NavigationService.Navigate(GameEnd.CreateNavigationUri(gameState.Id));
            }
            else
            {
                Puzzle.Game = gameState;
                Puzzle.Initialise();
            }
        }


        /// <summary>
        /// When the game changes state this handler is invoked to allow us to reconfigure the Application Bar using <see cref="ConfigureApplicationBar"/>
        /// </summary>
        /// <param name="sender"><see cref="PuzzleViewModel"/></param>
        /// <param name="eventargs">The new <see cref="GameStates"/></param>
        private void HandleGameStateChanges(object sender, GameStateChangeMessageArgs eventargs)
        {
            ConfigureApplicationBar(eventargs.GameState);
        }


        /// <summary>
        /// Loads the <see cref="SaveGame"/> specified by the <see cref="SaveGameQueryParameterName"/> query parameter and returns it
        /// </summary>
        /// <remarks>
        /// If anything goes wrong here, we throw an exception which will kill the game entirely, however the game should never let us get in to such a state</remarks>
        /// <exception cref="InvalidStateException">If no game was specified</exception>
        private SaveGame LoadSaveGame()
        {
            string saveGameFilename;
            SaveGame game;
            if (NavigationContext.QueryString.TryGetValue(SaveGameQueryParameterName, out saveGameFilename))
            {
                Logger.Info("Loading SaveGame from {0} as passed in the {1} query parameter", saveGameFilename,
                            SaveGameQueryParameterName);
                game = SaveGameStorageManager.Instance.Load(saveGameFilename);
            }
            else
            {
                throw new InvalidStateException("GamePage has been invoked with no {0} parameter", SaveGameQueryParameterName);
            }
            return game;
        }


        /// <summary>
        /// When navigating away from this page this method invokes <see cref="PuzzleViewModel.OnNavigatingFrom"/> to ensure that the game state is preserved
        /// </summary>
        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            base.OnNavigatingFrom(e);

            ViewModel.Thumbnail = CreateThumbnail();
            ViewModel.OnNavigatingFrom(e);
        }


        /// <summary>
        /// Creates a screenshot of the level in its current state
        /// </summary>
        /// <returns></returns>
        private WriteableBitmap CreateThumbnail()
        {
            Logger.Info("Creating thumbnail image for current level state");
            WriteableBitmap thumbnail = new WriteableBitmap(Puzzle, null);
            thumbnail.Resize(LevelState.ThumbnailSize, LevelState.ThumbnailSize,
                             WriteableBitmapExtensions.Interpolation.Bilinear);
            return thumbnail;
        }


        /// <summary>
        /// This is the event handler for the <see cref="FrameworkElement.SizeChanged"/> event.
        /// </summary>
        /// <remarks>
        /// When the size of the available space within the grid cell for the puzzle changes, it's important that the puzzle is resized take advantage of the full amount of screen space avaialble.
        /// In the case of our control hierarchy, the puzzle is contained within a <see cref="PuzzleBorder"/> control, so we'll set the <see cref="FrameworkElement.Width"/> and <see cref="FrameworkElement.Height"/>
        /// to the largest square within that space.</remarks>
        /// <param name="unused"></param>
        /// <param name="unused2"></param>
        private void LayoutRootSizeChanged(object unused, SizeChangedEventArgs unused2)
        {
            // The Puzzle Border, which contains the puzzle, needs to be set to the biggest size it can

            // Find the grid row and column index that the PuzzleBorder is contained within
            int puzzleControlGridCellRowIndex = Grid.GetRow(PuzzleBorder);
            int puzzleControlGridCellColumnIndex = Grid.GetColumn(PuzzleBorder);

            // Now the biggest square that will fit within the ActualWidth and ActualHeight of that grid cell
            double largestSquareWithinParent = Math.Min(LayoutRoot.ColumnDefinitions[puzzleControlGridCellColumnIndex].ActualWidth,
                                                        LayoutRoot.RowDefinitions[puzzleControlGridCellRowIndex].ActualHeight);

            if (Math.Abs(PuzzleBorder.Width - PuzzleBorder.Height) < 1 && Math.Abs(PuzzleBorder.Height - largestSquareWithinParent) < 1)
            {
                Logger.Debug("Ignoring grid size change as values haven't changed: PuzzleBorder.Width({0}) and PuzzleBorder.Height({1}) unchanged at {2}",
                    PuzzleBorder.ActualWidth, PuzzleBorder.ActualHeight, largestSquareWithinParent);
            }
            else
            {
                Logger.Debug(
                    "LayoutRoot grid size has changed, updating PuzzleBorder.Width({0}) and PuzzleBorder.Height({1}) to {2}",
                    PuzzleBorder.ActualWidth, PuzzleBorder.ActualHeight, largestSquareWithinParent);
                PuzzleBorder.Width = largestSquareWithinParent;
                PuzzleBorder.Height = largestSquareWithinParent;
            }
        }


        /// <summary>
        ///   Pauses the game using the <see cref="PuzzleViewModel.PauseGameCommand" /> which toggles the pause state, then updates the <see
        ///    cref="ApplicationBar" /> so that a pause or resume button is displayed appropriately
        /// </summary>
        /// <remarks>
        /// We have to do this via a <see cref="Button.OnClick"/> handler because <see cref="IApplicationBarIconButton"/>s don't support <see cref="ICommand"/>s</remarks>
        /// <param name="sender"> The pause <see cref="IApplicationBarIconButton" /> </param>
        /// <param name="unused">unused</param>
        private void PauseButtonOnClick(object sender, EventArgs unused)
        {
            if (ViewModel.PauseGameCommand.CanExecute(null))
            {
                ViewModel.PauseGameCommand.Execute(null);
            }
        }

        /// <summary>
        ///   Pauses the game using the <see cref="PuzzleViewModel.PauseGameCommand" /> which toggles the pause state, then updates the <see
        ///    cref="ApplicationBar" /> so that a pause or resume button is displayed appropriately
        /// </summary>
        /// <remarks>
        /// We have to do this via a <see cref="Button.OnClick"/> handler because <see cref="IApplicationBarIconButton"/>s don't support <see cref="ICommand"/>s</remarks>
        /// <param name="sender"> The pause <see cref="IApplicationBarIconButton" /> </param>
        /// <param name="unused">unused</param>
        private void ResumeButtonOnClick(object sender, EventArgs unused)
        {
            if (ViewModel.ResumeGameCommand.CanExecute(null))
            {
                ViewModel.ResumeGameCommand.Execute(null);
            }
        }


        private void ConfigureApplicationBar(GameStates gameState)
        {
            Logger.Info("Configuring application bar for game state {0}", gameState);
            switch (gameState)
            {
                case GameStates.Running:
                    ApplicationBar = new ApplicationBar();
                    ApplicationBarHelper.AddButton(ApplicationBar, ApplicationBarHelper.ButtonIcons["Pause"],
                                                   LocalizationHelper.GetString("Pause")).Click += PauseButtonOnClick;
                    break;
                case GameStates.Paused:
                    ApplicationBar = new ApplicationBar();
                    ApplicationBarHelper.AddButton(ApplicationBar, ApplicationBarHelper.ButtonIcons["Resume"],
                                                   LocalizationHelper.GetString("Resume")).Click += ResumeButtonOnClick;
                    break;
                case GameStates.NotStarted:
                    ApplicationBar = new ApplicationBar();
                    break;
                case GameStates.Completed:
                    ApplicationBar = new ApplicationBar();
                    break;
                default:
                    throw new InvalidStateException("Unhandled GameState passed (I've added an enum value and forgotten to handle it in ConfigurationApplicationBar");
            }
        }

        #region Uri Generator Methods

        /// <summary>
        ///   Creates a new <see cref="Uri" /> object that points to this page for either starting new games or continuing existing games.
        /// </summary>
        /// <remarks>
        ///   This method is designed to be used by other classes that which to navigate to this page
        /// </remarks>
        /// <param name="puzzleMetadataFilename"> The file name for the <see cref="SaveGame" /> that contains the configuration for this game </param>
        /// <param name="suppressBack">If true, then the loaded game page will remove the previous page from the backstack.
        /// This is useful when progressing another level</param>
        /// <returns> A <see cref="Uri" /> that will launch the game using the parameters provided </returns>
        public static Uri CreateNavigationUri(string puzzleMetadataFilename, int suppressBack)
        {
            return
                new Uri(
                    string.Format("/GamePage.xaml?{0}={1}&{2}={3}", SaveGameQueryParameterName,
                                  HttpUtility.UrlEncode(puzzleMetadataFilename), App.SuppressBackQueryParameterName,
                                  suppressBack), UriKind.Relative);
        }

        #endregion
    }


    internal class GameStateChangeMessageArgs : MessageArgs
    {
        public GameStates GameState;
    }


}