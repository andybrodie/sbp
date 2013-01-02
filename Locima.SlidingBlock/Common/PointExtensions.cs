using System.Windows;

namespace Locima.SlidingBlock.Common
{
    /// <summary>
    /// Extensions to the <see cref="Point"/> struct
    /// </summary>
    public static class PointExtensions
    {
        /// <summary>
        ///   Work out the relative position of <paramref name="p1" /> to <paramref name="p2" />
        /// </summary>
        /// <param name="p1"> The point to substract <paramref name="p2" /> from </param>
        /// <param name="p2"> The origin </param>
        /// <returns> </returns>
        public static Point Subtract(this Point p1, Point p2)
        {
            return new Point(p1.X - p2.X, p1.Y - p2.Y);
        }
    }
}