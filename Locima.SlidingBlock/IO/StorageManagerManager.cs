namespace Locima.SlidingBlock.IO
{
    /// <summary>
    ///   Manages the storage manager's initialisation.  This method is invoked during application initialisation (i.e. in the <see
    ///    cref="App" /> constructor's <see cref="App.InitializePhoneApplication" />
    ///   utility method)
    /// </summary>
    public class StorageManagerManager
    {
        /// <summary>
        ///   Initialise the individual storage managers.
        /// </summary>
        /// <remarks>
        ///   Initialisation gives the storage managers a chance to do any set up required before the application gets going, for example, creating required directories or files,
        ///   or initialisation databases
        /// </remarks>
        public static void Initialise()
        {
            SaveGameStorageManager.Instance.Initialise();
            PlayerStorageManager.Instance.Initialise();
            ImageStorageManager.Instance.Initialise();
            HighScoresStorageManager.Instance.Initialise();
            GameDefinitionStorageManager.Instance.Initialise();
        }
    }
}