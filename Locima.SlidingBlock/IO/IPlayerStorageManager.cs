using System.Collections.Generic;
using Locima.SlidingBlock.Persistence;

namespace Locima.SlidingBlock.IO
{

    /// <summary>
    /// Player Storage Management interface.  Contains all the methods required to manange the persistence of players.
    /// </summary>
    public interface IPlayerStorageManager : IStorageManager
    {
        /// <summary>
        /// Retrieve the current (i.e. active, or last active) player.  May return null if no players exist.
        /// </summary>
        PlayerDetails CurrentPlayer { get; set; }

        /// <summary>
        /// Retrieves a list of available players, ordered by <see cref="PlayerDetails.LastUpdate"/>
        /// </summary>
        /// <returns>Ordered list of players, never returns null, but may return an empty list</returns>
        List<PlayerDetails> GetAvailablePlayers();

        /// <summary>
        /// Load a player using their uniquely-assigned ID (assigned by <see cref="SavePlayer"/>)
        /// </summary>
        /// <param name="playerId"></param>
        /// <returns></returns>
        PlayerDetails Load(string playerId);

        /// <summary>
        /// Saves an existing player or new player
        /// </summary>
        void SavePlayer(PlayerDetails player);
        
        /// <summary>
        /// Deletes a player's record permanently
        /// </summary>
        /// <remarks>
        /// This method must also ensure that any other data (e.g. <see cref="SaveGame"/> persisted objects are also tidied up)</remarks>
        /// <param name="playerId"></param>
        void DeletePlayer(string playerId);

    }
}