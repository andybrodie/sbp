using System;
using System.Windows.Media;
using Locima.SlidingBlock.Common;
using Locima.SlidingBlock.ViewModel;
using NLog;

namespace Locima.SlidingBlock.Model
{

    /// <summary>
    /// The MVVM model for each Tile
    /// </summary>
    /// <remarks>
    /// The "one version of the truth" for a tile's model is actually held within the <see cref="PuzzleModel"/>, so this class effectively facades 
    /// the <see cref="PuzzleModel"/>'s data for a specific tile
    /// </remarks>
    public class TileModel
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        
        private readonly PuzzleModel _puzzleModel;

        /// <summary>
        /// Fired when any property on the model changes, includes the name of the property
        /// </summary>
        public EventHandler<PropertyEventChangeArgs> ModelPropertyChanged;

        /// <summary>
        /// Store the <paramref name="puzzleModel"/> and adds this class as a listener for the <see cref="PuzzleModel.PlayerMoving"/> and <see cref="PuzzleModel.PlayerMoved"/> events
        /// so we know when things change
        /// </summary>
        /// <param name="puzzleModel">The puzzle model that owns this tile</param>
        public TileModel(PuzzleModel puzzleModel)
        {
            _puzzleModel = puzzleModel;
            _puzzleModel.PlayerMoving += PuzzleModelOnPlayerMoving;
            _puzzleModel.PlayerMoved += PuzzleModelOnPlayerMoved;
        }

        /// <summary>
        /// The brush for this tile
        /// </summary>
        /// <see cref="PuzzleModel.GetTileBrush"/>
        public Brush TileBrush { get { return _puzzleModel.GetTileBrush(this); } }

        /// <summary>
        /// The brush for the player (if this tile doesn't represent a player then this returns a default value that will never be used (need a default as <see cref="Color"/> is a struct.
        /// </summary>
        public Color PlayerBrush
        {
            get { return Player == null ? default(Color) : Player.PlayerColor; }
        }

        /// <summary>
        /// The position the tile needs to be in for the level to be compelted
        /// </summary>
        public Position SolvedPosition { get; set; }

        /// <summary>
        /// Determines whether the tile is in the solved position
        /// </summary>
        /// <remarks>Compares <see cref="Position"/> with <see cref="SolvedPosition"/></remarks>
        public bool IsInSolvedPosition
        {
            get { return Position.Equals(SolvedPosition); }
        }

        /// <summary>
        /// The player that this tile represents, is null for all but one tile
        /// </summary>
        public Player Player { get; set; }

        /// <summary>
        /// Determines whether this tile is a player tile (if <see cref="Player"/> is not null then this returns <c>true</c>)
        /// </summary>
        public bool IsPlayerTile
        {
            get { return Player != null; }
        }

        /// <summary>
        /// The position of this tile within the puzzle
        /// </summary>
        public Position Position
        {
            get { return _puzzleModel.TilePosition(this); }
        }

        /// <summary>
        /// Raised when <see cref="PuzzleModel.PlayerMoving"/> is raised and it affects this tile
        /// </summary>
        /// <remarks>This event is captured by the <see cref="TileViewModel"/></remarks>
        public event EventHandler<TileMoveEventArgs> TileMoving;


        /// <summary>
        /// Raised when <see cref="PuzzleModel.PlayerMoved"/> is raised and it affects this tile
        /// </summary>
        /// <remarks>This event is captured by the <see cref="TileViewModel"/></remarks>
        public event EventHandler<TileMoveEventArgs> TileMoved;


        private void PuzzleModelOnPlayerMoving(object sender, PlayerMovingEventArgs playerMovingEventArgs)
        {
            if (playerMovingEventArgs.PlayerTile == this)
            {
                TileMovingEventArgs args = new TileMovingEventArgs
                                               {
                                                   OldPosition = playerMovingEventArgs.PuzzleTile.Position,
                                                   NewPosition = playerMovingEventArgs.PlayerTile.Position
                                               };
                SafeRaise.Raise(TileMoving, this, args);
                if (args.Cancel)
                {
                    playerMovingEventArgs.Cancel = true;
                }
            }
            else if (playerMovingEventArgs.PuzzleTile == this)
            {
                TileMovingEventArgs args = new TileMovingEventArgs
                {
                    OldPosition = playerMovingEventArgs.PlayerTile.Position,
                    NewPosition = playerMovingEventArgs.PuzzleTile.Position
                };
                SafeRaise.Raise(TileMoving, this, args);
                if (args.Cancel)
                {
                    playerMovingEventArgs.Cancel = true;
                }
            }
        }


        private void PuzzleModelOnPlayerMoved(object sender, PlayerMovedEventArgs playerMoveEventArgs)
        {
            if (playerMoveEventArgs.PlayerTile == this)
            {
                SafeRaise.Raise(TileMoved, this,
                                new TileMovedEventArgs
                                    {
                                        OldPosition = playerMoveEventArgs.PuzzleTile.Position,
                                        NewPosition = playerMoveEventArgs.PlayerTile.Position
                                    });
            }
            else if (playerMoveEventArgs.PuzzleTile == this)
            {
                SafeRaise.Raise(TileMoved, this,
                                new TileMovedEventArgs
                                {
                                    OldPosition = playerMoveEventArgs.PlayerTile.Position,
                                    NewPosition = playerMoveEventArgs.PuzzleTile.Position
                                });
            }
        }


        /// <summary>
        /// Forces the tile image to be shown, regardless of the player settings
        /// </summary>
        public void ShowImage()
        {
            // TODO This is crude an irreversible
            Player = null;
            SafeRaise.Raise(ModelPropertyChanged, this, new PropertyEventChangeArgs { Name = "Player"});
        }
    }
}