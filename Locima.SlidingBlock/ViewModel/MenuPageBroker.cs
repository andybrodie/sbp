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
    internal class MenuPageBroker
    {
        private static readonly Dictionary<string, Func<MenuPageViewModel>> Menus;

        static MenuPageBroker()
        {            
            Menus = new Dictionary<string, Func<MenuPageViewModel>>
                {
                    {
                        "MainMenu", CreateMenuMenu
                    },
                    {
                        "NewGameMenu", () => new MenuPageViewModel
                            {
                                PageTitle = LocalizationHelper.GetString("NewGame"),
                                MenuItems = GetNewGameTypes()
                            }
                    },
                    {
                        "LoadGameMenu", () => new MenuPageViewModel
                            {
                                PageTitle = LocalizationHelper.GetString("LoadGame"),
                                MenuItems = GetContinuableGames()
                            }
                    }
                };
        }


        private static MenuPageViewModel CreateMenuMenu()
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
                                }
                        },
                    PageTitle = LocalizationHelper.GetString("MainMenu")
                };
            return mpvm;

        }


        public static MenuPageViewModel RetrieveMenuPage (string pageName)
        {
            Func<MenuPageViewModel> menuPageFactory;
            if (Menus.TryGetValue(pageName, out menuPageFactory))
            {
                return menuPageFactory.Invoke();
            }
            throw new InvalidStateException("Invalid menu page name navigated to: {0}",pageName);
        }


        private static ObservableCollection<MenuItemViewModel> GetContinuableGames()
        {
            IEnumerable<SaveGame> games = SaveGameStorageManager.Instance.LoadGames();
            ObservableCollection<MenuItemViewModel> items = new ObservableCollection<MenuItemViewModel>();
            foreach (SaveGame puzzle in games)
            {
                MenuItemViewModel item = new MenuItemViewModel
                    {
                        Title = LocalizationHelper.GetString("SavedGameTitle", puzzle.LastUpdate),
                        Text = LocalizationHelper.GetString("SavedGameDescription", puzzle.LocalPlayerDetails.Name),
                        Icon = puzzle.CurrentLevel.Thumbnail
                    };
            }
            return items;
        }


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


        private static ObservableCollection<MenuItemViewModel> GetNewGameTypes()
        {
            ObservableCollection<MenuItemViewModel> gameTypeList = new ObservableCollection<MenuItemViewModel>
                                           {
                                               CreateGameMenuItem("Easy", 3,3),
                                               CreateGameMenuItem("Medium", 4,4),
                                               CreateGameMenuItem("Hard", 5,5)

//                                                ,new MenuItemViewModel
//                                                    {
//                                                        Title = LocalizationHelper.GetString("Custom"),
//                                                        Text = LocalizationHelper.GetString("CustomDescription"),
//                                                        TargetUri = new Uri("/CreateCustomGame.xaml", UriKind.Relative)
//                                                    }
                                           };
            return gameTypeList;
        }



    }
}