using System;
using System.Windows.Media;
using Locima.SlidingBlock.Common;
using NLog;

namespace Locima.SlidingBlock.Model
{
    public class TileModel
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        
        private readonly PuzzleModel _puzzleModel;

        public TileModel(PuzzleModel puzzleModel)
        {
            _puzzleModel = puzzleModel;
            _puzzleModel.PlayerMoving += PuzzleModelOnPlayerMoving;
            _puzzleModel.PlayerMoved += PuzzleModelOnPlayerMoved;
        }

        public Brush TileBrush { get { return _puzzleModel.GetTileBrush(this); } 
        }

        public Color PlayerBrush
        {
            get { return Player == null ? default(Color) : Player.PlayerColor; }
        }
    
        public Position SolvedPosition { get; set; }

        public bool IsInSolvedPosition
        {
            get { return Position.X == SolvedPosition.X && Position.Y == SolvedPosition.Y; }
        }

        public Player Player { get; set; }

        public bool IsPlayerTile
        {
            get { return Player != null; }
        }

        public Position Position
        {
            get { return _puzzleModel.TilePosition(this); }
        }

        public event EventHandler<TileMoveEventArgs> TileMoving;
        public event EventHandler<TileMoveEventArgs> TileMoved;


        private void PuzzleModelOnPlayerMoving(object sender, PlayerMovingEventArgs playerMovingEventArgs)
        {
            if (playerMovingEventArgs.PlayerTile == this)
            {
                TileMovingEventArgs args = new TileMovingEventArgs(playerMovingEventArgs.PuzzleTile.Position,
                                                   playerMovingEventArgs.PlayerTile.Position);
                SafeRaise.Raise(TileMoving, this, args);
                if (args.Cancel)
                {
                    playerMovingEventArgs.Cancel = true;
                }
            }
            else if (playerMovingEventArgs.PuzzleTile == this)
            {
                TileMovingEventArgs args = new TileMovingEventArgs(playerMovingEventArgs.PlayerTile.Position,
                                                   playerMovingEventArgs.PuzzleTile.Position);
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
                                new TileMovedEventArgs(playerMoveEventArgs.PuzzleTile.Position,
                                                       playerMoveEventArgs.PlayerTile.Position));
            }
            else if (playerMoveEventArgs.PuzzleTile == this)
            {
                SafeRaise.Raise(TileMoved, this,
                                new TileMovedEventArgs(playerMoveEventArgs.PlayerTile.Position,
                                                       playerMoveEventArgs.PuzzleTile.Position));
            }
        }


    }
}