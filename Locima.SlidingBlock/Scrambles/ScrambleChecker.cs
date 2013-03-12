using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        /// <param name="blankTile">The position of the blank tile in the solved puzzle</param>
        public void EnsureSolveable(Position[][] scramble, Position blankTile)
        {
            int tilesAcross = scramble[0].Length;
            int tilesHigh = scramble.Length;
            Parity solvedPuzzleParity = CalculateParity(tilesAcross, tilesHigh, blankTile);
            Logger.Debug("Parity of solved puzzle = {0}", solvedPuzzleParity);

            Parity scrambledPuzzleParity = CalculateParity(scramble, blankTile);
            Logger.Debug("Parity of scrambled puzzle = {0}", scrambledPuzzleParity);
            if (scrambledPuzzleParity == solvedPuzzleParity)
            {
                Logger.Info("Solved puzzle and randomised puzzle are both {0} parity, so the puzzle is solveable",
                            solvedPuzzleParity);
            }
            else
            {
                Logger.Info(
                    "Solved puzzle has parity {0} but scrambled puzzle has parity {1}, so need to correct by swapping two tiles (neither of which can be the blank tile)",
                    solvedPuzzleParity, scrambledPuzzleParity);
                // We know we've got at least 2 rows, so if the blank tile is in the 1st row, go for the 2nd, or vice versa
                int rowToSwap = blankTile.Y == 0 ? 1 : 0;
                ArrayTools.SwapElements(scramble, 0, rowToSwap, 1, rowToSwap);

                // Double check
                scrambledPuzzleParity = CalculateParity(scramble, blankTile);
                Logger.Info("New scrambled puzzle parity is {0}", scrambledPuzzleParity);
                Debug.Assert(scrambledPuzzleParity==solvedPuzzleParity);
            }
        }


        /// <summary>
        /// Creates the parity list for a solved puzzle of the domensions specified
        /// </summary>
        /// <param name="tilesAcross">Number of tiles horizontally in the puzzle</param>
        /// <param name="tilesHigh">Number of tiles vertically in the puzzle</param>
        /// <param name="blankTile">The position of the blank tile in the puzzle</param>
        /// <returns>A list ready for <see cref="CalculateParity(int,int,Locima.SlidingBlock.Common.Position)"/> of</returns>
        private List<int> ConvertToParityList(int tilesAcross, int tilesHigh, Position blankTile)
        {
            List<int> parityList = new List<int>();
            for (int y = 0; y < tilesHigh; y++)
            {
                for (int x = 0; x < tilesAcross; x++)
                {
                    if (blankTile.X == x && blankTile.Y == y) continue; // Skip the blank tile
                    /* Calculate the absolute tile position (i.e. a single integer representing the position of the tile
                     * by iterating over a each row alterately left to right and right to left.  For example, on a 3 x 3 puzzle, the order is:
                     * 0 1 2
                     * 5 4 3
                     * 7 8 9
                     * We can achieve this simply by changing the x-offset of the tile we're after to either be based from the left or the right
                     */
                    int tilePosition = (y%2 == 0) ? 
                        (x + (y*tilesAcross)) :     
                        (((tilesAcross - 1) - x) + (y*tilesAcross));
                    parityList.Add(tilePosition);
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
        /// <param name="blankTile">The position of the blank tile in the solved puzzle</param>
        /// <returns>A list ready to be passed to <see cref="CalculateParity(int,int,Locima.SlidingBlock.Common.Position)"/></returns>
        private IList<int> ConvertToParityList(Position[][] tiles, Position blankTile)
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
                    if (!tile.Equals(blankTile)) // Ignore the blank tile in creating the parity list
                    {
                        parityList.Add((tile.Y*tilesAcross) + tile.X);
                    }
                    else
                    {
                        Logger.Debug("Ignoring tile {0} as it is the blank tile", blankTile);
                    }
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
        /// <param name="tilesAcross">The number of tiles horizontally in the puzzle</param>
        /// <param name="tilesHigh">The number of tiles vertically in the puzzle</param>
        /// <param name="blankTile">The position of the blank tile in the solved puzzle</param>
        /// <returns></returns>
        private Parity CalculateParity(int tilesAcross, int tilesHigh, Position blankTile)
        {
            // If it's not a hard-coded one (this code will only ever be called if we extend the game to support weird and wonderful puzzle shapes
            List<int> parityList = ConvertToParityList(tilesAcross, tilesHigh, blankTile);
            return CalculateParity(parityList);
        }


        /// <summary>
        /// Calculates the parity of any puzzle state by converting it to a list (<see cref="ConvertToParityList(int,int,Locima.SlidingBlock.Common.Position)"/>) and 
        /// calling <see cref="CalculateParity(System.Collections.Generic.IList{int})"/>
        /// </summary>
        /// <param name="tiles">The tiles (typically this would be calculated by a scramble)</param>
        /// <param name="blankTile">The position of the blank tile in the solved puzzle</param>
        /// <returns>The calculated parity of the puzzle</returns>
        private Parity CalculateParity(Position[][] tiles, Position blankTile)
        {
            return CalculateParity(ConvertToParityList(tiles, blankTile));
        }


        /// <summary>
        /// Calculates the parity of a list representation of a puzzle
        /// </summary>
        /// <remarks>
        /// The parity of the puzzle can be identified by performing a bubble sort (swap sort) on the list.  The number of moves it takes 
        /// to sort the list in ascending order (i.e. the solved position) is the parity.  For a puzzle to be solveable the solved puzzle and scrambled
        /// puzzle must have the same parity.</remarks>
        /// <param name="list">A puzzle grid, flattened by <see cref="ConvertToParityList(Locima.SlidingBlock.Common.Position[][],Locima.SlidingBlock.Common.Position)"/></param>
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
            /// <summary>
            /// Even parity means that it takes an even number of swaps in a bubble sort to solve the puzzle
            /// </summary>
            Even,

            /// <summary>
            /// Odd parity means that it takes an even number of swaps in a bubble sort to solve the puzzle
            /// </summary>
            Odd
        }
    }
}