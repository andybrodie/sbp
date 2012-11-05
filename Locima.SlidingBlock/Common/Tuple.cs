namespace Locima.SlidingBlock.Common
{

    /// <summary>
    /// Models a tuple
    /// </summary>
    /// <remarks>
    /// One of those general purpose useful utility classes!</remarks>
    /// <typeparam name="T1">Type of the first element of the tuple</typeparam>
    /// <typeparam name="T2">Type of the second element of the tuple</typeparam>
    public class Tuple<T1, T2>
    {
        /// <summary>
        /// First element of the typle
        /// </summary>
        public T1 First { get; set; }

        /// <summary>
        /// Second element of the tuple
        /// </summary>
        public T2 Second { get; set; }

        /// <summary>
        /// Create a new tuple
        /// </summary>
        /// <param name="first"></param>
        /// <param name="second"></param>
        public Tuple(T1 first, T2 second)
        {
            First = first;
            Second = second;
        }

    }
}
