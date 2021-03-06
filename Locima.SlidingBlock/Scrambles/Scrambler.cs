﻿using System;
using System.Collections.Generic;
using Locima.SlidingBlock.Common;
using Locima.SlidingBlock.Model;
using NLog;

namespace Locima.SlidingBlock.Scrambles
{
    /// <summary>
    ///   The scrambler is responsible for messing up the tiles before a level starts.  This class implements several options for scrambles which can then be re-used on different levels
    /// </summary>
    /// <remarks>
    ///   The definition of a scramble is a two-dimension array of tile positions.  Each index in the array is the position of the tile on the screen (i.e. Position[3][5] is the tile on the screen
    ///   that is in the 3rd column and 5th row.  The value of each array position is a <see cref="Position" /> object which contains where the tile must be to be considered in the "solved" position.
    ///   E.g. if Position[3][5] contains <see cref="Position" /> with <see cref="Position.X">X</see> = 2 and <see
    ///    cref="Position.Y">Y</see> = 7 then the tile will start at 3,5 but must be moved
    ///   to 2,7 to be solved.
    /// </remarks>
    public class Scrambler
    {

        #region ScrambleType enum

        /// <summary>
        /// The difference types of scramble (see <see cref="Scrambler"/>)
        /// </summary>
        public enum ScrambleType
        {
            /// <summary>
            ///   Randomised scramble
            /// </summary>
            Random,

            /// <summary>
            ///   Flip all the tiles along a central X axis
            /// </summary>
            XFlip,

            /// <summary>
            ///   Flip all the tiles along a central Y axis
            /// </summary>
            YFlip,

            /// <summary>
            ///   Flips all tiles along a central X and Y axis
            /// </summary>
            XyFlip,

            /// <summary>
            ///   Do not scramble at all
            /// </summary>
            Identity,

            /// <summary>
            /// Only swaps the tiles at 0,0 and 1,0, allowing the puzzle to be completed in a single move  
            /// </summary>
            /// <remarks>
            /// Used for testing
            /// </remarks>
            OneMoveToFinish,

            /// <summary>
            /// Starts with a solved puzzle and moves the tiles a configurable number of times
            /// </summary>
            Shuffle,
        }

        #endregion

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// The singleton instance
        /// </summary>
        public static Scrambler Instance { get; private set; }

        static Scrambler()
        {
            Instance = new Scrambler();
        }

        /// <summary>
        /// Apply the <paramref name="type"/> scramble to a new position grid
        /// </summary>
        /// <param name="type">The type of scramble to apply</param>
        /// <param name="tilesAcross">Horizontal dimension of the puzzle</param>
        /// <param name="tilesHigh">Vertical dimension of the puzzle</param>
        /// <param name="blankTile">The position that the blank tile should be in the solved puzzle</param>
        /// <returns>A grid of solved positions for each each tile</returns>
        public Position[][] Scramble(ScrambleType type, int tilesAcross, int tilesHigh, Position blankTile)
        {
            Position[][] scramble;
            Logger.Info("Creating scramble {0} for a grid {1} x {2} with blank tile at {3},{4}", type, tilesAcross, tilesHigh, blankTile.X, blankTile.Y);
            switch (type)
            {
                case ScrambleType.XFlip:
                    scramble = XFlipScramble(tilesAcross, tilesHigh);
                    break;
                case ScrambleType.YFlip:
                    scramble = YFlipScramble(tilesAcross, tilesHigh);
                    break;
                case ScrambleType.XyFlip:
                    scramble = XYFlipScramble(tilesAcross, tilesHigh);
                    break;
                case ScrambleType.Identity:
                    scramble = IdentityScramble(tilesAcross, tilesHigh);
                    break;
                case ScrambleType.Random:
                    scramble = RandomScramble(tilesAcross, tilesHigh);
                    break;
                    case ScrambleType.OneMoveToFinish:
                    scramble = IdentityScramble(tilesAcross, tilesHigh);
                    int swapTile = blankTile.X + ((blankTile.X == tilesAcross - 1) ? -1 : 1);
                    ArrayTools.SwapElements(scramble, blankTile.X, blankTile.Y, swapTile, blankTile.Y); // BUG This is sawpping the wrong tile
                    break;
                case ScrambleType.Shuffle:
                    scramble = ShuffleScramble(tilesAcross, tilesHigh, 100); // Picked an arbitrary number of 100 moves, TODO make this link to difficulty or something!
                    break;
                default:
                    Logger.Error("Unknown ScrambleType passed: {0}.  Returning random scramble.", type);
                    scramble = RandomScramble(tilesAcross, tilesHigh);
                    break;
            }

            // There's a 50/50 chance that any scramble is unsolveable, this method call ensures that the puzzle is solveable.
            ScrambleChecker.Instance.EnsureSolveable(scramble, blankTile);
            return scramble;
        }

        /// <summary>
        /// Create a puzzle grid <paramref name="tilesAcross"/> wide and <paramref name="tilesHigh"/> using <paramref name="customScrambler"/> to decide the solved position of each tile.
        /// </summary>
        /// <param name="customScrambler">A customised scrambler function that, given the x and y co-ordinate of a tile, returns the position the tile should be in to be in the solved position</param>
        /// <param name="tilesAcross">The number of tiles across in the puzzle</param>
        /// <param name="tilesHigh">The number of tiles high in the puzzle</param>
        /// <returns>A 2D array of <see cref="Position"/> instances.  Each <see cref="Position"/> object contains the solved position of the tile, it's position within the array determines
        /// where the tile currently is in the puzzle</returns>
        private Position[][] Scramble(Func<int, int, Position> customScrambler, int tilesAcross, int tilesHigh)
        {
            return ArrayTools.Create(tilesAcross, tilesHigh, customScrambler);
        }

        #region Scramblers
        
        /// <summary>
        /// Flips the tiles along an imaginary Y-axis down the middle of the puzzle
        /// </summary>
        /// <param name="tilesAcross">The number of tiles across in the puzzle</param>
        /// <param name="tilesHigh">The number of tiles high in the puzzle</param>
        /// <returns>A 2D array of <see cref="Position"/> instances.  Each <see cref="Position"/> object contains the solved position of the tile, it's position within the array determines
        /// where the tile currently is in the puzzle</returns>
        private Position[][] XFlipScramble(int tilesAcross, int tilesHigh)
        {
            return ArrayTools.Create(tilesAcross, tilesHigh,
                                     (x, y) => new Position(x, (tilesHigh - 1) - y));
        }

        /// <summary>
        /// Flips the tiles along an imaginary X-axis across the middle of the puzzle
        /// </summary>
        /// <param name="tilesAcross">The number of tiles across in the puzzle</param>
        /// <param name="tilesHigh">The number of tiles high in the puzzle</param>
        /// <returns>A 2D array of <see cref="Position"/> instances.  Each <see cref="Position"/> object contains the solved position of the tile, it's position within the array determines
        /// where the tile currently is in the puzzle</returns>
        private Position[][] YFlipScramble(int tilesAcross, int tilesHigh)
        {
            return ArrayTools.Create(tilesAcross, tilesHigh,
                                     (x, y) => new Position((tilesAcross - 1) - x, y));
        }

        /// <summary>
        /// Combines <see cref="XFlipScramble"/> and <see cref="YFlipScramble"/> to flip the tiles along both axes
        /// </summary>
        /// <param name="tilesAcross">The number of tiles across in the puzzle</param>
        /// <param name="tilesHigh">The number of tiles high in the puzzle</param>
        /// <returns>A 2D array of <see cref="Position"/> instances.  Each <see cref="Position"/> object contains the solved position of the tile, it's position within the array determines
        /// where the tile currently is in the puzzle</returns>
        private Position[][] XYFlipScramble(int tilesAcross, int tilesHigh)
        {
            return ArrayTools.Create(tilesAcross, tilesHigh,
                                     (x, y) => new Position((tilesAcross - 1) - x, (tilesHigh - 1) - y));
        }


        /// <summary>
        /// Performs no translation of tiles, i.e. each tile is in the solved position
        /// </summary>
        /// <param name="tilesAcross">The number of tiles across in the puzzle</param>
        /// <param name="tilesHigh">The number of tiles high in the puzzle</param>
        /// <returns>A 2D array of <see cref="Position"/> instances.  Each <see cref="Position"/> object contains the solved position of the tile, it's position within the array determines
        /// where the tile currently is in the puzzle</returns>
        private Position[][] IdentityScramble(int tilesAcross, int tilesHigh)
        {
            return ArrayTools.Create(tilesAcross, tilesHigh, (x, y) => new Position(x, y));
        }

        /// <summary>
        ///  Creates a random scrambled set of tiles
        /// </summary>
        /// <param name="tilesAcross">The number of tiles across in the puzzle</param>
        /// <param name="tilesHigh">The number of tiles high in the puzzle</param>
        /// <returns>A 2D array of <see cref="Position"/> instances.  Each <see cref="Position"/> object contains the solved position of the tile, it's position within the array determines
        /// where the tile currently is in the puzzle</returns>
        private Position[][] RandomScramble(int tilesAcross, int tilesHigh)
        {
            int totalTiles = tilesAcross*tilesHigh;
            List<Position> allTiles = new List<Position>(totalTiles);
            for (int y = 0; y < tilesHigh; y++)
            {
                for (int x = 0; x < tilesAcross; x++)
                {
                    allTiles.Add(new Position(x, y));
                }
            }

            Random r = new Random();

            Position[][] solvedPosition = ArrayTools.Create<Position>(tilesAcross, tilesHigh);

            for (int y = 0; y < tilesHigh; y++)
            {
                for (int x = 0; x < tilesAcross; x++)
                {
                    int tileToInsert = r.Next(allTiles.Count);
                    solvedPosition[y][x] = allTiles[tileToInsert];
                    allTiles.RemoveAt(tileToInsert);
                }
            }

            return solvedPosition;
        }


        /// <summary>
        /// Creates a scramble randomly based on moving tiles
        /// </summary>
        /// <remarks>
        /// This allows us to create different scrambles which maintain a known number of moves to finish.  Whilst the implementation of this method may appear inefficient, it seems to work
        /// fine for 100 moves or so (takes less than a second)</remarks>
        /// <param name="tilesAcross">The number of tiles across in the puzzle</param>
        /// <param name="tilesHigh">The number of tiles high in the puzzle</param>
        /// <param name="totalMoves">The number of moves to apply to the initial (solved) grid</param>
        /// <returns>A 2D array of <see cref="Position"/> instances.  Each <see cref="Position"/> object contains the solved position of the tile, it's position within the array determines
        /// where the tile currently is in the puzzle</returns>
        private Position[][] ShuffleScramble(int tilesAcross, int tilesHigh, int totalMoves)
        {
            Random random = new Random(); // Default random constructor is good enough for our purposes
            Position[][] solvedPosition = ArrayTools.Create(tilesAcross, tilesHigh, (x,y) => new Position(x,y));
            
            Position blankTile = new Position(0,0);
            int moveCount = 0;
            List<TileDirection> allowedMoves = new List<TileDirection>();
            DateTime startTime = DateTime.Now;
            while (moveCount < totalMoves)
            {
                allowedMoves.Clear();
                if (blankTile.X > 0) allowedMoves.Add(TileDirection.FromLeft);
                if (blankTile.X < tilesAcross-1) allowedMoves.Add(TileDirection.FromRight);
                if (blankTile.Y > 0) allowedMoves.Add(TileDirection.FromAbove);
                if (blankTile.Y < tilesHigh - 1) allowedMoves.Add(TileDirection.FromBelow);
                switch (allowedMoves[random.Next(allowedMoves.Count)])
                {
                    case TileDirection.FromLeft:
                        ArrayTools.SwapElements(solvedPosition, blankTile.X, blankTile.Y, blankTile.X - 1, blankTile.Y);
                        blankTile.X--;
                        break;
                    case TileDirection.FromRight:
                        ArrayTools.SwapElements(solvedPosition, blankTile.X, blankTile.Y, blankTile.X + 1, blankTile.Y);
                        blankTile.X++;
                        break;
                    case TileDirection.FromAbove:
                        ArrayTools.SwapElements(solvedPosition, blankTile.X, blankTile.Y, blankTile.X, blankTile.Y - 1);
                        blankTile.Y--;
                        break;
                    case TileDirection.FromBelow:
                        ArrayTools.SwapElements(solvedPosition, blankTile.X, blankTile.Y, blankTile.X, blankTile.Y + 1);
                        blankTile.Y++;
                        break;
                }
                moveCount++;
            }
            TimeSpan timeTaken = DateTime.Now - startTime;
            Logger.Info("Took {0} to apply {1} moves to a {2} x {3} grid", timeTaken, totalMoves, tilesAcross, tilesHigh);
            return solvedPosition;
        }


        #endregion
    }


}