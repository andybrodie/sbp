namespace Locima.SlidingBlock.Common
{
    public class Tuple<T1, T2>
    {
        public T1 First { get; set; }
        public T2 Second { get; set; }

        public Tuple(T1 first, T2 second)
        {
            this.First = first;
            this.Second = second;
        }

    }
}
