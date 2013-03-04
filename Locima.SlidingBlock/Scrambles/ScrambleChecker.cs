using System;
using System.Collections.Generic;
using Locima.SlidingBlock.Common;
using NLog;

namespace Locima.SlidingBlock.Scrambles
{

    /// <summary>
    /// Ensures that any scramble generated is solveable, based on the parity algorithm by Jim Loy: <a href="http://www.jimloy.com/puzz/15.htm">http://www.jimloy.com/puzz/15.htm</a>
    /// </summary>
    public class ScrambleChecker
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public static ScrambleChecker Instance { get; private set; }

        static ScrambleChecker()
        {
            Instance = new ScrambleChecker();
        }

        /// <summary>
        /// Ensures that a scramble is solveable by fixing it if it's not
        /// </summary>
        /// <param name="scramble">The scrambled puzzle</param>
        public void EnsureSolveable(Position[][] scramble)
        {
            int tilesAcross = scramble[0].Length;
            int tilesHigh = scramble.Length;
            Parity solvedPuzzleParity = CalculateParity(tilesAcross, tilesHigh);
            Logger.Info("Parity of solved puzzle = {0}", solvedPuzzleParity);
            Parity scrambledPuzzleParity = CalculateParity(scramble);
            if (scrambledPuzzleParity == solvedPuzzleParity)
            {
                Logger.Info("Solved puzzle and randomised puzzle have same parity {0}, so the puzzle is solveable",
                            solvedPuzzleParity);
            }
            else
            {
                Logger.Info("Solved puzzle has parity {0} but scrambled puzzle has parity {1}, so need to correct by swapping first two elements", solvedPuzzleParity, scrambledPuzzleParity);
                ArrayTools.SwapElements(scramble, 0, 0, 1, 0);

                // Double check
                Logger.Info("New parity is {0}", CalculateParity(scramble));
            }
        }


        #region Parity calculators and support methods

        private List<int> ConvertToParityList(int tilesAcross, int tilesHigh)
        {
            List<int> parityList = new List<int>();
            for (int y = 0; y < tilesHigh; y++)
            {
                for (int x = 0; x < tilesAcross; x++)
                {
                    int number = (y % 2 == 0) ? (x + (y * tilesAcross)) : (((tilesAcross - 1) - x) + (y * tilesAcross));
                    parityList.Add(number);
                }
            }
            if (Logger.IsDebugEnabled)
            {
                Logger.Debug("Parity list for solved {0} x {1} = {2}", tilesAcross, tilesHigh,
                             String.Join(", ", parityList));
            }
            return parityList;
        }


        private IList<int> ConvertToParityList(Position[][] tiles)
        {
            int tilesHigh = tiles.Length;
            int tilesAcross = tiles[0].Length;
            List<int> parityList = new List<int>();
            for (int y = 0; y < tilesHigh; y++)
            {
                for (int x = 0; x < tilesAcross; x++)
                {
                    int xOffset = (y % 2 == 0) ? x : (tilesAcross - 1) - x;
                    Position tile = tiles[y][xOffset];
                    parityList.Add(tile.Y + (tile.X * tilesAcross));
                }
            }
            if (Logger.IsDebugEnabled)
            {
                Logger.Debug("Parity list for tiles {0} x {1} = {2}", tilesAcross, tilesHigh,
                             String.Join(", ", parityList));
            }
            return parityList;
        }


        private Parity CalculateParity(int tilesAcross, int tilesHigh)
        {
            if (tilesAcross == tilesHigh)
            {
                switch (tilesAcross)
                {
                    case 3:
                        return Parity.Odd;
                    case 4:
                        return Parity.Even;
                    case 5:
                        return Parity.Even;
                }
            }
            return CalculateParity(ConvertToParityList(tilesAcross, tilesHigh));
        }


        private Parity CalculateParity(Position[][] tiles)
        {
            return CalculateParity(ConvertToParityList(tiles));
        }


        private Parity CalculateParity(IList<int> list)
        {
            int movesRequiredToSort;
            BubbleSort(list, out movesRequiredToSort);
            Parity parity = movesRequiredToSort % 2 == 0 ? Parity.Even : Parity.Odd;
            Logger.Debug("Required {0} moves to sort, so parity is {1}", movesRequiredToSort, parity);
            return parity;
        }

        private enum Parity
        {
            Even,
            Odd
        }

        #endregion

        #region Simple bubble sort implementation

        /// <summary>
        /// Simple optimised bubble sorter
        /// </summary>
        /// <remarks>
        /// We need our own implementation because we need to know how many swaps occurred</remarks>
        /// <typeparam name="T">The type of element within <paramref name="list"/></typeparam>
        /// <param name="list">The list to sort</param>
        /// <param name="moves">The number of moves that it took to sort the list</param>
        /// <returns>The sorted list</returns>
        /// <see cref="BubbleSort{T}(System.Collections.Generic.IList{T},out int)"/>
        public IList<T> BubbleSort<T>(IList<T> list, out int moves)
        {
            return BubbleSort(list, Comparer<T>.Default, out moves);
        }


        /// <summary>
        /// Simple optimised bubble sorter
        /// </summary>
        /// <remarks>
        /// We need our own implementation because we need to know how many swaps occurred</remarks>
        /// <typeparam name="T">The type of element within <paramref name="list"/></typeparam>
        /// <param name="list">The list to sort</param>
        /// <param name="comparer">The function to compare two elements of type <typeparamref name="T"/> within the <paramref name="list"/></param>
        /// <param name="moves">The number of moves that it took to sort the list</param>
        /// <returns>The sorted list</returns>
        public IList<T> BubbleSort<T>(IList<T> list, IComparer<T> comparer, out int moves)
        {
            IList<T> workList = new List<T>(list);
            int moveCount = 0;
            bool stillGoing = true;
            while (stillGoing)
            {
                stillGoing = false;
                for (int i = 0; i < workList.Count - 1; i++)
                {
                    T x = workList[i];
                    T y = workList[i + 1];
                    if (comparer.Compare(x, y) > 0)
                    {
                        workList[i] = y;
                        workList[i + 1] = x;
                        moveCount++;
                        stillGoing = true;
                    }
                }
            }
            moves = moveCount;
            return workList;
        }

        #endregion
    }
}