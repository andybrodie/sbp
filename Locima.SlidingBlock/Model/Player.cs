using System.Windows.Media;
using Locima.SlidingBlock.Common;

namespace Locima.SlidingBlock.Model
{
    public class Player
    {
        public PlayerType Type;
        private readonly TileModel _tile;
        public Position Position { get { return _tile.Position;  } }
        public string Name { get; set; }

        /// <summary>
        /// The brush used to draw the tile that represents this player
        /// </summary>
        public Color PlayerColor { get; set; }

        public Player(TileModel tile)
        {
            _tile = tile;
        }

        public override string ToString()
        {
            return string.Format("Player({0}, {1} at {2})", Type, Name, Position);
        }
        
    }
}