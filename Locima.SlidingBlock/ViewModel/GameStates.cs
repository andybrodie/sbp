namespace Locima.SlidingBlock.ViewModel
{
    /// <summary>
    /// The various states that a game can be in, with respect to the game
    /// </summary>
    public enum GameStates
    {
        /// <summary>
        /// This level hasn't been started yet.  This causes the start screen to be displayed until the user taps the puzzle
        /// </summary>
        NotStarted, 

        /// <summary>
        /// The puzzle is running, so the tiles move on taps and the stopwatch is running
        /// </summary>
        Running, 

        /// <summary>
        /// The game is paused, so show the pause screen and pause the stopwatch
        /// </summary>
        Paused,

        /// <summary>
        /// The level has been completed, so show all the tiles so the user can see what they've achieved!
        /// </summary>
        Completed
    }
}