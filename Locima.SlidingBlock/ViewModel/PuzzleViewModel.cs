using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using Locima.SlidingBlock.Common;
using Locima.SlidingBlock.Controls;
using Locima.SlidingBlock.GameTemplates;
using Locima.SlidingBlock.IO;
using Locima.SlidingBlock.Messaging;
using Locima.SlidingBlock.Model;
using Locima.SlidingBlock.Persistence;
using NLog;

namespace Locima.SlidingBlock.ViewModel
{
    /// <summary>
    ///   The view model for the <see cref="Puzzle" /> control, this brokers communication between the control and the <see
    ///    cref="PuzzleModel" /> model class.
    /// </summary>
    public class PuzzleViewModel : ViewModelBase
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        ///   A collection of all the objects that are currently running animations which must finish before the level can be completed
        /// </summary>
        /// <remarks>
        ///   Objects are added to this list in <see cref="LogAnimationStarted" /> and removed in <see cref="CheckPuzzleCompleted" />.  Only when this list is empty will
        ///   the UI inform the user that the level is complete.
        /// </remarks>
        private readonly ICollection<object> _animationsPending = new List<object>();

        private string _completedText;
        private string _completedTitle;

        /// <summary>
        ///   The current game, as saved by the player
        /// </summary>
        private SaveGame _currentGame;

        /// <summary>
        ///   The current level number, used to display the <see cref="PageTitle" /> and the level completion message
        /// </summary>
        private int _currentLevelNumber;

        /// <summary>
        /// Used to indicate whether the game should be saved during <see cref="OnNavigatingFrom"/>
        /// </summary>
        /// <remarks>
        /// Used in <see cref="ProceedToNextLevel"/> to stop <see cref="OnNavigatingFrom"/> from updating
        /// and saving the game when the user navigates away, because when proceeding to the next level
        /// we don't want <see cref="OnNavigatingFrom"/> to update and save <see cref="_currentGame"/>.
        /// In all other cases of navigating away from the page, it's because the user has quit
        /// or an event is deactivating the application, so we want the game to be saved.
        /// </remarks>
        private bool _dontSaveGameOnNavigatingFrom;

        /// <summary>
        /// Backing field for <see cref="ImageText"/>
        /// </summary>
        private string _imageText;

        /// <summary>
        /// Backing field for <see cref="ImageTitle"/>
        /// </summary>
        private string _imageTitle;

        /// <summary>
        /// Backing field for <see cref="LicenseLink"/>
        /// </summary>
        private Uri _licenseLink;

        /// <summary>
        /// Backing field for <see cref="LicenseTitle"/>
        /// </summary>
        private string _licenseTitle;

        /// <summary>
        ///   Backing field for <see cref="PageTitle" />
        /// </summary>
        private string _pageTitle;

        /// <summary>
        ///   Backing field for <see cref="PuzzleArea" />
        /// </summary>
        private Size _puzzleArea;

        /// <summary>
        ///   Set when the model reports that the puzzle has been completed
        /// </summary>
        /// <remarks>
        /// When this happens and all the running animations have finished <see cref="GameState"/> will move to <see cref="GameStates.Completed"/></remarks>
        private bool _puzzleCompleted;

        /// <summary>
        /// The underlying model for this puzzle control
        /// </summary>
        private PuzzleModel _puzzleModel;

        private string _imageOwner;
        private Uri _imageLink;

        /// <summary>
        /// Invoked by the view when the user wishes to pause the game
        /// </summary>
        public ICommand PauseGameCommand { get; private set; }

        /// <summary>
        /// Invoked by the view when the user wishes to resume a paused game
        /// </summary>
        public ICommand ResumeGameCommand { get; private set; }

        /// <summary>
        /// Invoked by the view when the user wishes to resume a paused game
        /// </summary>
        public ICommand StartGameCommand { get; private set; }

        /// <summary>
        ///   The amount of time the player has been playing this particular level.  This is bound to the UI.
        /// </summary>
        /// <remarks>
        ///   Checking for null because the puzzle model and therefore stopwatch might not be initalised yet
        /// </remarks>
        public TimeSpan ElapsedTime
        {
            get { return _puzzleModel.Stopwatch == null ? new TimeSpan(0) : _puzzleModel.Stopwatch.ElapsedTime; }
        }


        /// <summary>
        ///   A flat list of all the <see cref="TileViewModel" /> instances that this puzzle consists of.
        /// </summary>
        public List<TileViewModel> Tiles { get; private set; }

        /// <summary>
        ///   The size that each tile in the puzzle, measured in pixels
        /// </summary>
        public Size TileSize { get; set; }

        /// <summary>
        ///   The number of tiles across in the puzzle (the x-dimension of the puzzle)
        /// </summary>
        /// <remarks>
        ///   Delegates to <see cref="PuzzleModel.TilesAcross" />
        /// </remarks>
        public int TilesAcross
        {
            get { return _puzzleModel.TilesAcross; }
        }


        /// <summary>
        ///   The border around each tile. This provides the facility to create an overlaid border, either for aesthetics or to make the puzzle more difficult by increasing the border size.
        /// </summary>
        public int TileBorder { get; set; }

        /// <summary>
        ///   The area the puzzle takes up in total, measured in pixels
        /// </summary>
        /// <remarks>
        ///   The area of the puzzle is typically altered by the Grid layout being created (controls being laid out on initial load), or updated (new controls inserted). 
        ///   When this happens, all visuals of the puzzle must be recalculated: the size of each tile, the brush for each tile, etc. 
        ///   This is an expensive operation, so only if the puzzle area has actually changed will <see
        ///    cref="PuzzleModelPuzzleResized" /> method 
        ///   be called to perform the recalculation.
        /// </remarks>
        public Size PuzzleArea
        {
            get { return _puzzleArea; }
            set
            {
                if (_puzzleArea != value)
                {
                    Logger.Info("Updating Puzzle Area size from {0} to {1}", _puzzleArea, value);
                    _puzzleArea = value;
                    PuzzleModelPuzzleResized(false);
                }
                else
                {
                    Logger.Warn("Ignoring call to resize puzzle area to {0}, because that's what it is at the moment",
                                value);
                }
            }
        }

        /// <summary>
        ///   The number of moves that the player has made on this level
        /// </summary>
        /// <remarks>
        ///   Delegates to <see cref="PuzzleModel.MoveCount" />
        /// </remarks>
        public int MoveCount
        {
            get { return _puzzleModel.MoveCount; }
        }


        /// <summary>
        ///   The title of the page, which is the number of the level the player is on
        /// </summary>
        public string PageTitle
        {
            get { return _pageTitle; }
            private set
            {
                _pageTitle = value;
                OnNotifyPropertyChanged("PageTitle");
            }
        }


        /// <summary>
        /// The state of the game at any moment in time.  Changes in state may will cause the UI to update
        /// </summary>
        public GameStates GameState
        {
            get
            {
                GameStates output;
                return TryGetState("GameState", out output) ? output : GameStates.NotStarted;
            }
            set
            {
                SetState("GameState", value);
                OnNotifyPropertyChanged("GameState");
                SendViewMessage(new GameStateChangeMessageArgs {GameState = value});
            }
        }


        /// <summary>
        /// A thumbnail image for the puzzle, used to set <see cref="LevelState.Thumbnail"/>
        /// </summary>
        public WriteableBitmap Thumbnail { get; set; }

        /// <summary>
        /// The title of the current level, a short description of the image the user is trying to assemble
        /// </summary>
        /// <see cref="LevelDefinition.ImageTitle"/>
        public string ImageTitle
        {
            get { return _imageTitle; }
            set
            {
                _imageTitle = value;
                OnNotifyPropertyChanged("ImageTitle");
            }
        }

        /// <summary>
        /// The text of the current level, describing the image the user is trying to assemble
        /// </summary>
        /// <see cref="LevelDefinition.ImageText"/>
        public string ImageText
        {
            get { return _imageText; }
            set
            {
                _imageText = value;
                OnNotifyPropertyChanged("ImageText");
            }
        }

        /// <summary>
        /// The URL for the full text of the license attributed to the current picture of the current level
        /// </summary>
        /// <see cref="LicenseDefinition.Link"/>
        public Uri LicenseLink
        {
            get { return _licenseLink; }
            set
            {
                _licenseLink = value;
                OnNotifyPropertyChanged("LicenseLink");
            }
        }

        /// <summary>
        /// The name of the license attributed to the current picture of the current level
        /// </summary>
        /// <see cref="LicenseDefinition.Title"/>
        public string LicenseTitle
        {
            get { return _licenseTitle; }
            set
            {
                _licenseTitle = value;
                OnNotifyPropertyChanged("LicenseTitle");
            }
        }

        /// <summary>
        /// The title of the message to show when the user has completed a level
        /// </summary>
        public string CompletedTitle
        {
            get { return _completedTitle; }
            set
            {
                _completedTitle = value;
                OnNotifyPropertyChanged("CompletedTitle");
            }
        }


        /// <summary>
        /// A description of the stats accumulated when the user has completed a level
        /// </summary>
        public string CompletedText
        {
            get { return _completedText; }
            set
            {
                _completedText = value;
                OnNotifyPropertyChanged("CompletedText");
            }
        }

        /// <summary>
        /// The name of the level image author
        /// </summary>
        public string ImageOwner
        {
            get { return _imageOwner; }
            set
            {
                _imageOwner = value;
                OnNotifyPropertyChanged("ImageOwner");
            }
        }

        /// <summary>
        /// A link to an online resource connected to the image
        /// </summary>
        public Uri ImageLink
        {
            get { return _imageLink; }
            set
            {
                _imageLink = value;
                OnNotifyPropertyChanged("ImageLink");
            }
        }



        /// <summary>
        /// Initialises the view model, creating a new <see cref="PuzzleModel"/>.
        /// </summary>
        /// <remarks>
        /// This doens't do much until the <see cref="Initialise(Locima.SlidingBlock.Persistence.SaveGame)"/> is called.</remarks>
        public void Initialise()
        {
            Logger.Debug("PuzzleViewModel Initialise entry");
            StartGameCommand = new DelegateCommand(StartGameAction);
            PauseGameCommand = new DelegateCommand(PauseGameAction);
            ResumeGameCommand = new DelegateCommand(ResumeGameAction);

            // Create the puzzle model and hook up event handlers that we are interested in
            _puzzleModel = new PuzzleModel();

            // When a player moves, the PuzzleModel.MoveCount property will have incremented.  This is done for simplicity because it saves having to create a separate event for tracking MoveCount
            _puzzleModel.PlayerMoved += (sender, args) => OnNotifyPropertyChanged("MoveCount");

            // When the puzzle is complete, make a note of it.  We don't finish the level straight away because animations are still running.
            _puzzleModel.PuzzleCompleted += (sender, args) => _puzzleCompleted = true;
            Logger.Debug("PuzzleViewModel Initialise exit");
        }


        /// <summary>
        /// The implementation of the <see cref="PauseGameCommand"/> action
        /// </summary>
        /// <param name="ignoredParam">Ignored</param>
        public void PauseGameAction(object ignoredParam)
        {
            _puzzleModel.Stopwatch.Stop();
            GameState = GameStates.Paused;
        }

        /// <summary>
        /// The implementation of the <see cref="ResumeGameCommand"/> action
        /// </summary>
        /// <param name="ignoredParam">Ignored</param>
        public void ResumeGameAction(object ignoredParam)
        {
            _puzzleModel.Stopwatch.Start();
            GameState = GameStates.Running;
        }

        /// <summary>
        /// The implementation of the <see cref="ResumeGameCommand"/> action
        /// </summary>
        /// <param name="ignoredParam">Ignored</param>
        public void StartGameAction(object ignoredParam)
        {
            _puzzleModel.Stopwatch.Start();
            GameState = GameStates.Running;
        }


        /// <summary>
        ///   Resets the visuals of the puzzle as a result of the puzzle area changing shape.
        /// </summary>
        /// <param name="force">If <c>true</c> the we force a refresh of the puzzle, regardless of whether the old size is the same as the new size</param>
        /// <remarks>
        ///   This is invoked as part of the Setter on <see cref="PuzzleArea" /> . The size of each tile is calculated by dividing up the number of tiles in to equal amounts then set on each <see
        ///    cref="TileViewModel" /> instance within <see cref="Tiles" /> .
        /// </remarks>
        public void PuzzleModelPuzzleResized(bool force)
        {
            Size newTileSize = new Size(PuzzleArea.Width/_puzzleModel.TilesAcross,
                                        PuzzleArea.Height/_puzzleModel.TilesHigh);

            if (newTileSize.Equals(TileSize) && !force)
            {
                Logger.Debug("Ignoring PuzzleModel resize request as it's not changing: {0}", TileSize);
            }
            else
            {
                Logger.Debug("Updating TileSize from {0} to {1} (forced={2})", TileSize, newTileSize, force);
                TileSize = newTileSize;
                if (Tiles != null)
                {
                    foreach (TileViewModel tvm in Tiles)
                    {
                        tvm.Width = TileSize.Width;
                        tvm.Height = TileSize.Height;
                    }
                }
            }
        }


        /// <summary>
        /// Finds the position of the tile in <paramref name="tiles"/> that has the solved position passed by <paramref name="solvedPosition"/>
        /// </summary> 
        /// <param name="tiles">The tile grid to search</param>
        /// <param name="solvedPosition">The solved position of the tile to find</param>
        /// <returns>The position of the tile that belongs at <paramref name="solvedPosition"/></returns>
        private Position FindTilePosition(Position[][] tiles, Position solvedPosition)
        {
            for (int y = 0; y < tiles.Length; y++)
            {
                for (int x = 0; x < tiles[y].Length; x++)
                {
                    if (tiles[y][x].Equals(solvedPosition))
                    {
                        Position foundPosition = new Position(x, y);
                        Logger.Info("Found tile {0} at {1}", solvedPosition, foundPosition);
                        return foundPosition;
                    }
                }
            }
            throw new InvalidStateException("Unable to find tile with solved position {0} in tiles", solvedPosition);
        }

        /// <summary>
        /// Reconfigure this view model with the <paramref name="game"/>, showing <see cref="SaveGame.CurrentLevel"/>
        /// </summary>
        /// <remarks>
        /// We need the whole <see cref="SaveGame"/> object as we'll be updating the top level data within this object (e.g. <see cref="SaveGame.TotalMoves"/>)</remarks>
        /// <param name="game"></param>
        public void Initialise(SaveGame game)
        {
            Logger.Debug("Initialising PuzzleViewModel using level {0} of {1}", game.CurrentLevelIndex, game);

            // Set up the model based on the current level
            _puzzleModel.Initialise(game.CurrentLevel);

            // NOTE
            _puzzleModel.AddPlayer(game.LocalPlayerDetails.Name, PlayerType.Local,
                                   game.LocalPlayerDetails.PreferredColor, FindTilePosition(game.CurrentLevel.SolvedTilePositions, game.CurrentLevel.BlankTilePosition));

            // TODO What about remote players, how do we bring them in to the mix ?  Probably via a popup/Task equivalent that pauses the game until all remote players joined, or something!

            // Initialise our tile viewmodels simply wrapping them around the TileModel instances created within the puzzle model
            // These will later be passed as the DataContext for the individual tile controls in the puzzle control
            Tiles = new List<TileViewModel>();
            foreach (TileModel tileModel in _puzzleModel.AllTiles())
            {
                TileViewModel tmv = new TileViewModel(tileModel) {TileBorder = TileBorder};
                tmv.TileAnimationCompleted += CheckPuzzleCompleted;
                tmv.TileAnimationStarted += LogAnimationStarted;
                Tiles.Add(tmv);
            }

            // Register event handler for stopwatch to update the UI on each tick
            _puzzleModel.Stopwatch.Tick += (sender, args) => OnNotifyPropertyChanged("ElapsedTime");

            _currentLevelNumber = game.CurrentLevelIndex;
            PageTitle = LocalizationHelper.GetString("GamePageTitle", _currentLevelNumber + 1);
            // Have to add 1 as the first level should be "level 1", not "level 0"


            // Set up the fields used in the display of the start screen
            ImageTitle = game.CurrentLevel.Title;
            ImageText = game.CurrentLevel.Text;
            ImageOwner = game.CurrentLevel.ImageOwner;
            ImageLink = game.CurrentLevel.ImageLink;
            LicenseLink = game.CurrentLevel.License.Link;
            LicenseTitle = game.CurrentLevel.License.Title;

            _currentGame = game;

            Logger.Info("Loaded GameState as {0}", GameState);
            if (GameState == GameStates.Running)
            {
                _puzzleModel.Stopwatch.Start();
            }
            
            Logger.Debug("Initialise exit");
        }


        /// <summary>
        ///   Event handler that logs that an animation has been kicked off that should prevent the UI responding to the puzzle being completed
        /// </summary>
        /// <remarks>
        ///   Animations running pending completion are stored in <see cref="_animationsPending" />
        /// </remarks>
        /// <param name="sender"> The object representing the animation </param>
        /// <param name="e"> Ignored </param>
        private void LogAnimationStarted(object sender, EventArgs e)
        {
            _animationsPending.Add(sender);
            Logger.Debug("Added animation from {0} to the list of pending animations, now {1} animations active", sender,
                         _animationsPending.Count);
        }


        /// <summary>
        ///   Event handler invoked when any animation completes.  The object is removed from the <see cref="_animationsPending" /> list and if the model
        ///   has marked the puzzle as completed (<see cref="_puzzleCompleted" /> set via <see cref="PuzzleModel.PuzzleCompleted" />) then the UI
        ///   can respond to the user finishing the level
        /// </summary>
        /// <param name="sender"> The object representing the animation </param>
        /// <param name="unused"> Ignored </param>
        private void CheckPuzzleCompleted(object sender, EventArgs unused)
        {
            _animationsPending.Remove(sender);
            Logger.Debug("Removed animation {0} from the list of pending animations, now {1} animations active", sender,
                         _animationsPending.Count);
            if (_puzzleCompleted && _animationsPending.Count == 0)
            {
                CompleteLevel();
            }
        }


        /// <summary>
        /// Invoked when the user completes the level
        /// </summary>
        private void CompleteLevel()
        {
            _puzzleModel.Stopwatch.Stop();
            _puzzleModel.ShowAllTiles();
            CompletedTitle = LocalizationHelper.GetString("LevelFinishedCaption");
            CompletedText = LocalizationHelper.GetString("LevelFinishedMessage",
                                                         _currentLevelNumber + 1,
                                                         LocalizationHelper.GetTimeSpanString(
                                                             _puzzleModel.Stopwatch.ElapsedTime),
                                                         _puzzleModel.MoveCount,
                                                         _puzzleModel.MoveCount != 1 ? "moves" : "move");
            GameState = GameStates.Completed;
        }


        /// <summary>
        /// Loads the next level, or proceeds to <see cref="GameEnd"/> if all levels are complete
        /// </summary>
        /// <remarks>
        /// Invoked when the user taps the puzzle and <see cref="GameState"/> is <see cref="GameStates.Completed"/>
        /// </remarks>
        public void ProceedToNextLevel()
        {
            // Update with ElapsedTime and TotalMoves
            UpdateCurrentGame();

            // Overwrite the thumbnail with one of the completed image
            WriteableBitmap thumbnail = _puzzleModel.Image;
            thumbnail.Resize(LevelState.ThumbnailSize, LevelState.ThumbnailSize,
                                 WriteableBitmapExtensions.Interpolation.Bilinear);
            _currentGame.CurrentLevel.Thumbnail = thumbnail;
            _currentGame.CurrentLevelIndex++;
            SaveGameStorageManager.Instance.SaveGame(_currentGame);

            // Move on to the next level, or redirect to the "Game Completed" page
            Uri nextPageUri; 
            if (_currentGame.CurrentLevelIndex == _currentGame.Levels.Count)
            {
                Logger.Info("Player has finished the game, redirecting to the endgame page");
                nextPageUri = GameEnd.CreateNavigationUri(_currentGame.Id);
            }
            else
            {
                Logger.Info("Moving to level {0} of {1}", _currentGame.CurrentLevelIndex,
                            _currentGame.Levels.Count);
                nextPageUri = GamePage.CreateNavigationUri(_currentGame.Id, 1, GameStates.NotStarted);
            }
            Logger.Info("Navigating on to {0}", nextPageUri);
            _dontSaveGameOnNavigatingFrom = true;
                // Prevent the game being saved again when we navigate away (otherwise desirable when deactivating!)
            SendViewMessage(new NavigationMessageArgs(nextPageUri));
        }


        /// <summary>
        ///   Given the co-ordinates of a tap on the screen this methods then translates that in to a request to move a tile in a specific direction.
        /// </summary>
        /// <remarks>
        ///   There are 4 directions from which a tile can move in to a blank. This methods works out the preferred order of directions, based on the position of the tap relative to the player's blank square.
        /// </remarks>
        /// <param name="tapPosition"> The position on the screen that was tapped </param>
        public void MoveTileBasedOnTap(Point tapPosition)
        {
            Point relativeToBlank = tapPosition.Subtract(ConvertTilePositionToPoint(_puzzleModel.LocalPlayer.Position));
            IEnumerable<TileDirection> preferredDirections = CalculateDirectionPreferences(relativeToBlank);

            foreach (TileDirection dir in preferredDirections)
            {
                // Attempt to move in each of the preferred directions in order, but stop as soon as we successfully move
                if (_puzzleModel.MoveTile(_puzzleModel.LocalPlayer, dir)) break;
            }
        }

        public void MoveTileBasedOnFlick(double horizontalVelocity, double verticalVelocity)
        {
            TileDirection dir;
            if (Math.Abs(horizontalVelocity) > Math.Abs(verticalVelocity))
            {
                dir = horizontalVelocity > 0 ? TileDirection.FromLeft : TileDirection.FromRight;
            }
            else
            {
                dir = verticalVelocity > 0 ? TileDirection.FromAbove : TileDirection.FromBelow;
            }
            _puzzleModel.MoveTile(_puzzleModel.LocalPlayer, dir);
        }



        /// <summary>
        ///   Work out the preferred order of tile movement, based on where an input tap event occurred relative to the blank tile for that player
        /// </summary>
        /// <param name="tapPointRelativeToBlank"> A point offset from the centre of the blank tile (e.g. 0,0 would be right in the middle of the blank tile) </param>
        /// <returns> The order of preference for which tile to swap with the player's blank tile </returns>
        private static IEnumerable<TileDirection> CalculateDirectionPreferences(Point tapPointRelativeToBlank)
        {
            // TODO Fix this to work better WRT non 1:1 aspect ratios (assuming we ever need that!)
            TileDirection[] dirsArray = new TileDirection[4];
            /* Start by working out whether the tap was furthest away from the blank on the horizontal or vertical axis.  Whichever is greater, this will occupy
             * positions 0 and 2 in the dirsArray output, the lesser will occupy positions 1 and 3 */
            int horizOffset = Math.Abs(tapPointRelativeToBlank.X) > Math.Abs(tapPointRelativeToBlank.Y) ? 0 : 1;
            int vertOffset = horizOffset == 0 ? 1 : 0;
            dirsArray[horizOffset] = tapPointRelativeToBlank.X < 0 ? TileDirection.FromLeft : TileDirection.FromRight;
            dirsArray[horizOffset + 2] = tapPointRelativeToBlank.X < 0
                                             ? TileDirection.FromRight
                                             : TileDirection.FromLeft;
            dirsArray[vertOffset] = tapPointRelativeToBlank.Y < 0 ? TileDirection.FromAbove : TileDirection.FromBelow;
            dirsArray[vertOffset + 2] = tapPointRelativeToBlank.Y < 0
                                            ? TileDirection.FromBelow
                                            : TileDirection.FromAbove;
            return dirsArray;
        }



        /// <summary>
        ///   Converts a co-ordinate position of a tile within the grid (e.g. 0 to <see cref="TilesAcross" /> ) (as opposed to <see
        ///    cref="PuzzleArea" /> to pixel co-ordinates relative to the top level corner of the puzzle.
        /// </summary>
        /// <param name="tilePosition"> The position of the tile </param>
        /// <returns> The pixel co-ordinates of the centre of the specified tile, relative to the top-level corner of the puzzle control </returns>
        private Point ConvertTilePositionToPoint(Position tilePosition)
        {
            return new Point(TileSize.Width*tilePosition.X + TileSize.Width/2,
                             TileSize.Height*tilePosition.Y + TileSize.Height/2);
        }


        /// <summary>
        /// When the user navigates away from the page (or a system event interrupts us) then save our current position
        /// </summary>
        /// <remarks>
        /// This method has to be quick, as it impacts the user experience.  As <see cref="SaveGame"/> instances are only a few kilobytes this is fine</remarks>
        /// <param name="navigatingCancelEventArgs">Unused</param>
        public void OnNavigatingFrom(NavigatingCancelEventArgs navigatingCancelEventArgs)
        {
            Logger.Info("OnNavigatingFrom fired");

            if (GameState == GameStates.Running)
            {
                // Pause the game when we exit the page, so when the player resumes it doens't go straight in
                GameState = GameStates.Paused;
            }

            if (!_dontSaveGameOnNavigatingFrom && _currentGame != null)
            {
                Logger.Info("OnNavigatingFrom event fired, repopulating _currentGame with current state and saving it");
                UpdateCurrentGame();
                SaveGameStorageManager.Instance.SaveGame(_currentGame);
            }
            else
            {
                Logger.Debug(
                    "Not updating the save game because this navigation event was caused by a level progression, not by an external event");
            }
        }


        /// <summary>
        /// Updates the <see cref="SaveGame"/> instance stored in <see cref="_currentGame"/> with the current position of all the tiles and the stopwatch
        /// </summary>
        /// <remarks>
        /// This should be called before persisting the <see cref="SaveGame"/></remarks>
        private void UpdateCurrentGame()
        {
            LevelState currentLevel = _currentGame.CurrentLevel;
            // Stop the stopwatch
            currentLevel.MoveCount = MoveCount;
            currentLevel.ElapsedTime = _puzzleModel.Stopwatch.ElapsedTime;

            // Copy down tile positions from current grid in to the save game
            currentLevel.SolvedTilePositions = _puzzleModel.GetCurrentPuzzleState();
//            currentLevel.BlankTilePosition = _puzzleModel.LocalPlayer.Position;

            // Create the thumbnail image
            currentLevel.Thumbnail = CreateThumbnailBitmap();
        }


        /// <summary>
        /// Create a small thumbnail of the current state of the puzzle for use within the save game file
        /// </summary>
        /// <remarks>
        /// Not implemented yet, this currently just returns a blank thumbnail image </remarks>
        /// <returns>An empty bitmap of size <see cref="SaveGame.ThumbnailWidth"/> by <see cref="SaveGame.ThumbnailHeight"/></returns>
        private WriteableBitmap CreateThumbnailBitmap()
        {
            return Thumbnail;
        }

    }
}