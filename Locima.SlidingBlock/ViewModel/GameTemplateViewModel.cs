using System.Windows.Input;
using Locima.SlidingBlock.Common;
using Locima.SlidingBlock.GameTemplates;
using Locima.SlidingBlock.IO;
using Locima.SlidingBlock.Messaging;
using NLog;

namespace Locima.SlidingBlock.ViewModel
{
    /// <summary>
    /// MVVM view model for a single item in the list of custom games (<see cref="GameTemplateSelectorViewModel"/>)
    /// </summary>
    public class GameTemplateViewModel : ViewModelBase
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Backing field for <see cref="Title"/>
        /// </summary>
        private string _title;

        /// <summary>
        /// Initialise this view model and all its fields using the <paramref name="game"/> passed
        /// </summary>
        /// <param name="parent">The view model that owns this specific custom game</param>
        /// <param name="game">The game template that this item represents</param>
        public GameTemplateViewModel(GameTemplateSelectorViewModel parent, GameTemplate game)
        {
            ShareMessageHandlers(parent);
            Title = game.Title;
            Id = game.Id;
            IsReadOnly = game.IsReadOnly;
            DeleteGameTemplateCommand = new DelegateCommand(DeleteGameTemplateAction);
            CopyGameTemplateCommand = new DelegateCommand(CopyGameTemplateAction);
            Parent = parent;
        }


        /// <summary>
        /// The parent view model, this needs to be notified if we remove or add a new game template via <see cref="DeleteGameTemplateAction"/> or <see cref="CopyGameTemplateAction"/>
        /// so it can tell the view to refresh the list of available templates
        /// </summary>
        protected GameTemplateSelectorViewModel Parent { get; set; }


        /// <summary>
        /// If the template is read only, as copied from <see cref="GameTemplate.IsReadOnly"/>
        /// </summary>
        /// <remarks>Read yonly game templates cannot be deleted</remarks>
        private bool IsReadOnly { get; set; }

        /// <summary>
        /// The title of the game template, copied from <see cref="GameTemplate.Title"/>
        /// </summary>
        public string Title
        {
            get { return _title; }
            private set
            {
                _title = value;
                OnNotifyPropertyChanged("Title");
            }
        }

        /// <summary>
        /// The ID of the game template, as copied from <see cref="GameTemplate.Id"/>
        /// </summary>
        public string Id { get; private set; }


        /// <summary>
        /// Invoked if a game template is to be deleted.
        /// </summary>
        /// <see cref="DeleteGameTemplateAction"/>
        public ICommand DeleteGameTemplateCommand { get; set; }

        /// <summary>
        /// Invoked if a game template is to be copied.
        /// </summary>
        public ICommand CopyGameTemplateCommand { get; set; }

        private void CopyGameTemplateAction(object obj)
        {
            Logger.Info("Cloning game template {0} ({1})", Title, Id);
            GameTemplate template = GameTemplateStorageManager.Instance.Load(Id); // TODO Fix wastefulness of loading this twice
            template.Id = null;
                // Resetting ID to null will cause a new ID to be assigned and a copy to be made when we save
            template.IsReadOnly = false; // If cloning a built-in game, then set it to read-write
            template.Title = LocalizationHelper.GetString("GameTemplateCopyTitle", template.Title);
                // Change the name so the user can tell them apart
            GameTemplateStorageManager.Instance.Save(template);
            Parent.Refresh();
        }


        private void DeleteGameTemplateAction(object obj)
        {
            if (IsReadOnly)
            {
                SendViewMessage(new ConfirmationMessageArgs
                                    {
                                        Title = LocalizationHelper.GetString("CannotDeleteReadOnlyGameTemplateTitle"),
                                        Message =
                                            LocalizationHelper.GetString("CannotDeleteReadOnlyGameTemplateText", Title)
                                    });
            }
            else
            {
                SendViewMessage(new ConfirmationMessageArgs
                                    {
                                        Title = LocalizationHelper.GetString("ConfirmDeleteGameTemplateTitle"),
                                        Message = LocalizationHelper.GetString("ConfirmDeleteGameTemplateText", Title),
                                        OnOkCommand = new DelegateCommand(delegate
                                                                              {
                                                                                  Logger.Info(
                                                                                      "User confirmed delete of game template {0}",
                                                                                      Id);
                                                                                  GameTemplateStorageManager.Instance
                                                                                                            .Delete(Id);
                                                                                  Parent.GameTemplateList.Remove(this);

                                                                              }),
                                        OnCancelCommand =
                                            new DelegateCommand(
                                            o => Logger.Info("User decided not to delete game template"))
                                    });
            }
        }
    }
}