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
            Author = game.Author;
            LevelCount = game.Levels.Count;
            Id = game.Id;
            IsReadOnly = game.IsReadOnly;
            DeleteGameTemplateCommand = new DelegateCommand(DeleteGameTemplateAction);
            CopyGameTemplateCommand = new DelegateCommand(CopyGameTemplateAction);
            Parent = parent;
        }


        private void CopyGameTemplateAction(object obj)
        {
            Logger.Info("Cloning game template {0} ({1})", Title, Id);
            GameTemplate template = GameTemplateStorageManager.Instance.Load(Id);
            template.Id = null; // Resetting ID to null will cause a new ID to be assigned and a copy to be made when we save
            template.IsReadOnly = false; // If cloning a built-in game, then set it to read-write
            template.Title = LocalizationHelper.GetString("GameTemplateCopyTitle", template.Title); // Change the name so the user can tell them apart
            GameTemplateStorageManager.Instance.Save(template);
            Parent.Refresh();

        }

        protected GameTemplateSelectorViewModel Parent { get; set; }

        protected bool IsReadOnly { get; set; }

        public int LevelCount { get; private set; }

        public string Author { get; private set; }

        public string Title
        {
            get { return _title; }
            private set
            {
                _title = value;
                OnNotifyPropertyChanged("Title");
            }
        }

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
                                                                              }),
                                        OnCancelCommand =
                                            new DelegateCommand(
                                            o => Logger.Info("User decided not to delete game template"))
                                    });
                GameTemplateStorageManager.Instance.Delete(Id);
                Parent.GameTemplateList.Remove(this);
            }
        }
    }
}