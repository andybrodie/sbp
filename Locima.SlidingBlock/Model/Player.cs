using System.Windows.Media;
using Locima.SlidingBlock.Common;
using Locima.SlidingBlock.Persistence;

namespace Locima.SlidingBlock.Model
{

    /// <summary>
    /// Represents a player active within a <see cref="PuzzleModel"/>.
    /// </summary>
    /// <remarks>
    /// Not to be confused with <see cref="PlayerDetails"/> or <see cref="PlayerLink"/></remarks>
    /// <see cref="PuzzleModel.Players"/>
    public class Player
    {
        /// <summary>
        /// The type of player (currently unused)
        /// </summary>
        public PlayerType Type;
        
        /// <summary>
        /// The tile this player is occupying
        /// </summary>
        private readonly TileModel _tile;

        /// <summary>
        /// The position of the tile relative to other tiles (delegates to <see cref="Common.Position"/>)
        /// </summary>
        public Position Position { get { return _tile.Position;  } }

        /// <summary>
        /// The name of the player
        /// </summary>
        /// <remarks>Currently only used for debugging</remarks>
        public string Name { get; set; }

        /// <summary>
        /// The brush used to draw the tile that represents this player
        /// </summary>
        public Color PlayerColor { get; set; }

        /// <summary>
        /// Iniitalise the player on top of the <paramref name="tile"/> specified
        /// </summary>
        /// <param name="tile"></param>
        public Player(TileModel tile)
        {
            _tile = tile;
        }

        /// <summary>
        /// Succinct serislisation useful for debugging
        /// </summary>
        public override string ToString()
        {
            return string.Format("Player({0}, {1} at {2})", Type, Name, Position);
        }
        
    }
}