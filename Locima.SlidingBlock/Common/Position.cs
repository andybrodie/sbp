using System.Runtime.Serialization;

namespace Locima.SlidingBlock.Common
{
    [DataContract]
    public class Position
    {
        public Position()
        {}

        public Position(int x, int y)
        {
            X = x;
            Y = y;
        }

        [DataMember]
        public int X { get; set; }

        [DataMember]
        public int Y { get; set; }

        public override int GetHashCode()
        {
            return X.GetHashCode() ^ Y.GetHashCode();
        }

        public override string ToString()
        {
            return string.Format("{0}({1},{2})", base.ToString(), X, Y);
        }

        public override bool Equals(object obj)
        {
            Position otherPosition = obj as Position;
            return otherPosition != null && (X == otherPosition.X && Y == otherPosition.Y);
        }

        public static Position Create(Position origin, int relativeX, int relativeY)
        {
            return new Position(origin.X + relativeX, origin.Y + relativeY);
        }
    }
}
