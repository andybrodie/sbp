using System.Collections.Generic;
using Locima.SlidingBlock.Persistence;

namespace Locima.SlidingBlock.IO
{

    /// <summary>
    /// Player Storage Management interface.  Contains all the methods required to manange the persistence of players
    /// </summary>
    public interface IPlayerStorageManager
    {
        PlayerDetails CurrentPlayer { get; set; }
        List<PlayerDetails> GetAvailablePlayers();
        PlayerDetails Load(string playerId);
        void SavePlayer(PlayerDetails player);
        void Initialise();

        void DeletePlayer(string playerId);

        /// <summary>
        /// Ensure that there is a valid player to be returned by <see cref="CurrentPlayer"/>
        /// </summary>
        /// <remarks>
        /// <para>If code assumes that <see cref="CurrentPlayer"/> is never null, then this method should be used to ensure that there is at least one player defined and they are referenced
        /// in <see cref="CurrentPlayer"/></para>
        /// <para>This method should be called after <see cref="Initialise"/> as this method will set the initial current player</para>
        /// </remarks>
        void EnsureCurrentPlayer();
    }
}