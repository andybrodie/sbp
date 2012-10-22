using System.Collections.Generic;
using Locima.SlidingBlock.Persistence;

namespace Locima.SlidingBlock.IO
{
    public interface ISaveGameStorageManager
    {
        void Initialise();
        
        void SaveGame(SaveGame saveGame);
        
        SaveGame Load(string saveGameId);
        
        void DeleteGame(SaveGame saveGame);
        void DeleteGame(string saveGameId);
        void DeleteGames(string playerId);

        IEnumerable<SaveGame> LoadGames();
        
        IEnumerable<SaveGame> LoadGames(string playerId);
        SaveGame GetContinuableGame();
    }
}