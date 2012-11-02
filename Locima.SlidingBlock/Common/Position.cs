using System.Runtime.Serialization;

namespace Locima.SlidingBlock.Common
{

    /// <summary>
    /// Represents the position of a tile relative to the other tiles.  <see cref="X"/>=0, <see cref="Y"/>=0 represents the top left tile.
    /// </summary>
    [DataContract]
    public class Position
    {

        /// <summary>
        /// No-op
        /// </summary>
        public Position()
        {}

        /// <summary>
        /// Initialises with the parameters passed
        /// </summary>
        /// <param name="x"><see cref="Position.X"/> value</param>
        /// <param name="y"><see cref="Position.Y"/> value</param>
        public Position(int x, int y)
        {
            X = x;
            Y = y;
        }

        /// <summary>
        /// X co-ordinate of a tile
        /// </summary>
        [DataMember]
        public int X { get; set; }

        /// <summary>
        /// Y co-ordinate of a tile
        /// </summary>
        [DataMember]
        public int Y { get; set; }

        /// <summary>
        /// XOR of the hashcodes for <see cref="X"/> and <see cref="Y"/>
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return X.GetHashCode() ^ Y.GetHashCode();
        }

        /// <summary>
        /// Includes <see cref="X"/> and <see cref="Y"/>
        /// </summary>
        /// <returns><c>Position(x,y)</c></returns>
        public override string ToString()
        {
            return string.Format("Position({0},{1})", X, Y);
        }

        /// <summary>
        /// Returns true if both <see cref="X"/> and <see cref="Y"/> are the same on both this and on <paramref name="obj"/>
        /// </summary>
        /// <param name="obj">Object to compare with</param>
        /// <returns>True if equivalently, false otherwise</returns>
        public override bool Equals(object obj)
        {
            Position otherPosition = obj as Position;
            return otherPosition != null && (X == otherPosition.X && Y == otherPosition.Y);
        }


        /// <summary>
        /// Creates a new instance, relative to the <paramref name="origin"/>
        /// </summary>
        /// <param name="origin">The origin point</param>
        /// <param name="relativeX">The offset from <paramref name="origin"/> on the X-axis</param>
        /// <param name="relativeY">The offset from <paramref name="origin"/> on the Y-axis</param>
        /// <returns></returns>
        public static Position Create(Position origin, int relativeX, int relativeY)
        {
            return new Position(origin.X + relativeX, origin.Y + relativeY);
        }
    }
}
