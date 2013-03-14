using System;
using System.Text;

namespace Locima.SlidingBlock.Common
{
    /// <summary>
    /// Various tools for creating and converting 1D and 2D arrays
    /// </summary>
    public class ArrayTools
    {
        /// <summary>
        /// Converts an array of type <typeparamref name="TSourceType"/> to a similarly sized array of <typeparamref name="TOutputType"/>
        /// </summary>
        /// <typeparam name="TSourceType">The type of objects in <paramref name="inputArray"/></typeparam>
        /// <typeparam name="TOutputType">The type of objects in the returned array</typeparam>
        /// <param name="inputArray">The input array</param>
        /// <param name="converter">A function to a single element in <paramref name="inputArray"/> to a single element in the output array</param>
        /// <returns>An identically sized array to <paramref name="inputArray"/>, but each element will be of type <typeparamref name="TOutputType"/></returns>
        public static TOutputType[] ConvertTo<TSourceType, TOutputType>(TSourceType[] inputArray,
                                                                        Func<TSourceType, TOutputType> converter)
        {
            if (inputArray == null) return null;
            TOutputType[] outputArray = new TOutputType[(inputArray.Length)];
            for (int i = 0; i < inputArray.Length; i++)
            {
                outputArray[i] = converter(inputArray[i]);
            }
            return outputArray;
        }

        /// <summary>
        /// Converts a 2D array of type <typeparamref name="TSourceType"/> to a similarly sized array of <typeparamref name="TOutputType"/>
        /// </summary>
        /// <typeparam name="TSourceType">The type of objects in <paramref name="inputArray"/></typeparam>
        /// <typeparam name="TOutputType">The type of objects in the returned array</typeparam>
        /// <param name="inputArray">The input array</param>
        /// <param name="converter">A function to a single element in <paramref name="inputArray"/> to a single element in the output array</param>
        /// <returns>An identically sized array to <paramref name="inputArray"/>, but each element will be of type <typeparamref name="TOutputType"/></returns>
        public static TOutputType[][] ConvertTo<TSourceType, TOutputType>(TSourceType[][] inputArray,
                                                                          Func<TSourceType, TOutputType> converter)
        {
            if (inputArray == null) return null;
            TOutputType[][] outputArray = new TOutputType[(inputArray.Length)][];
            for (int i = 0; i < inputArray.Length; i++)
            {
                outputArray[i] = ConvertTo(inputArray[i], converter);
            }
            return outputArray;
        }


        /// <summary>
        /// Delegates to <see cref="Create{T}(int,int,Func{int,int,T})"/> passing null for <c>elementFactory</c> parameter (each array element is null)
        /// </summary>
        public static T[][] Create<T>(int xDim, int yDim) where T : class
        {
            return Create<T>(xDim, yDim, null);
        }

        /// <summary>
        /// Creates a 2D array of type <typeparamref name="T"/> of dimensions <paramref name="xDim"/> and <paramref name="yDim"/> using the <paramref name="elementFactory"/> to
        /// generate an object for each element of the array.
        /// </summary>
        /// <param name="xDim">X dimension of the array</param>
        /// <param name="yDim">Y dimension of the array</param>
        /// <param name="elementFactory">Function to create elements of the array</param>
        /// <typeparam name="T">The type of each element within the array</typeparam>
        /// <returns>An array, never returns null</returns>
        public static T[][] Create<T>(int xDim, int yDim, Func<int, int, T> elementFactory) where T : class
        {
            T[][] array = new T[yDim][];
            for (int y = 0; y < yDim; y++)
            {
                array[y] = new T[xDim];
                if (elementFactory != null)
                {
                    for (int x = 0; x < xDim; x++)
                    {
                        array[y][x] = elementFactory(x, y);
                    }
                }
            }
            return array;
        }


        /// <summary>
        /// Swaps to elements within a 2D array
        /// </summary>
        /// <typeparam name="T">2D array element type</typeparam>
        /// <param name="array">The array to search within</param>
        /// <param name="x1">X co-ordinate of the first element to swap</param>
        /// <param name="y1">Y co-ordinate of the first element to swap </param>
        /// <param name="x2">X co-ordinate of the second element to swap</param>
        /// <param name="y2">Y co-ordinate of the second element to swap</param>
        public static void SwapElements<T>(T[][] array, int x1, int y1, int x2, int y2)
        {
            T swap = array[y1][x1];
            array[y1][x1] = array[y2][x2];
            array[y2][x2] = swap;
        }


        /// <summary>
        /// Outputs a 2D array to the Debug log
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="array"></param>
        public static string ArrayToString<T>(T[][] array)
        {
            StringBuilder sb = new StringBuilder();
            foreach (T[] row in array)
            {
                sb.Append('(');
                sb.Append(String.Join(",", row));
                sb.Append(")\n");
            }
            return sb.ToString();
        }
    }
}