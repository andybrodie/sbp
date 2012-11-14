using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Locima.SlidingBlock.Common;
using Locima.SlidingBlock.IO;
using Locima.SlidingBlock.Persistence;
using Locima.SlidingBlock.SinglePlayer;

namespace Locima.SlidingBlock.ViewModel
{

    /// <summary>
    /// Brokers different menu pages, allowing a single XAML page (<see cref="MainPage"/>) to be reused for all menus
    /// </summary>
    /// <remarks>
    /// Everything in here is static, because the data model for menu pages is static</remarks>
    internal class MenuPageBroker
    {

        /// <summary>
        /// Holds the different menu pages available, see <see cref="MenuPageBroker"/>
        /// </summary>
        private static readonly Dictionary<string, Func<MenuPageViewModel>> Menus;

        /// <summary>
        /// Initialises the different menu pages available
        /// </summary>
        static MenuPageBroker()
        {            
            Menus = new Dictionary<string, Func<MenuPageViewModel>>
                {
                    {
                        // The first page the user sees
                        "MainMenu", CreateMainMenu
                    },
                    {
                        // Used to start a new game
                        "NewGameMenu", () => new MenuPageViewModel
                            {
                                PageTitle = LocalizationHelper.GetString("NewGame"),
                                MenuItems = GetNewGameTypes()
                            }
                    },
                    {
                        // Used to load an existing game
                        "LoadGameMenu", () => new MenuPageViewModel
                            {
                                PageTitle = LocalizationHelper.GetString("LoadGame"),
                                MenuItems = GetContinuableGames()
                            }
                    }
                };
        }


        /// <summary>
        /// Creates the main menu menu items which is the first page the user sees when launching the application
        /// </summary>
        private static MenuPageViewModel CreateMainMenu()
        {
            IEnumerable<SaveGame> sg = SaveGameStorageManager.Instance.LoadGames();
            SaveGame continuableGame = sg.FirstOrDefault();

            MenuPageViewModel mpvm = new MenuPageViewModel
                {
                    MenuItems = new ObservableCollection<MenuItemViewModel>
                        {
                            new MenuItemViewModel
                                {
                                    Title = LocalizationHelper.GetString("ContinueGame"),
                                    Text = LocalizationHelper.GetString("ContinueGameDescription"),
                                    TargetUri = continuableGame==null ? null : GamePage.CreateNavigationUri(continuableGame.Id, false),
                                    IsEnabled = continuableGame!=null
                                },
                            new MenuItemViewModel
                                {
                                    Title = LocalizationHelper.GetString("NewGame"),
                                    Text = LocalizationHelper.GetString("NewGameDescription"),
                                    TargetPage = "NewGameMenu"
                                },
                            new MenuItemViewModel
                                {
                                    Title = LocalizationHelper.GetString("LoadGame"),
                                    Text = LocalizationHelper.GetString("LoadGameDescription"),
                                    TargetUri = SavedGameSelector.CreateNavigationUri(),
                                    IsEnabled = continuableGame!=null
                                },
                            new MenuItemViewModel
                                {
                                    Title = LocalizationHelper.GetString("Custom"),
                                    Text = LocalizationHelper.GetString("CustomDescription"),
                                    TargetUri = GameTemplateSelector.GetNavigationUri(),
                                }
                        },
                    PageTitle = LocalizationHelper.GetString("MainMenu")
                };
            return mpvm;

        }

        /// <summary>
        /// Retrieves a menu page from <see cref="Menus"/> based on the <paramref name="pageName"/>,
        /// either returning the desired menu page model or throws <see cref="InvalidStateException"/> if a menu page doesn't exist (this should be considered either
        /// a hack attempt or, more likely, a defect).
        /// </summary>
        /// <param name="pageName"></param>
        /// <returns>Never returns null</returns>
        public static MenuPageViewModel RetrieveMenuPage (string pageName)
        {
            Func<MenuPageViewModel> menuPageFactory;
            if (Menus.TryGetValue(pageName, out menuPageFactory))
            {
                return menuPageFactory.Invoke();
            }
            throw new InvalidStateException("Invalid menu page name navigated to: {0}",pageName);
        }


        /// <summary>
        /// Retrieves a list of save games for the current player (see <see cref="IPlayerStorageManager.CurrentPlayer"/>) 
        /// </summary>
        /// <returns></returns>
        private static ObservableCollection<MenuItemViewModel> GetContinuableGames()
        {
            IEnumerable<SaveGame> games = SaveGameStorageManager.Instance.LoadGames(PlayerStorageManager.Instance.CurrentPlayer.Id);
            ObservableCollection<MenuItemViewModel> items = new ObservableCollection<MenuItemViewModel>();
            foreach (SaveGame puzzle in games)
            {
                MenuItemViewModel item = new MenuItemViewModel
                    {
                        Title = LocalizationHelper.GetString("SavedGameTitle", puzzle.LastUpdate),
                        Text = LocalizationHelper.GetString("SavedGameDescription", puzzle.LocalPlayerDetails.Name),
                        Icon = puzzle.CurrentLevel.Thumbnail
                    };
                items.Add(item);
            }
            return items;
        }


        /// <summary>
        /// Creates a menu item that will create a new game
        /// </summary>
        /// <remarks>
        /// This is a helper method for <see cref="GetNewGameTypes"/></remarks>
        /// <param name="gameLabel">The label to use (will be localised by this method)</param>
        /// <param name="tilesAcross">The number of tiles across the puzzle should be</param>
        /// <param name="tilesHigh">The number of tiles high the puzzle should be</param>
        /// <returns>A menu item that, when selected, will create a new game</returns>
        private static MenuItemViewModel CreateGameMenuItem(string gameLabel, int tilesAcross, int tilesHigh)
        {
            return new MenuItemViewModel
                {
                    Title = LocalizationHelper.GetString(gameLabel),
                    Text = LocalizationHelper.GetString("GameDescription", tilesAcross, tilesHigh),
                    SelectedAction = delegate
                        {
                            SaveGame sg = GameFactory.CreateSinglePlayerGame(tilesAcross, tilesHigh);
                            /* We want to suppress navigating back to this page because otherwise clicking "Back" from a game would put you back to this menu item
                             * which isn't useful for the user (i.e. in the middle of a game, clicking back would give you the "Easy", "Medium", "Hard" choice again
                             */
                            return GamePage.CreateNavigationUri(sg.Id, true);
                        }
                };
        }


        /// <summary>
        /// Creates the menu items for creating a new game
        /// </summary>
        /// <returns></returns>
        private static ObservableCollection<MenuItemViewModel> GetNewGameTypes()
        {
            ObservableCollection<MenuItemViewModel> gameTypeList = new ObservableCollection<MenuItemViewModel>
                                                                       {
                                                                           CreateGameMenuItem("Easy", 3, 3),
                                                                           CreateGameMenuItem("Medium", 4, 4),
                                                                           CreateGameMenuItem("Hard", 5, 5)
                                                                       };
            return gameTypeList;
        }



    }
}