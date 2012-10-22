using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows.Media;
using Locima.SlidingBlock.Common;
using Locima.SlidingBlock.Persistence;
using NLog;

namespace Locima.SlidingBlock.Model
{
    public class PuzzleModel
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        ///   The current state of the level, ready for persisting if necessary
        /// </summary>
        /// <remarks>
        ///   This object is passed to <see cref="Initialise" /> and is used to initialise this object.  When the game needs to be saved, this object is updated and extracted by the save game 
        ///   event handler
        /// </remarks>
        private LevelState _levelState;

        private TileModel[][] _puzzleGrid;

        /// <summary>
        ///   Reponsible for splitting up the final image in to tiles and issuing the brush for each tile
        /// </summary>
        private ImageDivider _tileBrushFactory;

        /// <summary>
        ///   Initialises <see cref="Players" /> and adds event handlers so that after each move we check to see if the puzzle is completed
        /// </summary>
        public PuzzleModel()
        {
            Players = new List<Player>();
            PlayerMoved += CheckIfPuzzleCompleted;
            PlayerMoved += (sender, args) => MoveCount++;
        }


        /// <summary>
        /// Retrieves the current state of all the tiles in the puzzle (used for saving a game in progress)
        /// </summary>
        /// <returns>Each position within the array contains a <see cref="Position"/> object which contains the solved position of that tile. Ends up in <see cref="LevelState.SolvedTilePositions"/></returns>
        public Position[][] GetCurrentPuzzleState()
        {
            Position[][] solvedPositions = new Position[TilesHigh][];
            for (int y=0; y<TilesHigh; y++)
            {
                solvedPositions[y] = new Position[TilesAcross];
                for (int x=0; x<TilesAcross; x++)
                {
                    solvedPositions[y][x] = _puzzleGrid[y][x].SolvedPosition;   
                }
            }
            return solvedPositions;
        }


        /// <summary>
        ///   The number of moves that the player has made on this level
        /// </summary>
        /// <remarks>
        ///   This will be persisted to <see cref="LevelState.MoveCount" />
        /// </remarks>
        public int MoveCount { get; private set; }

        /// <summary>
        ///   The timer that records how long the player has been playing this particular level
        /// </summary>
        public StopwatchModel Stopwatch { get; private set; }

        /// <summary>
        ///   The players who are currently playing the game
        /// </summary>
        /// <remarks>
        ///   Currently there is only ever one Player instance within this collection.  Future verisons hope to support multiplayer
        /// </remarks>
        public ICollection<Player> Players { get; private set; }

        /// <summary>
        ///   The Local player
        /// </summary>
        /// <remarks>
        ///   Returns the player within <see cref="Players" /> that is operating the local device
        /// </remarks>
        public Player LocalPlayer
        {
            get { return Players.FirstOrDefault(p => p.Type == PlayerType.Local); }
        }


        /// <summary>
        ///   Returns the total number of tiles across the puzzle is
        /// </summary>
        public int TilesAcross
        {
            get { return _puzzleGrid[0].Length; }
        }


        /// <summary>
        ///   Returns the total number of tiles high the puzzle is
        /// </summary>
        public int TilesHigh
        {
            get { return _puzzleGrid.Length; }
        }

        /// <summary>
        ///   Raised when a player wishes to a move a tile.
        /// </summary>
        /// <remarks>
        ///   The move can be cancelled during this event
        /// </remarks>
        public event EventHandler<PlayerMovingEventArgs> PlayerMoving;

        /// <summary>
        ///   Raised when a move has been approved and the animation should be started
        /// </summary>
        public event EventHandler<PlayerMovedEventArgs> PlayerMoved;

        /// <summary>
        ///   Raised when the puzzle has been completed.
        /// </summary>
        /// <remarks>
        ///   Note that this event is raised when the model represents a completed puzzle.  The tile sliding animation will still be running at this point
        /// </remarks>
        public event EventHandler<EventArgs> PuzzleCompleted;

        /// <summary>
        ///   Determines whether each tile of the puzzle is in its "solved" position, i.e. the puzzle has been completed successfully
        /// </summary>
        /// <returns> <c>true</c> if the puzzle has been completed, <c>false</c> otherwise</returns>
        private bool IsPuzzleCompleted()
        {
            for (int v = 0; v < TilesHigh; v++)
            {
                for (int a = 0; a < TilesAcross; a++)
                {
                    if (!_puzzleGrid[v][a].IsInSolvedPosition) return false;
                }
            }
            return true;
        }


        /// <summary>
        ///   Checks to see if the puzzle has been completed using <see cref="IsPuzzleCompleted" /> and if so, raises <see
        ///    cref="PuzzleCompleted" />.
        /// </summary>
        /// <remarks>
        ///   This is an event handler for the <see cref="PlayerMoved" /> event
        /// </remarks>
        private void CheckIfPuzzleCompleted(object sender, PlayerMovedEventArgs e)
        {
            if (IsPuzzleCompleted())
            {
                Logger.Info("Raising PuzzleCompleted");
                SafeRaise.Raise(PuzzleCompleted, this, EventArgs.Empty);
            }
        }


        /// <summary>
        ///   Adds a new player to the game
        /// </summary>
        /// <param name="playerName"> The name of the player </param>
        /// <param name="playerType"> Whether the player is a local or remote player (always local currently) </param>
        /// <param name="playerColor"> The brush to use to paint the "blank" tile that this player represents (typically derived from <see
        ///    cref="PlayerDetails.PreferredColor" /> ) </param>
        /// <param name="playerPosition"> The position of the player's "blank" tile </param>
        /// <exception cref="ArgumentException">If the player's position is off the puzzle grid or another player occupies the same
        ///   <paramref name="playerPosition" />
        /// </exception>
        public void AddPlayer(string playerName, PlayerType playerType, Color playerColor, Position playerPosition)
        {
            if (playerPosition.X < 0 || playerPosition.X >= TilesAcross)
                throw new ArgumentException(string.Format("Player {0} X is outside of grid.  Must be 0 <= {1} < {2}",
                                                          playerName, playerPosition.X, TilesAcross));
            if (playerPosition.Y < 0 || playerPosition.Y >= TilesHigh)
                throw new ArgumentException(string.Format("Player {0} Y is outside of grid.  Must be 0 <= {1} < {2}",
                                                          playerName, playerPosition.Y, TilesHigh));

            TileModel tile = _puzzleGrid[playerPosition.Y][playerPosition.X];
            if (tile.Player != null)
            {
                throw new ArgumentException(
                    string.Format("Two players cannot occupy the same tile: {0} and {1} at {2}", tile.Player,
                                  playerName, playerPosition.X));
            }

            Player newPlayer = new Player(tile)
                {
                    Name = playerName,
                    Type = playerType,
                    PlayerColor = playerColor
                };
            Logger.Info("Adding player {0} and allocating to tile {1}", newPlayer, tile.Position);
            tile.Player = newPlayer;
            Players.Add(newPlayer);
        }


        /// <summary>
        ///   Initialise the model for the grid size specified.
        /// </summary>
        /// <remarks>
        /// </remarks>
        public void Initialise(LevelState level)
        {
            if (level == null) throw new ArgumentNullException("level");

            _levelState = level;

            int tilesAcross = _levelState.TilesAcross;
            int tilesHigh = _levelState.TilesHigh;
            Logger.Info("Initialising Puzzle Model : {0},{1}", tilesAcross, tilesHigh);
            Debug.Assert(tilesAcross >= 2, "Puzzle must be 2 or more tiles wide");
            Debug.Assert(tilesHigh >= 2, "Puzzle must be 2 or more tiles high");

            // Initialise the grid of TileModel objects
            _puzzleGrid = new TileModel[tilesHigh][];

            _puzzleGrid = ArrayTools.ConvertTo<Position, TileModel>(_levelState.SolvedTilePositions,
                                                                        solvedPosition =>
                                                                        new TileModel(this)
                                                                            {SolvedPosition = solvedPosition});
            _tileBrushFactory = new ImageDivider
                {
                    TilesAcross = _levelState.TilesAcross,
                    TilesHigh = _levelState.TilesHigh,
                    ImageBitmap = _levelState.Image
                };

            Stopwatch = new StopwatchModel {BaseElapsedTime = _levelState.ElapsedTime};
            MoveCount = _levelState.MoveCount;

            Logger.Info("Initialised puzzle successfully");

            LogPuzzleGrid();
        }


        /// <summary>
        ///   Returns the tile model at the co-ordinates passed
        /// </summary>
        /// <returns> The tile at the co-ordinates specified </returns>
        /// <exception cref="IndexOutOfRangeException">if the co-ordinates are outside the puzzle grid</exception>
        public TileModel TileAt(int x, int y)
        {
            return _puzzleGrid[y][x];
        }


        /// <summary>
        ///   Returns the tile model at the position passed
        /// </summary>
        /// <returns> The tile at the position specified </returns>
        /// <exception cref="IndexOutOfRangeException">if the position is outside the puzzle grid</exception>
        public TileModel TileAt(Position position)
        {
            return _puzzleGrid[position.Y][position.X];
        }


        public bool MoveTile(Player player, TileDirection direction)
        {
            Logger.Info("Moving {0} in direction {1}", player, direction);
            bool tileMoved = false;
            switch (direction)
            {
                case TileDirection.FromAbove:
                    if (player.Position.Y > 0)
                    {
                        MovePlayer(player.Position, Position.Create(player.Position, 0, -1));
                        tileMoved = true;
                    }
                    break;
                case TileDirection.FromBelow:
                    if (player.Position.Y < TilesHigh - 1)
                    {
                        MovePlayer(player.Position, Position.Create(player.Position, 0, 1));
                        tileMoved = true;
                    }
                    break;
                case TileDirection.FromLeft:
                    if (player.Position.X > 0)
                    {
                        MovePlayer(player.Position, Position.Create(player.Position, -1, 0));
                        tileMoved = true;
                    }
                    break;
                case TileDirection.FromRight:
                    if (player.Position.X < TilesAcross - 1)
                    {
                        MovePlayer(player.Position, Position.Create(player.Position, 1, 0));
                        tileMoved = true;
                    }
                    break;
                default:
                    throw new InvalidOperationException(
                        string.Format("Unexpected direction passed to MoveTile({0}, {1})", player, direction));
            }
            return tileMoved;
        }


        private void MovePlayer(Position currentPosition, Position newPosition)
        {
            TileModel swapTile = TileAt(newPosition);
            TileModel playerTile = TileAt(currentPosition);
            PlayerMovingEventArgs args = new PlayerMovingEventArgs {PlayerTile = playerTile, PuzzleTile = swapTile};
            SafeRaise.Raise(PlayerMoving, this, args);

            if (args.Cancel)
            {
                Logger.Info("Move cancelled by event PlayerMoving event handler");
            } else
            {

                // Swap the tiles at the positions oldX, oldY and newX and newY
                Logger.Info("Moving player {0} from ({1}) to ({2})", playerTile.Player.Name,
                            currentPosition, newPosition);
                _puzzleGrid[currentPosition.Y][currentPosition.X] = swapTile;
                _puzzleGrid[newPosition.Y][newPosition.X] = playerTile;
                SafeRaise.Raise(PlayerMoved, this,
                                new PlayerMovedEventArgs { PlayerTile = playerTile, PuzzleTile = swapTile});
            }
        }


        public IEnumerable<TileModel> AllTiles()
        {
            List<TileModel> tiles = new List<TileModel>();
            for (int y = 0; y < TilesHigh; y++)
            {
                for (int x = 0; x < TilesAcross; x++)
                {
                    tiles.Add(TileAt(x, y));
                }
            }
            return tiles;
        }


        public Brush GetTileBrush(TileModel tileModel)
        {
            return _tileBrushFactory.GetTileBrush(tileModel.SolvedPosition.X, tileModel.SolvedPosition.Y);
        }


        public void LogPuzzleGrid()
        {
            if (Logger.IsDebugEnabled)
            {
                StringBuilder sb = new StringBuilder("Current grid status:");
                sb.Append(Environment.NewLine);
                foreach (TileModel[] t in _puzzleGrid)
                {
                    for (int a = 0; a < t.Length; a++)
                    {
                        sb.AppendFormat("({0},{1})", t[a].SolvedPosition.X, t[a].SolvedPosition.Y);
                        if (a < t.Length - 1) sb.Append(", ");
                    }
                    sb.Append(Environment.NewLine);
                }
                Logger.Debug(sb.ToString());
            }
        }


        // TODO Surely there's a better way of doing this?  This is how a TileModel knows where it is in the puzzle
        public Position TilePosition(TileModel tileModel)
        {
            for (int y = 0; y < TilesHigh; y++)
            {
                for (int x = 0; x < TilesAcross; x++)
                {
                    if (TileAt(x, y) == tileModel) return new Position(x,y);
                }
            }
            throw new InvalidOperationException(string.Format("Tile {0} is not present in the puzzle grid", tileModel));
        }
    }
}   