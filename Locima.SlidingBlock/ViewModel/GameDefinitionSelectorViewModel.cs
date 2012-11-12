using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Input;
using Locima.SlidingBlock.GameTemplates;
using Locima.SlidingBlock.IO;
using Locima.SlidingBlock.Messaging;
using NLog;

namespace Locima.SlidingBlock.ViewModel
{
    /// <summary>
    /// The MVVM view model for the <see cref="GameDefinitionSelector"/> view
    /// </summary>
    public class GameDefinitionSelectorViewModel : ViewModelBase
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Initialise <see cref="CustomGameList"/>
        /// </summary>
        public GameDefinitionSelectorViewModel()
        {
            CustomGameList = new ObservableCollection<GameDefinitionViewModel>();
            CreateGameDefinitionCommand = new DelegateCommand(CreateGameDefinitionAction);
            SelectGameDefinitionCommand = new DelegateCommand(SelectGameDefinitionAction);
        }

        public ObservableCollection<GameDefinitionViewModel> CustomGameList { get; private set; }

        /// <summary>
        /// Invoked to create a new custom game template
        /// </summary>
        public ICommand CreateGameDefinitionCommand { get; private set; }

        /// <summary>
        /// Invoked to select a custom game template to edit
        /// </summary>
        public ICommand SelectGameDefinitionCommand { get; private set; }

        private void SelectGameDefinitionAction(object obj)
        {
            GameDefinitionViewModel selectedItem = (GameDefinitionViewModel) obj;

            SendViewMessage(new NavigationMessageArgs(GameEditor.CreateNavigationUri(selectedItem.Id)));
        }


        private void CreateGameDefinitionAction(object unused)
        {
            SendViewMessage(new NavigationMessageArgs(GameEditor.CreateNavigationUri()));
        }

        public void Initialise()
        {
            CustomGameList.Clear();
            List<GameDefinition> customGames = GameDefinitionStorageManager.Instance.GetCustomGameDefinitions();
            customGames.Sort(
                (definition, gameDefinition) =>
                String.Compare(definition.Title, gameDefinition.Title, StringComparison.InvariantCultureIgnoreCase));
            foreach (GameDefinition gameDef in customGames)
            {
                CustomGameList.Add(new GameDefinitionViewModel(this, gameDef));
            }
        }
    }
}