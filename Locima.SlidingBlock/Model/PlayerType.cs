namespace Locima.SlidingBlock.Model
{

    /// <summary>
    /// Currently unused, this will be used for multiplayer games to indicate in future whether a play is playing on a local device or remote device
    /// </summary>
    public enum PlayerType
    {
        /// <summary>
        /// The player is using the local device
        /// </summary>
        Local = 0, 
        /// <summary>
        /// The player is on a remote device
        /// </summary>
        Remote = 1
    }
}
