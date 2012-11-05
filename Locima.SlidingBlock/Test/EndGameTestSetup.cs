using System;
using Locima.SlidingBlock.IO;
using Locima.SlidingBlock.Persistence;
using Locima.SlidingBlock.SinglePlayer;

namespace Locima.SlidingBlock.Test
{

    /// <summary>
    /// Sets up some simple test data for testing the <see cref="GameEnd"/> screen.
    /// </summary>
    public class EndGameTestSetup
    {

        /// <summary>
        /// Create a <see cref="SaveGame"/> with some level completion data to try out the display of stats.
        /// </summary>
        /// <returns></returns>
        public static Uri TestEndGameScreen()
        {
            SaveGame sg = GameFactory.CreateSinglePlayerGame(3, 3);
            sg.Levels[0].MoveCount = 100;
            sg.Levels[0].ElapsedTime = new TimeSpan(0, 0, 10, 00);

            sg.Levels[1].MoveCount = 22;
            sg.Levels[1].ElapsedTime = new TimeSpan(0, 0, 00, 40);

            sg.Levels[2].MoveCount = 888888;
            sg.Levels[2].ElapsedTime = new TimeSpan(1, 2, 3, 4);

            sg.CurrentLevelIndex = 2;
            SaveGameStorageManager.Instance.SaveGame(sg);

            return GameEnd.CreateNavigationUri(sg.Id);
        }
    }
}