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


        /// <summary>
        /// Create the singleton <see cref="Instance" />
        /// </summary>
        static ScrambleChecker()
        {
            Instance = new ScrambleChecker();
        }

        
        /// <summary>
        /// Single instance initialised in static initialiser
        /// </summary>
        public static ScrambleChecker Instance { get; private set; }


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
                Logger.Info(
                    "Solved puzzle has parity {0} but scrambled puzzle has parity {1}, so need to correct by swapping first two elements",
                    solvedPuzzleParity, scrambledPuzzleParity);
                ArrayTools.SwapElements(scramble, 0, 0, 1, 0);

                // Double check
                Logger.Info("New parity is {0}", CalculateParity(scramble));
            }
        }

        
        /// <summary>
        /// Creates the parity list for a solved puzzle of the domensions specified
        /// </summary>
        /// <param name="tilesAcross">Number of tiles horizontally in the puzzle</param>
        /// <param name="tilesHigh">Number of tiles vertically in the puzzle</param>
        /// <returns>A list ready for <see cref="CalculateParity(int,int)"/> of</returns>
        private List<int> ConvertToParityList(int tilesAcross, int tilesHigh)
        {
            List<int> parityList = new List<int>();
            for (int y = 0; y < tilesHigh; y++)
            {
                for (int x = 0; x < tilesAcross; x++)
                {
                    int number = (y%2 == 0) ? (x + (y*tilesAcross)) : (((tilesAcross - 1) - x) + (y*tilesAcross));
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


        /// <summary>
        /// Creates a parity list from the puzzle positions passed
        /// </summary>
        /// <param name="tiles">The representation of any puzzle</param>
        /// <returns>A list ready to be passed to <see cref="CalculateParity(int,int)"/></returns>
        private IList<int> ConvertToParityList(Position[][] tiles)
        {
            int tilesHigh = tiles.Length;
            int tilesAcross = tiles[0].Length;
            List<int> parityList = new List<int>();
            for (int y = 0; y < tilesHigh; y++)
            {
                for (int x = 0; x < tilesAcross; x++)
                {
                    int xOffset = (y%2 == 0) ? x : (tilesAcross - 1) - x;
                    Position tile = tiles[y][xOffset];
                    parityList.Add((tile.Y * tilesAcross) + tile.X);

                }
            }
            if (Logger.IsDebugEnabled)
            {
                Logger.Debug("Parity list for tiles {0} x {1} = {2}", tilesAcross, tilesHigh,
                             String.Join(", ", parityList));
            }
            return parityList;
        }


        /// <summary>
        /// Calculates the parity of a solved puzzle of size <paramref name="tilesAcross"/> and <paramref name="tilesHigh"/>
        /// </summary>
        /// <remarks>
        /// The parity of the built-in puzzles: 3x3, 4x4 and 5x5 is hard-coded to save time, but this method remains generic and can handle and size</remarks>
        /// <param name="tilesAcross">The number of tiles horizontally in the puzzle</param>
        /// <param name="tilesHigh">The number of tiles vertically in the puzzle</param>
        /// <returns></returns>
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
            List<int> parityList = ConvertToParityList(tilesAcross, tilesHigh);
            return CalculateParity(parityList);
        }


        /// <summary>
        /// Calculates the parity of any puzzle state by converting it to a list (<see cref="ConvertToParityList(int,int)"/>) and 
        /// calling <see cref="CalculateParity(System.Collections.Generic.IList{int})"/>
        /// </summary>
        /// <param name="tiles">The tiles (typically this would be calculated by a scramble)</param>
        /// <returns></returns>
        private Parity CalculateParity(Position[][] tiles)
        {
            return CalculateParity(ConvertToParityList(tiles));
        }


        /// <summary>
        /// Calculates the parity of a list representation of a puzzle
        /// </summary>
        /// <remarks>
        /// The parity of the puzzle can be identified by performing a bubble sort (swap sort) on the list.  The number of moves it takes 
        /// to sort the list in ascending order (i.e. the solved position) is the parity.  For a puzzle to be solveable the solved puzzle and scrambled
        /// puzzle must have the same parity.</remarks>
        /// <param name="list">A puzzle grid, flattened by <see cref="ConvertToParityList(Position[][])"/></param>
        /// <returns>Whether the puzzle is even parity or odd parity.</returns>
        public Parity CalculateParity(IList<int> list)
        {
            int movesRequiredToSort;
            BubbleSorter.Sort(list, out movesRequiredToSort);
            Parity parity = movesRequiredToSort%2 == 0 ? Parity.Even : Parity.Odd;
            Logger.Debug("Required {0} moves to sort, so parity is {1}", movesRequiredToSort, parity);
            return parity;
        }


        /// <summary>
        /// Used to represent the parity of a puzzle
        /// </summary>
        public enum Parity
        {
            Even,
            Odd
        }
    }
}