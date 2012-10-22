using Locima.SlidingBlock.Persistence;

namespace Locima.SlidingBlock.IO
{
    public interface IHighscoresStorageManager
    {
        void Initialise();
        HighScoreTable Load();
        void Save(HighScoreTable highScoreTable);
    }
}