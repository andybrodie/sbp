using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using Locima.SlidingBlock.Common;
using Locima.SlidingBlock.IO;
using Locima.SlidingBlock.Persistence;
using Microsoft.Phone.Controls;
using NLog;

namespace Locima.SlidingBlock
{
    public partial class GameEnd : PhoneApplicationPage
    {
        private const string SaveGameQueryParameterName = "SaveGame";
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public GameEnd()
        {
            Logger.Info("Constructor entry");
            InitializeComponent();
            LevelStats = new ObservableCollection<LevelStat>();
            Unloaded += (sender, args) => Logger.Info("Unloaded entry/exit");
            Logger.Info("Constructor exit");
        }

        public ObservableCollection<LevelStat> LevelStats { get; set; }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            Logger.Info("OnNavigatedTo entry");

            LevelStats.Clear();
            string saveGameId;
            if (NavigationContext.QueryString.TryGetValue(SaveGameQueryParameterName, out saveGameId))
            {
                SaveGame sg = SaveGameStorageManager.Instance.Load(saveGameId);
                PopulateLevelStats(sg);

                int highscoreTablePosition = AddSaveGameToHighScores(sg);

                // SaveGameStorageManager.Instance.DeleteGame(saveGameId);

                string endGameMessageText = string.Format(LocalizationHelper.GetString("EndGameMessage"),
                                                          LocalizationHelper.GetTimeSpanString(sg.TotalTime),
                                                          sg.TotalMoves, highscoreTablePosition,
                                                          LocalizationHelper.GetOrdinal(highscoreTablePosition));

                EndGameMessage.Text = endGameMessageText;
            }
            else
            {
                Logger.Error("No savegame passed to GameEnd.xaml");
            }
            Logger.Info("OnNavigatedTo exit");
        }


        private void PopulateLevelStats(SaveGame sg)
        {
            if (sg == null) throw new ArgumentNullException("sg");
            LevelStats.Clear();
            for (int levelIndex = 0; levelIndex < sg.Levels.Count; levelIndex++)
            {
                LevelState level = sg.Levels[levelIndex];
                LevelStat ls = new LevelStat
                    {
                        Index = levelIndex + 1,
                        MoveCount = level.MoveCount,
                        ElapsedTime = level.ElapsedTime,
                        Thumbnail = level.Thumbnail
                    };
                LevelStats.Add(ls);
                Logger.Debug("Created new LevelStat: {0} {1} {2}", ls.Index, ls.MoveCount, ls.ElapsedTime);
            }
        }


        private int AddSaveGameToHighScores(SaveGame saveGame)
        {
            HighScoreTable table = HighScoresStorageManager.Instance.Load();
            Highscore newHs = new Highscore
                {
                    GameId = saveGame.Id,
                    Name = saveGame.LocalPlayerDetails.Name,
                    PlayerId = saveGame.LocalPlayerDetails.Id,
                    TotalTime = saveGame.TotalTime,
                    When = DateTime.Now
                };
            Highscore existingHighscore = table.Scores.FirstOrDefault(highscore => highscore.GameId == saveGame.Id);
            int hsIndex;
            if (existingHighscore == null)
            {
                Logger.Info("Adding new high score");
                table.Scores.Add(newHs);
                table.Sort();
                HighScoresStorageManager.Instance.Save(table);
                hsIndex = table.Scores.IndexOf(newHs);
            }
            else
            {
                Logger.Info(
                    "Ignoring repeat call to add existing highscore (may be caused by leave app and coming back to this page");
                hsIndex = table.Scores.IndexOf(existingHighscore);
            }
            return hsIndex;
        }


        public static Uri CreateNavigationUri(string saveGameId)
        {
            if (string.IsNullOrEmpty(saveGameId)) throw new ArgumentNullException("saveGameId");
            return new Uri(string.Format("/GameEnd.xaml?{0}={1}&{2}={3}", SaveGameQueryParameterName, saveGameId,
                                         App.SuppressBackQueryParameterName, Boolean.TrueString), UriKind.Relative);
        }


        private void HighscoreButtonClick(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(HighScores.CreateNavigationUri("MainMenu", MainPage.CreateNavigationUri()));
        }


        private void MenuMenuButtonClick(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(MainPage.CreateNavigationUri());
        }
    }

    public class LevelStats : ObservableCollection<LevelStat>
    {
    }

    public class LevelStat
    {
        public int Index { get; set; }
        public int MoveCount { get; set; }
        public TimeSpan ElapsedTime { get; set; }
        public WriteableBitmap Thumbnail { get; set; }
    }
}