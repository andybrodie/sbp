namespace Locima.SlidingBlock.Model
{

    /// <summary>
    /// Used to indicate the direction of movement of a tile
    /// </summary>
    public enum TileDirection
    {
        /// <summary>
        /// Tile is moving down
        /// </summary>
        FromAbove = 0, 

        /// <summary>
        /// Tile is moving up
        /// </summary>
        FromBelow = 1, 

        /// <summary>
        /// Tile is moving right
        /// </summary>
        FromLeft = 2, 

        /// <summary>
        /// Tile is moving left
        /// </summary>
        FromRight = 3
    }
}