using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using Locima.SlidingBlock.Common;
using Locima.SlidingBlock.GameTemplates;
using Locima.SlidingBlock.IO;
using Locima.SlidingBlock.Persistence;
using Microsoft.Phone.Controls;
using NLog;

namespace Locima.SlidingBlock
{


    /// <summary>
    /// Traditional "Code-behind" page shown to the user when they've finished the game
    /// </summary>
    public partial class GameEnd : PhoneApplicationPage
    {
        private const string SaveGameQueryParameterName = "SaveGame";
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Calls <see cref="InitializeComponent"/>, allocates <see cref="LevelStats"/>
        /// </summary>
        public GameEnd()
        {
            InitializeComponent();
            LevelStats = new LevelStats();
        }

        /// <summary>
        /// The stats for the level
        /// </summary>
        public LevelStats LevelStats { get; set; }

        /// <summary>
        /// Load the statistics from the <see cref="SaveGame"/> (named in the Uri as <see cref="SaveGameQueryParameterName"/>), set them up in using <see cref="PopulateLevelStats"/> 
        /// and finally initialise the <see cref="EndGameMessage"/>
        /// </summary>
        /// <param name="e"></param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            Logger.Info("OnNavigatedTo entry");

            LevelStats.Clear();
            string saveGameId;
            if (NavigationContext.QueryString.TryGetValue(SaveGameQueryParameterName, out saveGameId))
            {
                SaveGame sg = SaveGameStorageManager.Instance.Load(saveGameId);
                PopulateLevelStats(sg);

                int highScoreTablePosition = AddSaveGameToHighScores(sg);

                // SaveGameStorageManager.Instance.DeleteGame(saveGameId);

                string endGameMessageText = string.Format(LocalizationHelper.GetString("EndGameMessage"),
                                                          LocalizationHelper.GetTimeSpanString(sg.TotalTime),
                                                          sg.TotalMoves,
                                                          LocalizationHelper.GetOrdinal(highScoreTablePosition + 1));

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
            GameTemplate template = GameTemplateStorageManager.Instance.Load(saveGame.GameDefinitionId);
            HighScore newHs = new HighScore
                {
                    GameId = saveGame.Id,
                    Name = saveGame.LocalPlayerDetails.Name,
                    PlayerId = saveGame.LocalPlayerDetails.Id,
                    TotalTime = saveGame.TotalTime,
                    When = DateTime.Now,
                    TotalMoves = saveGame.TotalMoves,
                    TemplateName = template.Title,
                    Difficulty = GetDifficulty(saveGame)
                };
            HighScore existingHighScore = table.Scores.FirstOrDefault(highScore => highScore.GameId == saveGame.Id);
            int hsIndex;
            if (existingHighScore == null)
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
                    "Ignoring repeat call to add existing high score (may be caused by leave app and coming back to this page");
                hsIndex = table.Scores.IndexOf(existingHighScore);
            }
            return hsIndex;
        }

        private string GetDifficulty(SaveGame saveGame)
        {
            if (saveGame.Levels == null || saveGame.Levels.Count == 0)
            {
                return LocalizationHelper.GetString("CustomDifficulty");
            }
            switch (saveGame.Levels[0].TilesAcross)
            {
                case 3:
                    return LocalizationHelper.GetString("Easy");
                case 4:
                    return LocalizationHelper.GetString("Medium");
                case 5:
                    return LocalizationHelper.GetString("Hard");
                default:
                    return LocalizationHelper.GetString("CustomDifficulty");
            }
        }


        /// <summary>
        /// Creates a Uri that navigates to this page, showing statistics and adding to the high score for the <paramref name="saveGameId"/> <see cref="SaveGame"/> specified
        /// </summary>
        /// <param name="saveGameId">The save game that was completed</param>
        /// <returns>A Uri for use with <see cref="NavigationService.Navigate"/></returns>
        public static Uri CreateNavigationUri(string saveGameId)
        {
            if (string.IsNullOrEmpty(saveGameId)) throw new ArgumentNullException("saveGameId");
            UriConstructor uri = new UriConstructor("/GameEnd.xaml", UriKind.Relative);
            uri.AddParameter(SaveGameQueryParameterName, saveGameId);
            uri.AddParameter(App.SuppressBackQueryParameterName, Boolean.TrueString);
            return uri.ToUri();
        }


        private void HighScoreButtonClick(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(HighScores.CreateNavigationUri("MainMenu", MainPage.CreateNavigationUri(null)));
        }


        private void MenuMenuButtonClick(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(MainPage.CreateNavigationUri(null));
        }
    }


    /// <summary>
    /// Used for displaying level stats in the designer view, required by Silverlight 4 doesn't support generic type references in object declarations
    /// </summary>
    public class LevelStats : ObservableCollection<LevelStat>
    {
    }

    /// <summary>
    /// A set of statistics for a single level
    /// </summary>
    public class LevelStat
    {

        /// <summary>
        /// The position of the level with respect to other levels
        /// </summary>
        public int Index { get; set; }

        /// <summary>
        /// The number of moves the player took to complete the level
        /// </summary>
        public int MoveCount { get; set; }
        /// <summary>
        /// The total amount of time it took the player to complete the level
        /// </summary>
        public TimeSpan ElapsedTime { get; set; }

        /// <summary>
        /// The thumbnail image for the level
        /// </summary>
        public WriteableBitmap Thumbnail { get; set; }
    }
}