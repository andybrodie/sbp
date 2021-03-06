﻿using System;
using System.Collections.Generic;
using Locima.SlidingBlock.Scrambles;
using NLog;

namespace Locima.SlidingBlock.Common
{
    /// <summary>
    /// Simple bubble sort (a.k.a. swap sort) implementation
    /// </summary>
    /// <remarks>
    /// <para>This is used to check that puzzles can be solved.  See <see cref="ScrambleChecker"/>.</para>
    /// <para>We need our own implementation because we need to know how many swaps were required to sort the list.</para></remarks>
    public class BubbleSorter
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Simple optimised bubble sorter that uses the default <see cref="Comparer{T}"/> implementation for <typeparamref name="T"/>
        /// </summary>
        /// <typeparam name="T">The type of element within <paramref name="list"/></typeparam>
        /// <param name="list">The list to sort</param>
        /// <param name="swapCount">The number of moves that it took to sort the list</param>
        /// <returns>The sorted list</returns>
        /// <see cref="Sort{T}(System.Collections.Generic.IList{T},out int)"/>
        public static IList<T> Sort<T>(IList<T> list, out int swapCount)
        {
            return Sort(list, Comparer<T>.Default, out swapCount);
        }


        /// <summary>
        /// Simple bubble sort implementation that outputs the number of swaps required to sort the list (<paramref name="swapCount"/>
        /// </summary>
        /// <typeparam name="T">The type of element within <paramref name="list"/></typeparam>
        /// <param name="list">The list to sort</param>
        /// <param name="comparer">The function to compare two elements of type <typeparamref name="T"/> within the <paramref name="list"/></param>
        /// <param name="swapCount">The number of moves that it took to sort the list</param>
        /// <returns>The sorted list</returns>
        public static IList<T> Sort<T>(IList<T> list, IComparer<T> comparer, out int swapCount)
        {
            IList<T> workList = new List<T>(list);
            Logger.Info(String.Join(",", workList));
            int swapCounter = 0;
            bool anotherPassRequired = true;
            while (anotherPassRequired)
            {
                anotherPassRequired = false;
                for (int index = 0; index < workList.Count - 1; index++)
                {
                    T first = workList[index];
                    T second = workList[index + 1];
                    if (comparer.Compare(first, second) > 0)
                    {
                        // If the two elements in in the wrong order, swap them
                        workList[index] = second;
                        workList[index + 1] = first;

                        // Keep track of how many swaps its taking to solve the puzzle
                        swapCounter++;
                        /* As we've made a change during this pass we need to make a subsequent pass 
                         * (only when a complete pass is made with no changes are we sure we're sorted, as it were) */
                        anotherPassRequired = true;
                        Logger.Info(String.Join(",", workList));
                    }
                }
                // If we've made a pass through the entire list and made no swaps, then the list is sorted, incomplete will remain false and cause the loop to exit
            }
            swapCount = swapCounter;
            return workList;
        }

    }
}