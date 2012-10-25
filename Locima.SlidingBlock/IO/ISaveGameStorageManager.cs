using System.Collections.Generic;
using Locima.SlidingBlock.Persistence;

namespace Locima.SlidingBlock.IO
{

    /// <summary>
    /// Interface for managing the saving and loading of <see cref="SaveGame"/> instances.
    /// </summary>
    public interface ISaveGameStorageManager : IStorageManager
    {
        /// <summary>
        /// Saves either a new game or updates an existing game in storage
        /// </summary>
        /// <param name="saveGame"></param>
        void SaveGame(SaveGame saveGame);
        
        /// <summary>
        /// Loads a game based on an ID assigned by <see cref="SaveGame"/>
        /// </summary>
        SaveGame Load(string saveGameId);
        
        /// <summary>
        /// Permanently deletes the passed game from storage
        /// </summary>
        void DeleteGame(SaveGame saveGame);
        
        /// <summary>
        /// Permanently deletes the passed game from storage
        /// </summary>
        void DeleteGame(string saveGameId);

        /// <summary>
        /// Permanently deletes all the games associated with a specific player (based on their ID)
        /// </summary>
        void DeleteGames(string playerId);

        /// <summary>
        /// Loads all games from storage
        /// </summary>
        /// <remarks>This method should only be used with caution in an asynchronous loading thread, if there are lots of <see cref="SaveGame"/> instances to load
        /// this method could take considerable time to complete as well as use up considerable amounts of internal memory.</remarks>
        /// <returns>An unordered list of games.  May return an empty enumeration but never returns null.</returns>
        IEnumerable<SaveGame> LoadGames();


        /// <summary>
        /// Loads all games from storage that belong to <paramref name="playerId"/>
        /// </summary>
        /// <remarks>This method should only be used with caution in an asynchronous loading thread, if there are lots of <see cref="SaveGame"/> instances to load
        /// this method could take considerable time to complete as well as use up considerable amounts of internal memory.</remarks>
        /// <returns>An unordered list of games.  May return an empty enumeration but never returns null.</returns>
        IEnumerable<SaveGame> LoadGames(string playerId);

        /// <summary>
        /// Returns the most recently saved <see cref="SaveGame"/> instance that belongs to <paramref name="playerId"/>
        /// </summary>
        /// <returns>May return null if the <paramref name="playerId"/> has no save games yet (e.g. is a new player)</returns>
        SaveGame GetContinuableGame(string playerId);
    }
}