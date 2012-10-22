using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;
using Locima.SlidingBlock.Common;
using Locima.SlidingBlock.IO;
using Locima.SlidingBlock.Model;

namespace Locima.SlidingBlock.Persistence
{
    [DataContract]
    public class SaveGame : IPersistedObject
    {
        public static readonly int ThumbnailWidth = 64;
        public static readonly int ThumbnailHeight = 64;

        //private ExtensionDataObject ExtensionData { get; set; }

        #region GameTypes enum

        public enum GameTypes
        {
            SinglePlayer,
            Multiplayer
        }

        #endregion

        [DataMember]
        public List<LevelState> Levels { get; set; }

        [DataMember]
        public int CurrentLevelIndex { get; set; }

        public LevelState CurrentLevel
        {
            get
            {
                if (Levels == null)
                {
                    throw new InvalidStateException("No levels defined");
                }
                if (CurrentLevelIndex >= Levels.Count)
                {
                    throw new InvalidStateException(string.Format("CurrentLevelIndex={0} but Levels.Count={1}",
                                                                  CurrentLevelIndex, Levels.Count));
                }
                return Levels[CurrentLevelIndex];
            }
        }

        [DataMember]
        public PlayerLink LocalPlayer { get; set; }

        public PlayerDetails LocalPlayerDetails
        {
            get
            {
                Debug.Assert(LocalPlayer.PlayerDetailsId.Equals(PlayerStorageManager.Instance.CurrentPlayer.Id));
                return PlayerStorageManager.Instance.CurrentPlayer;
            }
        }


        /// <summary>
        ///   Dynamically sum up the total number of moves the player has made in the whole game
        /// </summary>
        public int TotalMoves
        {
            get { return (Levels != null) ? Levels.Sum(level => level.MoveCount) : 0; }
        }


        /// <summary>
        ///   Dynamically sum up the total amount of time the player has been solving puzzles for
        /// </summary>
        public TimeSpan TotalTime
        {
            get { return new TimeSpan((Levels != null) ? Levels.Sum(level => level.ElapsedTime.Ticks) : 0); }
        }


        public string Name
        {
            get { return string.Format("level {0} {1}", LocalPlayerDetails.Name, DateTime.Now.ToString("dd/MM HH:mm")); }
        }

        [DataMember]
        public GameTypes GameType { get; set; }

        #region IPersistedObject Members

        /// <summary>
        ///   When the game was saved
        /// </summary>
        /// <remarks>
        ///   This is not in the data contract as it's metadata on the file in isolated storage. It's set by the <see
        ///    cref="SaveGameStorageManager" /> on load or save
        /// </remarks>
        public DateTimeOffset LastUpdate { get; set; }

        /// <summary>
        ///   The name of this filename in isolated storage
        /// </summary>
        /// <remarks>
        ///   This is not in the data contract because it's persisted as the filename in isolated storage. It's set by the <see
        ///    cref="SaveGameStorageManager" /> on load or save
        /// </remarks>
        public string Id { get; set; }

        /// <summary>
        /// Returns true if the game has been completed
        /// </summary>
        public bool IsCompletedGame
        {
            get { return CurrentLevelIndex == Levels.Count; }
        }

        #endregion

        /// <summary>
        ///   Ensure that the <see cref="Levels" /> member is not null and contains at least <paramref name="levelCount" /> levels within it.
        /// </summary>
        public void EnsureLevels(int levelCount)
        {
            if (Levels == null)
            {
                Levels = new List<LevelState>(levelCount);
            }
            for (int i = Levels.Count; i < levelCount; i++)
            {
                Levels.Add(new LevelState());
            }
        }
    }
}