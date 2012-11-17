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
    /// The MVVM view model for the <see cref="GameTemplateSelector"/> view
    /// </summary>
    public class GameTemplateSelectorViewModel : ViewModelBase
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Initialise <see cref="GameTemplateList"/>
        /// </summary>
        public GameTemplateSelectorViewModel()
        {
            GameTemplateList = new ObservableCollection<GameTemplateViewModel>();
            CreateGameTemplateCommand = new DelegateCommand(CreateGameTemplateAction);
            SelectGameTemplateCommand = new DelegateCommand(SelectGameTemplateAction);
        }


        /// <summary>
        /// The list of game templates
        /// </summary>
        public ObservableCollection<GameTemplateViewModel> GameTemplateList { get; private set; }

        /// <summary>
        /// Invoked to create a new custom game template
        /// </summary>
        public ICommand CreateGameTemplateCommand { get; private set; }

        /// <summary>
        /// Invoked to select a game template to edit
        /// </summary>
        public ICommand SelectGameTemplateCommand { get; private set; }

        private void SelectGameTemplateAction(object obj)
        {
            GameTemplateViewModel selectedItem = (GameTemplateViewModel) obj;

            SendViewMessage(new NavigationMessageArgs(GameEditor.CreateNavigationUri(selectedItem.Id)));
        }


        private void CreateGameTemplateAction(object unused)
        {
            SendViewMessage(new NavigationMessageArgs(GameEditor.CreateNavigationUri()));
        }

        public void Initialise()
        {
            Refresh();
        }

        public void Refresh()
        {
            GameTemplateList.Clear();
            List<GameTemplate> gameTemplates = GameTemplateStorageManager.Instance.GetGameTemplates();
            gameTemplates.Sort(
                (definition, gameTemplate) =>
                String.Compare(definition.Title, gameTemplate.Title, StringComparison.InvariantCultureIgnoreCase));
            foreach (GameTemplate gameTemplate in gameTemplates)
            {
                GameTemplateList.Add(new GameTemplateViewModel(this, gameTemplate));
            }

        }
    }
}