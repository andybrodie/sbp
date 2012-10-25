namespace Locima.SlidingBlock.IO
{

    /// <summary>
    /// All storage managers (objects which manage the persistence of data) implement this interface.
    /// </summary>
    /// <remarks>
    /// This is a base interface only used by other interfaces (e.g. <see cref="IHighscoresStorageManager"/>, <see cref="ISaveGameStorageManager"/>, etc.</remarks>
    public interface IStorageManager
    {
        /// <summary>
        ///   Initialise this storage manager, called by <see cref="StorageManagerManager.Initialise" />.  This method will only ever be invoked once per application instance.
        /// </summary>
        void Initialise();
    }
}