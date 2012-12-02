using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO.IsolatedStorage;
using System.Windows.Navigation;
using Locima.SlidingBlock.Common;
using Locima.SlidingBlock.GameTemplates;
using Locima.SlidingBlock.IO;
using Locima.SlidingBlock.Persistence;
using NLog;

namespace Locima.SlidingBlock.ViewModel.Menus
{
    /// <summary>
    /// Brokers different menu pages, allowing a single XAML page (<see cref="MainPage"/>) to be reused for all menus
    /// </summary>
    /// <remarks>
    /// Everything in here is static, because the data model for menu pages is static</remarks>
    internal class MenuPageBroker
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Holds the different menu pages available, see <see cref="MenuPageBroker"/>
        /// </summary>
        private static readonly Dictionary<string, Func<IEnumerable<JournalEntry>,MenuPageViewModel>> Menus;

        /// <summary>
        /// Initialises the different named menu pages that are available
        /// </summary>
        /// <remarks>
        /// Each top level menu page has a method that generates the items for that particular menu, however we don't create these until we actually need them (hence using a <see cref="Func{TResult}"/>
        /// to create the menu page rather than just instantiate it immediately.</remarks>
        static MenuPageBroker()
        {
            Menus = new Dictionary<string, Func<IEnumerable<JournalEntry>,MenuPageViewModel>>
                        {
                            {
                                // The first page the user sees
                                "MainMenu", CreateMainMenu
                            },
                            {
                                // Used to start a new game
                                "NewGameMenu", CreateNewGameMenu
                            },
                            {
                                // Used to select a game difficulty
                                "SelectDifficulty", CreateGameDifficultySelector
                            },
                            {
                                // Select a game template
                                "SelectGameTemplate", CreateGameTemplateSelector
                            }
                        };
        }


        protected static string SelectedTemplateId
        {
            get { return IsolatedStorageSettings.ApplicationSettings["Menu_SelectedTemplateId"] as string; }
            set { IsolatedStorageSettings.ApplicationSettings["Menu_SelectedTemplateId"] = value; }
        }


        /// <summary>
        /// Retrieves a menu page from <see cref="Menus"/> based on the <paramref name="pageName"/>,
        /// either returning the desired menu page model or throws <see cref="InvalidStateException"/> if a menu page doesn't exist (this should be considered either
        /// a hack attempt or, more likely, a defect).
        /// </summary>
        /// <param name="pageName"></param>
        /// <param name="backStack"></param>
        /// <returns>Never returns null</returns>
        public static MenuPageViewModel RetrieveMenuPage(string pageName, IEnumerable<JournalEntry> backStack)
        {
            Func<IEnumerable<JournalEntry>, MenuPageViewModel> menuPageFactory;
            if (Menus.TryGetValue(pageName, out menuPageFactory))
            {
                Logger.Info("Creating menu page {0}", pageName);
                return menuPageFactory.Invoke(backStack);
            }
            throw new InvalidStateException("Invalid menu page name navigated to: {0}", pageName);
        }


        /// <summary>
        /// Creates the main menu menu items which is the first page the user sees when launching the application
        /// </summary>
        private static MenuPageViewModel CreateMainMenu(IEnumerable<JournalEntry> backStack)
        {
            SaveGame continuableGame =
                SaveGameStorageManager.Instance.GetContinuableGame(PlayerStorageManager.Instance.CurrentPlayer.Id);

            MenuPageViewModel mpvm = new MenuPageViewModel
                                         {
                                             PageTitle = LocalizationHelper.GetString("MainMenu"),
                                             MenuItems = new ObservableCollection<MenuItemViewModel>
                                                             {
                                                                 new MenuItemViewModel
                                                                     {
                                                                         Title =
                                                                             LocalizationHelper.GetString("ContinueGame"),
                                                                         Text =
                                                                             LocalizationHelper.GetString(
                                                                                 "ContinueGameDescription"),
                                                                         TargetUri =
                                                                             continuableGame == null
                                                                                 ? null
                                                                                 : GamePage.CreateNavigationUri(
                                                                                     continuableGame.Id, 0),
                                                                         IsEnabled = continuableGame != null
                                                                     },
                                                                 new MenuItemViewModel
                                                                     {
                                                                         Title = LocalizationHelper.GetString("NewGame"),
                                                                         Text =
                                                                             LocalizationHelper.GetString(
                                                                                 "NewGameDescription"),
                                                                         TargetPage = "NewGameMenu"
                                                                     },
                                                                 new MenuItemViewModel
                                                                     {
                                                                         Title =
                                                                             LocalizationHelper.GetString("LoadGame"),
                                                                         Text =
                                                                             LocalizationHelper.GetString(
                                                                                 "LoadGameDescription"),
                                                                         TargetUri =
                                                                             SavedGameSelector.CreateNavigationUri(),
                                                                         IsEnabled = continuableGame != null
                                                                     },
                                                                 new MenuItemViewModel
                                                                     {
                                                                         Title = LocalizationHelper.GetString("Custom"),
                                                                         Text =
                                                                             LocalizationHelper.GetString(
                                                                                 "CustomDescription"),
                                                                         TargetUri =
                                                                             GameTemplateSelector.CreateNavigationUri(),
                                                                     }
                                                             }
                                         };
            return mpvm;
        }


        /// <summary>
        /// When the user wants to start a new game they need to select which game template to use, IFF there is more than one defined, otherwise just use the default
        /// </summary>
        /// <param name="tilesAcross">Each puzzle will have this many tiles across in the puzzle</param>
        /// <param name="tilesHigh">Each puzzle will have this many tiles high in the puzzle</param>
        /// <param name="pagesOnBackStackToSuppress">The number of pages that have been navigated through the menus, which must be suppressed (<see cref="CalculatePagesToRemoveInBackstack"/>)</param>
        /// <returns>Either a Uri to navigate to the GameTemplateSelector</returns>
        private static Uri CreateNewGame(int tilesAcross, int tilesHigh, int pagesOnBackStackToSuppress)
        {           
            SaveGame sg = SaveGameFactory.CreateSaveGame(GameTemplateStorageManager.Instance.GetGameTemplates(false, false)[0],
                                                         tilesAcross, tilesHigh);

            /* We want to suppress navigating back to this page because otherwise clicking "Back" from a game would put you back to this menu item
             * which isn't useful for the user (i.e. in the middle of a game, clicking back would give you the "Easy", "Medium", "Hard" choice again
             */
            return GamePage.CreateNavigationUri(sg.Id, pagesOnBackStackToSuppress);
        }


        /// <summary>
        /// The new game menu is a virtual menu that either shows the <see cref="CreateGameTemplateSelector"/> menu or the <see cref="CreateGameDifficultySelector"/>
        /// </summary>
        /// <param name="backStack">The current app back stack, used by <see cref="CreateGameDifficultySelector"/></param>
        /// <returns></returns>
        private static MenuPageViewModel CreateNewGameMenu(IEnumerable<JournalEntry> backStack)
        {
            MenuPageViewModel mpvm;

            List<GameTemplate> templates = GameTemplateStorageManager.Instance.GetGameTemplates(false, false);
            if (templates.Count > 1)
            {
                // The user needs to select a game template because there's more than one
                mpvm = CreateGameTemplateSelector(templates);
            }
            else
            {
                // Auto-select the only game template and move on to the difficulty selector
                SelectedTemplateId = templates[0].Id;
                mpvm = CreateGameDifficultySelector(backStack);
            }
            return mpvm;
        }


        private static MenuPageViewModel CreateGameDifficultySelector(IEnumerable<JournalEntry> backStack)
        {
            /* Find how many pages to remove from the back stack to ensure that pressing "Back" from the game page returns to the top menu
             * We need this because we can't tell how many menu pages we've traversed to get here, because menus are dynamic
             * based on the number of game templates in the system
             */
            int pagesOnBackStackToSuppress = CalculatePagesToRemoveInBackstack(backStack);
            pagesOnBackStackToSuppress++; // Need to suppress this page too!

            return new MenuPageViewModel
                       {
                           PageTitle = LocalizationHelper.GetString("NewGame"),
                           MenuItems = new ObservableCollection<MenuItemViewModel>
                                           {
                                               CreateDifficultyMenuItem("Easy", 3, 3, pagesOnBackStackToSuppress),
                                               CreateDifficultyMenuItem("Medium", 4, 4, pagesOnBackStackToSuppress),
                                               CreateDifficultyMenuItem("Hard", 5, 5, pagesOnBackStackToSuppress)
                                           }
                       };
        }


        /// <summary>
        /// Calculates the number of pages in the backstack that we want to suppress when navigating to the <see cref="GamePage"/>
        /// </summary>
        /// <remarks>This is used to provide a "clean" user experience.  If the user hits <c>Back</c> from the game, then we don't want to give them
        /// the select difficulty screen on create new game, we want to bring them back to the main menu.  Therefore we need to suppress
        /// the game template selector and the difficulty selector menus.  Unfortunately, the number of menus between the <see cref="GamePage"/>
        /// and the main menu is variable, depending on what the user has been doing; therefore we have to examine the back stack and
        /// calculate how many pages we need to ask <see cref="GamePage"/> to suppress (using <see cref="App.HandleBackstackSuppression"/>.  Hence this method!</remarks>
        /// <param name="backStack">The current back stack (as obtained from <see cref="NavigationService.BackStack"/>) and passed to <see cref="RetrieveMenuPage"/></param>
        /// <returns>The number of pages to suppress on the back stack when we navigate to <see cref="GamePage"/>f</returns>
        private static int CalculatePagesToRemoveInBackstack(IEnumerable<JournalEntry> backStack)
        {
            string mainMenuExplicitUri = MainPage.CreateNavigationUri("MainMenu").ToString();
            string mainMenuImplicitUri = MainPage.CreateNavigationUri(null).ToString();
            Logger.Info("Looking for {0} or {1} in the back stack ", mainMenuImplicitUri, mainMenuExplicitUri);
            /* We're looking for either:
             * 1. A Uri for MenuPage.xaml which has no parameter, or
             * 2. A Uri for MenuPage.xaml which explicitly specifies the MainMenu page
             */
            int suppressCount = 0;
            foreach (JournalEntry je in backStack)
            {
                string uri = je.Source.ToString().Substring(1); // Need to remove one of the forward slash-characters.  For some reason Uris always start with // instead of /

                Logger.Debug("Comparing {0} with {1} and {2}", uri, mainMenuImplicitUri, mainMenuExplicitUri);
                if (uri.StartsWith(mainMenuImplicitUri) || // Using StartsWith just in case we add another parameter another time, but if so need to make sure menu page parameter is the first one!
                    uri.StartsWith(mainMenuExplicitUri))    
                {
                    Logger.Debug("Found MainMenu Uri {0} at {1} deep in the backstack, will remove up until that point",
                                 uri, suppressCount);
                }
                else
                {
                    suppressCount++;
                    Logger.Debug("Will suppress {0}, bringing suppress count to {1}", uri, suppressCount);
                }
            }
            Logger.Info("Found a total of {0} pages to suppress in the back stack", suppressCount);
            return suppressCount;
        }


        /// <summary>
        /// Creates a menu item that will create a new game of a specified difficulty
        /// </summary>
        /// <remarks>
        /// Difficulty is determined by <paramref name="tilesAcross"/> and <paramref name="tilesHigh"/></remarks>
        /// <param name="gameLabel">The label to use (will be localised by this method)</param>
        /// <param name="tilesAcross">The number of tiles across the puzzle should be</param>
        /// <param name="tilesHigh">The number of tiles high the puzzle should be</param>
        /// <param name="pagesOnBackStackToSuppress">The number of pages on the back stack to request that <see cref="GamePage"/> suppresses, see <see cref="CalculatePagesToRemoveInBackstack"/></param>
        /// <returns>A menu item that, when selected, will create a new game</returns>
        private static MenuItemViewModel CreateDifficultyMenuItem(string gameLabel, int tilesAcross, int tilesHigh, int pagesOnBackStackToSuppress)
        {
            return new MenuItemViewModel
            {
                Title = LocalizationHelper.GetString(gameLabel),
                Text = LocalizationHelper.GetString("GameDescription", tilesAcross, tilesHigh),
                SelectedAction = () => CreateNewGame(tilesAcross, tilesHigh, pagesOnBackStackToSuppress)
            };
        }


        /// <summary>
        /// Creates the game template selector page view model using the default template list (i.e. no shadows)
        /// </summary>
        /// <param name="unused">Unused</param>
        /// <returns>A menu page view model that offers a selection of templates</returns>
        public static MenuPageViewModel CreateGameTemplateSelector(IEnumerable<JournalEntry> unused)
        {
            return CreateGameTemplateSelector(GameTemplateStorageManager.Instance.GetGameTemplates(false, false));
        }

        /// <summary>
        /// Creates the game template selector page view model using the template list passed by <paramref name="templates"/>
        /// </summary>
        /// <param name="templates">A set of templates to select from</param>
        /// <returns>A menu page view model that offers a selection of templates</returns>
        private static MenuPageViewModel CreateGameTemplateSelector(IEnumerable<GameTemplate> templates)
        {
            MenuPageViewModel mpvm = new MenuPageViewModel
            {
                PageTitle = LocalizationHelper.GetString("GameTemplateSelectorTitle"),
                MenuItems = new ObservableCollection<MenuItemViewModel>()
            };

            foreach (GameTemplate template in templates)
            {
                GameTemplate gameTemplate = template;   // Have to do this because we access the template ID inside the closure below
                mpvm.MenuItems.Add(new MenuItemViewModel
                {
                    Title = template.Title,
                    Text = template.Author,
                    SelectedAction = delegate
                    {
                        SelectedTemplateId = gameTemplate.Id;
                        return MainPage.CreateNavigationUri("SelectDifficulty");
                    }
                });
            }
            return mpvm;
        }


    }
}