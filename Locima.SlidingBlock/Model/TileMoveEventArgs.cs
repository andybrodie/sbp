using System;
using Locima.SlidingBlock.Common;

namespace Locima.SlidingBlock.Model
{

    /// <summary>
    /// Base event arguments class used to provide the parameters for either a tile moving event (see <see cref="TileMovingEventArgs"/>) or a tile moved event (see <see cref="TileMovedEventArgs"/>).
    /// </summary>
    /// <remarks>
    /// Don't confuse this with <see cref="BasePlayerMovementEventArgs"/> which is for a player moving (i.e. both the player tile and the puzzle tile swap places)</remarks>
    public abstract class TileMoveEventArgs : EventArgs
    {
        /// <summary>
        /// The old or current position of the tile
        /// </summary>
        public Position OldPosition { get; set; }

        /// <summary>
        /// The new or target position of the tile
        /// </summary>
        public Position NewPosition { get; set; }
    }


    /// <summary>
    /// Used for parameterising the <see cref="TileModel.TileMoving"/> event
    /// </summary>
    public class TileMovingEventArgs : TileMoveEventArgs
    {

        /// <summary>
        /// Defaults to <c>false</c>.  If set to <c>true</c> by the event handler then the tile move will be aborted
        /// </summary>
        public bool Cancel { get; set; }
    }


    /// <summary>
    /// Used for parameterising the <see cref="TileModel.TileMoved"/> event
    /// </summary>
    public class TileMovedEventArgs : TileMoveEventArgs
    {
    }
}