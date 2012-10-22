using System;

namespace Locima.SlidingBlock.Model
{
    public abstract class BasePlayerMovementEventArgs : EventArgs
    {
        public TileModel PlayerTile { get; set; }
        public TileModel PuzzleTile { get; set; }
    }

    public class PlayerMovedEventArgs : BasePlayerMovementEventArgs
    {
        
    }

    public class PlayerMovingEventArgs: BasePlayerMovementEventArgs
    {
        public bool Cancel { get; set; }
    }
}