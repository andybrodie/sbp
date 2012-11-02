using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using Locima.SlidingBlock.Common;
using Locima.SlidingBlock.Controls;
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

        /// <summary>
        ///   The current game, as saved by the player
        /// </summary>
        private SaveGame _currentGame;

        /// <summary>
        ///   The current level number, used to display the <see cref="PageTitle" /> and the level completion message
        /// </summary>
        private int _currentLevelNumber;

        private bool _isPaused;

        /// <summary>
        ///   Backing field for <see cref="PageTitle" />
        /// </summary>
        private string _pageTitle;

        private ICommand _pauseGameCommand;

        /// <summary>
        ///   Backing field for <see cref="PuzzleArea" />
        /// </summary>
        private Size _puzzleArea;

        /// <summary>
        ///   Set when the model reports that the puzzle has been completed
        /// </summary>
        private bool _puzzleCompleted;

        private PuzzleModel _puzzleModel;
        private bool _dontSaveGameOnNavigatingFrom;

        /// <summary>
        /// Invoked by the view when the user wishes to pause the game
        /// </summary>
        public ICommand PauseGameCommand
        {
            get { return _pauseGameCommand; }
        }

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
                    PuzzleModelPuzzleResized();
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
        ///   Is true when the stopwatch is paused, false if the stopwatch is runnning
        /// </summary>
        public bool IsPaused
        {
            get { return _isPaused; }
            set
            {
                _isPaused = value;
                OnNotifyPropertyChanged("IsPaused");
            }
        }


        /// <summary>
        /// Initialises the view model, creating a new <see cref="PuzzleModel"/>.
        /// </summary>
        /// <remarks>
        /// This doens't do much until the <see cref="Configure"/> is called.</remarks>
        public void Initialise()
        {
            Logger.Debug("PuzzleViewModel Initialise entry");
            _pauseGameCommand = new DelegateCommand(PauseGameAction);

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
            if (_puzzleModel.Stopwatch.IsRunning)
            {
                _puzzleModel.Stopwatch.Stop();
            }
            else
            {
                _puzzleModel.Stopwatch.Start();
            }
        }


        /// <summary>
        ///   Resets the visuals of the puzzle as a result of the puzzle area changing shape.
        /// </summary>
        /// <remarks>
        ///   This is invoked as part of the Setter on <see cref="PuzzleArea" /> . The size of each tile is calculated by dividing up the number of tiles in to equal amounts then set on each <see
        ///    cref="TileViewModel" /> instance within <see cref="Tiles" /> .
        /// </remarks>
        public void PuzzleModelPuzzleResized()
        {
            Size newTileSize = new Size(PuzzleArea.Width/_puzzleModel.TilesAcross,
                                        PuzzleArea.Height/_puzzleModel.TilesHigh);

            if (newTileSize.Equals(TileSize))
            {
                Logger.Debug("Ignoring PuzzleModel resize request as it's not changing: {0}", TileSize);
            }
            else
            {
                Logger.Debug("Updating TileSize from {0} to {1}", TileSize, newTileSize);
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
        /// Reconfigure this view model with the <paramref name="game"/>, showing <see cref="SaveGame.CurrentLevel"/>
        /// </summary>
        /// <remarks>
        /// We need the whole <see cref="SaveGame"/> object as we'll be updating the top level data within this object (e.g. <see cref="SaveGame.TotalMoves"/>)</remarks>
        /// <param name="game"></param>
        public void Configure(SaveGame game)
        {
            Logger.Debug("Configure with SaveGame entry");

            _puzzleModel.Initialise(game.CurrentLevel);

            _puzzleModel.AddPlayer(game.LocalPlayerDetails.Name, PlayerType.Local,
                                   game.LocalPlayerDetails.PreferredColor, game.LocalPlayer.Position);

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

            // Register event handler for stopwatch to update the UI
            _puzzleModel.Stopwatch.Tick += (sender, args) => OnNotifyPropertyChanged("ElapsedTime");
            _puzzleModel.Stopwatch.Pause += (sender, args) => IsPaused = true;
            _puzzleModel.Stopwatch.Resume += (sender, args) => IsPaused = false;
            _puzzleModel.Stopwatch.Start();

            _currentLevelNumber = game.CurrentLevelIndex;
            PageTitle = LocalizationHelper.GetString("GamePageTitle", _currentLevelNumber + 1);
                // Have to add 1 as the first level should be "level 1", not "level 0"

            _currentGame = game;

            Logger.Debug("Configure exit");
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
        /// <param name="e"> Ignored </param>
        private void CheckPuzzleCompleted(object sender, EventArgs e)
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
        ///   Shows a level completion message and moves to the next level
        /// </summary>
        private void CompleteLevel()
        {
            _puzzleModel.Stopwatch.Stop();

            // Congratulate the user, when they've finished celebrating, the ok button will invoke ProceedToNextLevel
            SendViewMessage(new ConfirmationMessageArgs
                {
                    Title = LocalizationHelper.GetString("LevelFinishedCaption"),
                    Message = LocalizationHelper.GetString("LevelFinishedMessage",
                                                           _currentLevelNumber + 1,
                                                           LocalizationHelper.GetTimeSpanString(
                                                               _puzzleModel.Stopwatch.ElapsedTime),
                                                           _puzzleModel.MoveCount,
                                                           _puzzleModel.MoveCount != 1 ? "moves" : "move"),
                    OnOkCommand = new DelegateCommand(ProceedToNextLevel)
                });
        }


        private void ProceedToNextLevel(object obj)
        {
            // Update with ElapsedTime and TotalMoves
            UpdateCurrentGame();
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
                nextPageUri = GamePage.CreateNavigationUri(_currentGame.Id, true);
            }
            Logger.Info("Navigating on to {0}", nextPageUri);
            _dontSaveGameOnNavigatingFrom = true;
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
            Point relativeToBlank = Subtract(tapPosition,
                                             ConvertTilePositionToPoint(_puzzleModel.LocalPlayer.Position));
            DirectionPreference preferredDirections = CalculateDirectionPreferences(relativeToBlank);

            foreach (TileDirection dir in preferredDirections.Preferences)
            {
                // Attempt to move in each of the preferred directions in order, but stop as soon as we successfully move
                if (_puzzleModel.MoveTile(_puzzleModel.LocalPlayer, dir)) break;
            }
        }


        /// <summary>
        ///   Work out the preferred order of tile movement, based on where an input tap event occurred relative to the blank tile for that player
        /// </summary>
        /// <param name="tapPointRelativeToBlank"> A point offset from the centre of the blank tile (e.g. 0,0 would be right in the middle of the blank tile) </param>
        /// <returns> The order of preference for which tile to swap with the player's blank tile </returns>
        private static DirectionPreference CalculateDirectionPreferences(Point tapPointRelativeToBlank)
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
            return new DirectionPreference(dirsArray);
        }


        /// <summary>
        ///   Work out the relative position of <paramref name="p1" /> to <paramref name="p2" />
        /// </summary>
        /// <param name="p1"> The point to substract <paramref name="p2" /> from </param>
        /// <param name="p2"> The origin </param>
        /// <returns> </returns>
        private static Point Subtract(Point p1, Point p2)
        {
            return new Point(p1.X - p2.X, p1.Y - p2.Y);
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
            // Stop the stopwatch
            _currentGame.CurrentLevel.MoveCount = MoveCount;
            _currentGame.CurrentLevel.ElapsedTime = _puzzleModel.Stopwatch.ElapsedTime;

            // Copy down tile positions from current grid in to the save game
            _currentGame.CurrentLevel.SolvedTilePositions = _puzzleModel.GetCurrentPuzzleState();
            _currentGame.LocalPlayer.Position = _puzzleModel.LocalPlayer.Position;
            _currentGame.CurrentLevel.Thumbnail = CreateThumbnailBitmap();
        }


        /// <summary>
        /// Create a small thumbnail of the current state of the puzzle for use within the save game file
        /// </summary>
        /// <remarks>
        /// Not implemented yet, this currently just returns a blank thumbnail image </remarks>
        /// <returns>An empty bitmap of size <see cref="SaveGame.ThumbnailWidth"/> by <see cref="SaveGame.ThumbnailHeight"/></returns>
        private WriteableBitmap CreateThumbnailBitmap()
        {
            Logger.Debug("Haven't implemented thumbnail generation yet!");
            return new WriteableBitmap(SaveGame.ThumbnailWidth, SaveGame.ThumbnailHeight);
        }
    }
}