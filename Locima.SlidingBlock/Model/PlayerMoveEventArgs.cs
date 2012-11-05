using System;

namespace Locima.SlidingBlock.Model
{

    /// <summary>
    /// Base event arguments class used to provide the parameters for a player moving or has moved their tile (which, of course, involves both the player tile and a visible tile moving)
    /// </summary>
    /// <remarks>
    /// Don't confuse this with <see cref="TileMoveEventArgs"/> which is for a single tile moving.</remarks>
    public abstract class BasePlayerMovementEventArgs : EventArgs
    {
        /// <summary>
        /// The tile representing the player that has moved, or is about to move
        /// </summary>
        public TileModel PlayerTile { get; set; }

        /// <summary>
        /// The tile of the puzzle that has moved, or is about to move
        /// </summary>
        public TileModel PuzzleTile { get; set; }
    }

    /// <summary>
    /// Event arguments used for when a player has moved successfully
    /// </summary>
    public class PlayerMovedEventArgs : BasePlayerMovementEventArgs
    {
        
    }

    /// <summary>
    /// Event arguments used for when a player has requested a move
    /// </summary>
    public class PlayerMovingEventArgs: BasePlayerMovementEventArgs
    {

        /// <summary>
        /// Defaults to <c>false</c>.  If set to <c>true</c> by the event handler then the tile move will be aborted
        /// </summary>
        public bool Cancel { get; set; }
    }
}