using System;
using Locima.SlidingBlock.Common;

namespace Locima.SlidingBlock.Model
{
    public abstract class TileMoveEventArgs : EventArgs
    {
        protected TileMoveEventArgs(Position oldPosition, Position newPosition)
        {
            OldPosition = oldPosition;
            NewPosition = newPosition;
        }

        public Position OldPosition { get; private set; }
        public Position NewPosition { get; private set; }
    }

    public class TileMovingEventArgs : TileMoveEventArgs
    {
        public TileMovingEventArgs(Position oldPosition, Position newPosition)
            : base(oldPosition, newPosition)
        {
        }

        public bool Cancel { get; set; }
    }


    public class TileMovedEventArgs : TileMoveEventArgs
    {
        public TileMovedEventArgs(Position oldPosition, Position newPosition)
            : base(oldPosition, newPosition)
        {
        }
    }
}