using Locima.SlidingBlock.Persistence;

namespace Locima.SlidingBlock.IO
{

    /// <summary>
    /// Interface for managing the saving and loading of the high score table
    /// </summary>
    public interface IHighScoresStorageManager : IStorageManager
    {

        /// <summary>
        /// Load the high score table from storage
        /// </summary>
        /// <returns></returns>
        HighScoreTable Load();

        /// <summary>
        /// Save the high score table to storage
        /// </summary>
        /// <param name="highScoreTable">The high score table to save, either created manually or from <see cref="Load"/></param>
        void Save(HighScoreTable highScoreTable);
    }
}