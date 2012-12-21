using Locima.SlidingBlock.ViewModel;

namespace Locima.SlidingBlock.Messaging
{
    internal class GameStateChangeMessageArgs : MessageArgs
    {
        public GameStates GameState;
    }
}