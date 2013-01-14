namespace Locima.SlidingBlock.GameTemplates
{
    /// <summary>
    /// Used for programatically creating <see cref="GameTemplate"/> instances. 
    /// </summary>
    /// <remarks>
    /// These are used for the built-in game templates</remarks>
    public interface IGameTemplateFactory
    {

        /// <summary>
        /// Retrieve the persisted identity of this game template
        /// </summary>
        /// <remarks>This is used to ensure that 1 instance of this game template exists whenever the application is started.  Game Templates are
        /// created on the first run of the application</remarks>
        string PersistentId { get; }

        /// <summary>
        /// Creates a <see cref="GameTemplate"/> instance
        /// </summary>
        /// <returns></returns>
        GameTemplate Create();

    }
}