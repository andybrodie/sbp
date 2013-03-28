using System;
using System.Collections.Generic;
using System.IO;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Runtime.Serialization;
using System.Windows;
using System.Windows.Media;
using Locima.SlidingBlock.Common;
using Locima.SlidingBlock.Persistence;
using NLog;

namespace Locima.SlidingBlock.IO.IsolatedStorage
{
    /// <summary>
    /// Manages the persistence of players in isolated storage
    /// </summary>
    public class PlayerIsolatedStorageManager : IPlayerStorageManager
    {
        private const string PlayerProfileDirectory = "PlayerProfiles";

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        #region IPlayerStorageManager Members

        /// <summary>
        /// The current player using the app
        /// </summary>
        public PlayerDetails CurrentPlayer { get; set; }

        /// <inheritdoc/>
        public List<PlayerDetails> GetAvailablePlayers()
        {
            using (IsolatedStorageFile store = IsolatedStorageFile.GetUserStoreForApplication())
            {
                List<string> playerFiles = IOHelper.GetFileNames(PlayerProfileDirectory, store);
                List<PlayerDetails> players = new List<PlayerDetails>(playerFiles.Count);
                players.AddRange(playerFiles.Select(fileName => Load(fileName, store)));
                return players;
            }
        }


        /// <inheritdoc/>
        public void SavePlayer(PlayerDetails player)
        {
            if (player == null) throw new ArgumentException("puzzle");
            if (string.IsNullOrEmpty(player.Id))
            {
                player.Id = string.Format("{0}{1}{2}.player", PlayerProfileDirectory, IOHelper.PathSeparator,
                                          Guid.NewGuid());
                Logger.Info("Saving new player {0}", player);
            }
            else
            {
                Logger.Info("Saving existing player {0}", player);
            }
            using (IsolatedStorageFile store = IsolatedStorageFile.GetUserStoreForApplication())
            {
                IOHelper.EnsureDirectory(PlayerProfileDirectory, store);
                IOHelper.SaveObject(player, store);
            }
            if (CurrentPlayer != null && CurrentPlayer.Id == player.Id)
            {
                Logger.Debug("Updating CurrentPlayer with updated player passed");
                CurrentPlayer = player;
            }
        }


        /// <summary>
        ///   Ensure that the <see cref="PlayerProfileDirectory" /> directory exists.  If the directory has to be created then a default player is created so that
        ///   there is never "no player" playing the game
        /// </summary>
        public void Initialise()
        {
            IOHelper.EnsureDirectory(PlayerProfileDirectory);
            CurrentPlayer = GetLastPlayer();
            if (CurrentPlayer == null)
            {
                // CurrentPlayer = 
                EnsureCurrentPlayer();
            }
        }


        /// <summary>
        ///   /
        /// </summary>
        /// <param name="playerId"> </param>
        public PlayerDetails Load(string playerId)
        {
            using (IsolatedStorageFile store = IsolatedStorageFile.GetUserStoreForApplication())
            {
                return Load(playerId, store);
            }
        }


        /// <summary>
        ///   Delete a player from Isolated Storage
        /// </summary>
        /// <remarks>
        /// This method must ensure that <see cref="CurrentPlayer"/> remains valid, so if the player being deleted is the current player
        /// then a new current player will be nominated (randomly)</remarks>
        /// <param name="playerId"> </param>
        public void DeletePlayer(string playerId)
        {
            IOHelper.DeleteFile(playerId);
            EnsureCurrentPlayer();
        }


        /// <summary>
        /// Creates a player if none exists and sets them to be the <see cref="CurrentPlayer"/>
        /// </summary>
        public void EnsureCurrentPlayer()
        {
            if (CurrentPlayer == null)
            {
                Logger.Debug("No current player found, creating new player");
                PlayerDetails anonPlayer = new PlayerDetails
                    {
                        Name = LocalizationHelper.GetString("UnnamedPlayer"),
                        PreferredColor = (Color) Application.Current.Resources["PhoneAccentColor"]
                    };
                SavePlayer(anonPlayer);
                CurrentPlayer = anonPlayer;
                Logger.Info("Created new player {0} and set CurrentPlayer", CurrentPlayer.Name);
            }
            else
            {
                Logger.Debug("Current player ensured: {0}", CurrentPlayer.Name);
            }
        }

        #endregion

        private PlayerDetails Load(string fileName, IsolatedStorageFile store)
        {
            if (!store.FileExists(fileName))
            {
                throw new FileNotFoundException(fileName);
            }
            PlayerDetails player;
            using (IsolatedStorageFileStream fileStream = store.OpenFile(fileName, FileMode.Open,
                                                                         FileAccess.Read, FileShare.None))
            {
                Logger.Debug("Opened file {0} successfully.  Now reading player data", fileName);
                DataContractSerializer serializer = new DataContractSerializer(typeof (PlayerDetails));
                player = (PlayerDetails) serializer.ReadObject(fileStream);
                player.Id = fileName;
                Logger.Debug("Read data for {0} successfully", player.Name);
            }
            return player;
        }


        private PlayerDetails GetLastPlayer()
        {
            Logger.Info("Loading most recent player");
            using (IsolatedStorageFile store = IsolatedStorageFile.GetUserStoreForApplication())
            {
                string playerFilename;
                if (IsolatedStorageSettings.ApplicationSettings.TryGetValue("CurrentPlayerFilename", out playerFilename))
                {
                    Logger.Info("Found IsolatedStorageSettings value for latest player: {0}", playerFilename);
                    CurrentPlayer = Load(playerFilename, store);
                }
                else
                {
                    Logger.Info("No current player found in IsolatedStoragetSettings, loading first one");
                    List<PlayerDetails> players = GetAvailablePlayers();
                    if (players.Count == 0)
                    {
                        Logger.Error("No players available, returning null");
                        CurrentPlayer = null;
                    }
                    else
                    {
                        CurrentPlayer = players.First();
                    }
                }
            }
            return CurrentPlayer;
        }

    }
}